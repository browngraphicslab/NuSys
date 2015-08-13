﻿using System;
using System.Collections;
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
    public class NetworkConnector
    {
        private string _UDPPort = "2156";
        private string _TCPInputPort = "302";
        private string _TCPOutputPort = "1500";
        private HashSet<Tuple<DatagramSocket, DataWriter>> _UDPOutSockets;
        private HashSet<string> _otherIPs;
        private string _hostIP;
        private string _localIP;
        private WorkSpaceModel _workspaceModel;
        private Hashtable _locksOut;

        public NetworkConnector()
        {
            this.Init();
        }
        private async void Init()
        {
            _localIP  = NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null).RawName;
            _UDPOutSockets = new HashSet<Tuple<DatagramSocket, DataWriter>>();
            _otherIPs = new HashSet<string>();
            List<string> ips = GetOtherIPs();
            if (ips.Count == 1)
            {
                this.makeHost();
            }
            else
            {
                foreach (string ip in GetOtherIPs())
                {
                    this.addIP(ip);
                }
            }

            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += this.TCPConnectionRecieved;
            await listener.BindEndpointAsync(new HostName(this._localIP), _TCPInputPort);

            DatagramSocket socket = new DatagramSocket();
            socket.BindServiceNameAsync(_UDPPort);
            socket.MessageReceived += this.DatagramMessageRecieved;

            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);
            Debug.WriteLine("done");
        }

        private void makeHost()
        {
            _hostIP = _localIP;
            
            //ToDo add in other host responsibilities
        }

        public WorkSpaceModel WorkspaceModel
        {
            get { return _workspaceModel; }
            set { _workspaceModel = value; }
        }
        private void addIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP) ;
            {
                _otherIPs.Add(ip);
                AddUDPSocket(ip);
            }
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
                    Debug.WriteLine("TCP connection recieved at IP "+this._localIP+" but socket closed before full stream was read");
                    return;
                }
                uint stringLength = reader.ReadUInt32();
                uint actualLength = await reader.LoadAsync(stringLength);
                message = reader.ReadString(actualLength);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception caught during TCP connection recieve at IP " + this._localIP + " with error code: " + e.Message);
                return;
            }
            Debug.WriteLine("TCP connection recieve at IP " + this._localIP + " with message: " + message);
            this.MessageRecieved(ip,message);
        }

        private List<string> GetOtherIPs()
        {
            List<string> ips = new List<string>();
            ips.Add("10.38.22.71");
            ips.Add("10.38.22.74");
            return ips;//TODO add in Phil's php script
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
                Debug.WriteLine("Exception caught during message recieve at IP "+this._localIP+" with error code: "+e.Message);
                return;
            }
            Debug.WriteLine("UDP packet recieve at IP " + this._localIP + " with message: " + message);
            this.MessageRecieved(ip,message);
        }

        public async Task SendTCPMessage(string message, string recievingIP)
        {
            await SendTCPMessage(message, recievingIP, _TCPOutputPort);
        }
        public async Task SendTCPMessage(string message, string recievingIP, string outport)
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
                Debug.WriteLine("Exception caught during TCP message send at IP " + this._localIP + " with error code: " + e.Message);
                return;
            }
        }

        public async Task SendMassUDPMessage(string message)
        {
            foreach (Tuple<DatagramSocket,DataWriter> tup in this._UDPOutSockets)
            {
                await this.SendUDPMessage(message, tup.Item2);
            }
        }

        public async Task SendMassTCPMessage(string message)
        {
            foreach (string ip in _otherIPs)
            {
                await this.SendTCPMessage(message, ip, _TCPOutputPort);
            }
        }
        public async Task SendUDPMessage(string message, DataWriter writer)
        {
            writer.WriteString(message);
            await writer.StoreAsync();
        }

        private async Task MessageRecieved(string ip, string message)
        {
            string type = message.Substring(0, 7);
            switch (type)//OMG IM SWITCHING ON A STRING
            {
                case "SPECIAL":
                    await this.HandleSpecialMessage(ip,message.Substring(7));
                    break;
                default:
                    await this.HandleRegularMessage(ip,message);
                    break;
            }
        }
        private async Task HandleSpecialMessage(string ip, string message)
        {
            int indexOfColon = message.IndexOf(":");
            if (indexOfColon == -1)
            {
                Debug.WriteLine("ERROR: message recieved was formatted wrong");
                return;
            }
            string type = message.Substring(0, indexOfColon + 1);
            message = message.Substring(indexOfColon + 1);
            switch (message.Substring(0, 1))
            {
                case "0":
                    this.addIP(message);
                    await this.SendTCPMessage("SPECIAL1:" + _hostIP,ip);
                    break;
                case "1":
                    _hostIP = message;
                    this.makeHost();
                    break;
                case "2":

                    break;
                case "3":

                    break;
                case "4":

                    break;
            }
        }

        private async Task HandleRegularMessage(string ip, string message)
        {
            Debug.WriteLine(_localIP + " handled message: "+message);
        }
    }
}
