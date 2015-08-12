using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace NuSysApp
{
    class NetworkConnector
    {
        private string _UDPPort = "2156";
        private string _TCPInputPort = "302";
        private List<Tuple<DatagramSocket, DataWriter>> _UDPOutSockets;
        public NetworkConnector()
        {
            this.Init();
        }
        private async void Init()
        {
            _UDPOutSockets = new List<Tuple<DatagramSocket, DataWriter>>();
            foreach (string ip in GetOtherIPs())
            {
                this.AddUDPSocket(ip);
            }

            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += this.TCPConnectionRecieved;
            await listener.BindEndpointAsync(new HostName(this.LocalIPAddress()), _TCPInputPort);

            DatagramSocket socket = new DatagramSocket();
            socket.MessageReceived += this.DatagramMessageRecieved;
            socket.ConnectAsync(new HostName(this.LocalIPAddress()), _UDPPort);
            this.sendMassUDPMessage("this is a test");
            Debug.WriteLine("done");
        }
        private async void TCPConnectionRecieved(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            string ip = args.Socket.Information.LocalAddress.RawName;
            string message;
            try
            {
                uint fieldCount = await reader.LoadAsync(sizeof (uint));
                if (fieldCount != sizeof (uint))
                {
                    Debug.WriteLine("TCP connection recieved at IP "+this.LocalIPAddress()+" but socket closed before full stream was read");
                    return;
                }
                uint stringLength = reader.ReadUInt32();
                uint actualLength = await reader.LoadAsync(stringLength);
                message = reader.ReadString(actualLength);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception caught during TCP connection recieve at IP " + this.LocalIPAddress() + " with error code: " + e.Message);
                return;
            }
            Debug.WriteLine("TCP connection recieve at IP " + this.LocalIPAddress() + " with message: " + message);
        }

        private List<string> GetOtherIPs()
        {
            List<string> ips = new List<string>();
            ips.Add("10.38.22.71");
            ips.Add("10.38.22.74");
            return ips;//TODO add in Phil's php script
        }

        public string LocalIPAddress()
        {
            HostName localHostName = NetworkInformation.GetHostNames().FirstOrDefault(h =>
                    h.IPInformation != null &&
                    h.IPInformation.NetworkAdapter != null);
            return localHostName.RawName;
        }

        private void AddUDPSocket(string ip)
        {
            DatagramSocket socket = new DatagramSocket();
            socket.ConnectAsync(new HostName(ip), _UDPPort);
            DataWriter writer =  new DataWriter(socket.OutputStream);
            _UDPOutSockets.Add(new Tuple<DatagramSocket,DataWriter>(socket,writer));
        }
        private async void DatagramMessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string ip = sender.Information.LocalAddress.RawName;
            string message;
            try
            {
                var result = args.GetDataStream();
                var resultStream = result.AsStreamForRead(1024);

                using (var reader = new StreamReader(resultStream))
                {
                    message = await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught during message recieve at IP "+this.LocalIPAddress()+" with error code: "+e.Message);
                return;
            }
            Debug.WriteLine("UDP packet recieve at IP " + this.LocalIPAddress() + " with message: " + message);
        }
        public async void SendTCPMessage(string message, string recievingIP, string outport)
        {
            try
            {
                StreamSocket socket = new StreamSocket();
                await socket.ConnectAsync(new HostName(recievingIP), outport);
                DataWriter writer = new DataWriter(socket.OutputStream);
                writer.WriteUInt32(writer.MeasureString(message));
                writer.WriteString(message);
                await writer.StoreAsync();
                writer.Dispose();
                socket.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught during TCP message send at IP " + this.LocalIPAddress() + " with error code: " + e.Message);
                return;
            }
        }

        public async Task sendMassUDPMessage(string message)
        {
            foreach (Tuple<DatagramSocket,DataWriter> tup in this._UDPOutSockets)
            {
                await this.SendUDPMessage(message, tup.Item1, tup.Item2);
            }
        }
        private async Task SendUDPMessage(string message, DatagramSocket socket, DataWriter writer)
        {
            writer.WriteString(message);
            await writer.StoreAsync();
        }
    }
}
