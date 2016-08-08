using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class NetworkClient
    {
        #region Public Members

        public enum PacketType
        {
            UDP,
            TCP
        }
        public string LocalIP{get { return _localIP; }}

        #region Events

        public delegate void NewMessageEventHandler(string ip, Message message, PacketType packetType);
        public event NewMessageEventHandler OnNewMessage;

        public delegate void TCPDroppedEventHandler(string ip);
        public event TCPDroppedEventHandler OnTCPClientDrop;

        public delegate void UDPDroppedEventHandler(string ip);
        public event UDPDroppedEventHandler OnUDPClientDrop;

        #endregion Events

        #endregion Public Members

        #region Private Members

        private const string _UDPPort = "2156";
        private string _TCPPort = "302";

        private string _localIP;

        private DatagramSocket _UDPlistener;
        private StreamSocketListener _TCPlistener;

        private Dictionary<string, Tuple<DatagramSocket, DataWriter>> _outgoingUdpDictionary;
        private Dictionary<string, Tuple<StreamSocket, DataWriter>> _outgoingTcpDictionary;

        #endregion Private Members

        public async Task Init()
        {
            _localIP =
                NetworkInformation.GetHostNames()
                    .FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null)
                    .RawName;
            _outgoingUdpDictionary = new Dictionary<string, Tuple<DatagramSocket, DataWriter>>();
            _outgoingTcpDictionary = new Dictionary<string, Tuple<StreamSocket, DataWriter>>();

            _TCPlistener = new StreamSocketListener();
            _TCPlistener.ConnectionReceived += TCPConnectionRecieved;
            await _TCPlistener.BindEndpointAsync(new HostName(_localIP), _TCPPort);

            _UDPlistener = new DatagramSocket();
            await _UDPlistener.BindServiceNameAsync(_UDPPort);
            _UDPlistener.MessageReceived += DatagramMessageRecieved;
        }

        private async Task AddIP(string ip)
        {
            var hostName = new HostName(ip);

            DatagramSocket socket = new DatagramSocket();
            await socket.ConnectAsync(hostName, _UDPPort);
            DataWriter writer = new DataWriter(socket.OutputStream);
            _outgoingUdpDictionary[ip] = new Tuple<DatagramSocket, DataWriter>(socket,writer);

            StreamSocket tcpSocket = new StreamSocket();
            await tcpSocket.ConnectAsync(hostName,tcpSocket.ToString());
            DataWriter tcpwriter = new DataWriter(tcpSocket.OutputStream);
            _outgoingTcpDictionary[ip] = new Tuple<StreamSocket, DataWriter>(tcpSocket,tcpwriter);
            Task.Run(async delegate
            {
                var stream = tcpSocket.InputStream;
                var reader = new DataReader(stream);
                while (true)
                {
                    var stringLength = reader.ReadUInt32();
                    var actualLength = await reader.LoadAsync(stringLength);//Read the incoming message
                    var message = reader.ReadString(actualLength);
                    if (!String.IsNullOrEmpty(message))
                    {
                        OnNewMessage?.Invoke(ip,new Message(message),PacketType.TCP);
                    }
                }
            });
            //var stream = tcpSocket.InputStream;
            //stream.ReadAsync().GetResults().
        }

        private string GetSerializedMessage(Message m)
        {
            return m.GetSerialized();
        }
        #region Sending Messages
        public async Task SendTCPMessage(Message message, string ip)
        {
            await SendTCPMessage(GetSerializedMessage(message), ip);
        }
        public async Task SendUDPMessage(Message message, string ip)
        {
            await SendUDPMessage(GetSerializedMessage(message), ip);
        }
        public async Task SendTCPMessage(Message message, ICollection<string> ips)
        {
            var stringMessage = GetSerializedMessage(message);
            foreach (var ip in ips)
            {
                await SendTCPMessage(stringMessage, ip);
            }
        }
        public async Task SendMessage(Message message, string ip, PacketType type)
        {
            switch (type)
            {
                case PacketType.TCP:
                    await SendTCPMessage(message, ip);
                    break;
                case PacketType.UDP:
                    await SendUDPMessage(message, ip);
                    break;
            }
        }
        public async Task SendMessage(Message message, ICollection<string> ips, PacketType type)
        {
            var m = GetSerializedMessage(message);
            foreach (var ip  in ips)
            {
                await SendMessage(m, ip, type);
            }
        }
        public async Task SendUDPMessage(Message message, ICollection<string> ips)
        {
            var stringMessage = GetSerializedMessage(message);
            foreach (var ip in ips)
            {
                await SendUDPMessage(stringMessage, ip);
            }
        }
        private async Task SendMessage(string message, string ip, PacketType type)
        {
            switch (type)
            {
                case PacketType.TCP:
                    await SendTCPMessage(message, ip);
                    break;
                case PacketType.UDP:
                    await SendUDPMessage(message, ip);
                    break;
            }
        }
        private async Task SendTCPMessage(string message, string recievingIP)
        {
            try
            {
                var TCPsocket = new StreamSocket();
                await TCPsocket.ConnectAsync(new HostName(recievingIP), _TCPPort);
                /*
                if (!_outgoingTcpDictionary.ContainsKey(recievingIP))
                {
                    await AddIP(recievingIP);
                }
                var writer = _outgoingTcpDictionary[recievingIP].Item2;
                */
                var writer = new DataWriter(TCPsocket.OutputStream);
                writer.WriteUInt32(writer.MeasureString(message));
                writer.WriteString(message);

                await writer.StoreAsync();//awaiting recieve
                writer.Dispose();
                TCPsocket.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught during TCP message send TO IP " + recievingIP + " with error code: " + e.Message);
                OnTCPClientDrop?.Invoke(recievingIP);
            }
        }

        private async Task SendUDPMessage(string message, string ip)
        {
            DataWriter writer;
            if (!_outgoingUdpDictionary.ContainsKey(ip))
            {
                await AddIP(ip);
            }
            writer = _outgoingUdpDictionary[ip].Item2;
            writer.WriteString(message);
            await writer.StoreAsync();
        }
        #endregion Sending Messages
        #region Recieving Messages
        private async void TCPConnectionRecieved(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            ThreadPool.RunAsync(async delegate {
                var reader = new DataReader(args.Socket.InputStream);
                var ip = args.Socket.Information.RemoteAddress.RawName;//get the remote IP address
                string message;
                try
                {
                    var fieldCount = await reader.LoadAsync(sizeof(uint));
                    if (fieldCount != sizeof(uint))
                    {
                        Debug.WriteLine("TCP connection recieved FROM IP " + ip + " but socket closed before full stream was read");
                        OnTCPClientDrop?.Invoke(ip);
                        return;
                    }
                    var stringLength = reader.ReadUInt32();
                    var actualLength = await reader.LoadAsync(stringLength);//Read the incoming message
                    message = reader.ReadString(actualLength);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception caught during TCP connection recieve FROM IP " + ip + " with error code: " + e.Message);
                    OnTCPClientDrop?.Invoke(ip);
                    return;
                }
                //Debug.WriteLine("TCP connection recieve FROM IP " + ip + " with message: " + message);
                OnNewMessage?.Invoke(ip, new Message(message), PacketType.TCP);
            });
        }
        private async void DatagramMessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            ThreadPool.RunAsync(async delegate
            {
                var ip = args.RemoteAddress.RawName;//get the remote IP
                string message;
                try
                {
                    var result = args.GetDataStream();
                    var resultStream = result.AsStreamForRead(1024);
                    using (var reader = new StreamReader(resultStream))
                    {
                        message = await reader.ReadToEndAsync();//Reads the message to string
                    }
                }
                catch (Exception e)
                {
                    OnUDPClientDrop?.Invoke(ip);
                    return;
                }
                //Debug.WriteLine("UDP packet recieve FROM IP " + ip + " with message: " + message);
                OnNewMessage?.Invoke(ip, new Message(message), PacketType.UDP);
            });
        }
        #endregion Recieving Messages
    }
}
