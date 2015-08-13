using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private string _TCPOutputPort = "302";
        private HashSet<Tuple<DatagramSocket, DataWriter>> _UDPOutSockets; //the set of all UDP output sockets and the writers that send their data
        private Dictionary<string, Tuple<bool, List<Packet>>> _joiningMembers; //the dictionary of members in the loading process.  HOST ONLY
        private HashSet<string> _otherIPs;//the set of all other IP's currently known about
        private string _hostIP;
        private string _localIP;
        private WorkSpaceModel _workspaceModel;
        private Dictionary<string, DataWriter> _addressToWriter; //A Dictionary of UDP socket writers that correspond to IP's
        private HashSet<string> _locksOut;//The hashset of locks currently given out.  the string represents the IP that holds it

        public enum PacketType
        {
            UDP,
            TCP
        }

        private class Packet //private class to store messages for later
        {
            private string _message;
            private PacketType _type;
            private NetworkConnector _network;
            public Packet(NetworkConnector network,string message, PacketType type)//set all the params
            {
                _network = network;
                _message = message;
                _type = type;
            }

            /*
            *send message by passing in an address
            */
            public async void send(string address)//and send later on
            {
                switch (_type)
                {
                    case PacketType.TCP:
                        await _network.SendTCPMessage(_message, address);
                        break;
                    case PacketType.UDP:
                        await _network.SendUDPMessage(_message, address);
                        break;
                }
            }
        }
        public NetworkConnector()
        {
            this.Init();//to call an async method
        }
        private async void Init()
        {
            _localIP  = NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null).RawName;

            Debug.WriteLine("local IP: " + _localIP);

            _locksOut = new HashSet<string>();
            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();
            _addressToWriter = new Dictionary<string, DataWriter>();
            _UDPOutSockets = new HashSet<Tuple<DatagramSocket, DataWriter>>();
            _otherIPs = new HashSet<string>();

            List<string> ips = GetOtherIPs();
            if (ips.Count == 1)
            {
                this.makeHost();
            }
            else
            {
                foreach (string ip in ips)
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

        /*
        * this will make  
        */
        private void makeHost()
        {
            _hostIP = _localIP;
            _locksOut = new HashSet<string>();
            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();

            Debug.WriteLine("This machine (IP: "+_localIP+") set to be the host");
            //ToDo add in other host responsibilities
        }

        public WorkSpaceModel WorkspaceModel
        {
            get { return _workspaceModel; }
            set { _workspaceModel = value; }
        }
        private void addIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP) 
            {
                _otherIPs.Add(ip);
                AddUDPSocket(ip);
            }
        }
        private async void TCPConnectionRecieved(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            string ip = args.Socket.Information.RemoteAddress.RawName;
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
                Debug.WriteLine("Exception caught during TCP connection recieve FROM IP " + ip + " with error code: " + e.Message);
                return;
            }
            Debug.WriteLine("TCP connection recieve FROM IP " + ip + " with message: " + message);
            await this.MessageRecieved(ip,message,PacketType.TCP);
        }

        private List<string> GetOtherIPs()
        {
            string URL = "http://aint.ch/nusys/clients.php";
            string urlParameters = "?action=add&ip="+_localIP;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string people = "";

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                var d = response.Content.ReadAsStringAsync().Result;
                people = d;
            }
            else
            {
                Debug.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            Debug.WriteLine("in workspace: "+people);
            var split = people.Split(",".ToCharArray());

            List<string> ips = split.ToList();
            return ips;//TODO add in Phil's php script
        }

        private void AddUDPSocket(string ip)
        {
            DatagramSocket socket = new DatagramSocket();
            socket.ConnectAsync(new HostName(ip), _UDPPort);
            DataWriter writer =  new DataWriter(socket.OutputStream);
            _UDPOutSockets.Add(new Tuple<DatagramSocket,DataWriter>(socket,writer));
            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = writer;
            }
            else
            {
                _addressToWriter.Add(ip, writer);
            }
        }
        private async void DatagramMessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string ip = sender.Information.RemoteAddress.RawName;
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
                Debug.WriteLine("Exception caught during message recieve FROM IP "+ip+" with error code: "+e.Message);
                return;
            }
            Debug.WriteLine("UDP packet recieve FROM IP " + ip + " with message: " + message);
            await this.MessageRecieved(ip,message,PacketType.UDP);
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

        public async Task SendUDPMessage(string message, string ip)
        {
            await this.SendUDPMessage(message, _addressToWriter[ip]);
        }
        public async Task SendUDPMessage(string message, DataWriter writer)
        {
            writer.WriteString(message);
            await writer.StoreAsync();
        }
        private async Task MessageRecieved(string ip, string message, PacketType packetType)
        {
            string[] miniStrings = message.Split("&&".ToCharArray());
            foreach (string subMessage in miniStrings)
            {
                await this.HandleSubMessage(ip, subMessage, packetType);
            }
            
        }

        public async Task SendMessage(string ip, string message, PacketType packetType)
        {
            await SendMessage(ip, message, packetType, false);
        }
        public async Task SendMessage(string ip, string message, PacketType packetType, bool mass)
        {
            switch (packetType)
            {
                case PacketType.TCP:
                    if (mass)
                    {
                        await SendMassTCPMessage(message);
                        return;
                    }
                    else
                    {
                        await SendTCPMessage(message, ip);
                        return;
                    }
                    break;
                case PacketType.UDP:
                    if (mass)
                    {
                        await SendMassUDPMessage(message);
                        return;
                    }
                    else
                    {
                        await SendUDPMessage(message, _addressToWriter[ip]);
                        return;
                    }
                    break;
                default:
                    Debug.WriteLine("Message send failed because the PacketType was incorrect.  Message: " + message);
                    return;
                    break;
            }
        }

        private async Task HandleSubMessage(string ip, string message, PacketType packetType)
        {
            string type = message.Substring(0, 7);
            switch (type)//OMG IM SWITCHING ON A STRING
            {
                case "SPECIAL":
                    await this.HandleSpecialMessage(ip, message.Substring(7), packetType);
                    break;
                default:
                    await this.HandleRegularMessage(ip, message, packetType);
                    break;
            }
        }
        private async Task HandleSpecialMessage(string ip, string message,PacketType packetType)
        {
            int indexOfColon = message.IndexOf(":");
            if (indexOfColon == -1)
            {
                Debug.WriteLine("ERROR: message recieved was formatted wrong");
                return;
            }
            string type = message.Substring(0, indexOfColon);
            message = message.Substring(indexOfColon+1);
            switch (type)
            {
                case "0":
                    this.addIP(message);
                    await this.SendTCPMessage("SPECIAL1:" + _hostIP,ip);
                    if (_hostIP == _localIP)
                    {
                        _joiningMembers.Add(message,new Tuple<bool,List<Packet>>(false,new List<Packet>()));//add new joining member
                    }
                    break;
                case "1":
                    if (_hostIP != message)
                    {
                        _hostIP = message;
                        if (message == _localIP)
                        {
                            this.makeHost();
                        }
                    }
                    break;
                case "2":

                    break;
                case "3":

                    break;
                case "4":

                    break;
            }
        }

        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
        {
            if (_localIP == _hostIP)
            {
                foreach (KeyValuePair<string, Tuple<bool, List<Packet>>>  kvp in _joiningMembers)
                    // keeps track of messages sent durig initial loading into workspace
                {
                    kvp.Value.Item2.Add(new Packet(this, message, packetType));
                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
                    {
                        Tuple<bool, List<Packet>> tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
                        _joiningMembers[kvp.Key] = tup;
                    }
                }
            }

            Debug.WriteLine(_localIP + " handled message: " + message);
        }
    }
}
