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
        private HashSet<Tuple<StreamSocket, DataWriter>> _TCPOutSockets; //the set of all UDP output sockets and the writers that send their data
        private Dictionary<string, Tuple<bool, List<Packet>>> _joiningMembers; //the dictionary of members in the loading process.  HOST ONLY
        private HashSet<string> _otherIPs;//the set of all other IP's currently known about
        private string _hostIP;
        private string _localIP;
        private WorkspaceViewModel _workspaceViewModel;
        private Dictionary<string, Tuple<DataWriter,DataWriter>> _addressToWriter; //A Dictionary of UDP socket writers that correspond to IP's
        private Dictionary<string, StreamSocket> _addressToStreamSockets;  
        private Dictionary<string,string> _locksOut;//The hashset of locks currently given out.  the first string is the id number, the second string represents the IP that holds its lock


        private int _id = 5;//COMPLETELY TEMPORARY FOR TESTING PURPOSES

        public enum PacketType
        {
            UDP,
            TCP,
            Both
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

            _locksOut = new Dictionary<string, string>();
            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();
            _addressToWriter = new Dictionary<string, Tuple<DataWriter, DataWriter>>();
            _UDPOutSockets = new HashSet<Tuple<DatagramSocket, DataWriter>>();
            _addressToStreamSockets = new Dictionary<string, StreamSocket>();
            _TCPOutSockets = new HashSet<Tuple<StreamSocket, DataWriter>>();
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
                    await this.addIP(ip);
                }
            }

            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += this.TCPConnectionRecieved;
            await listener.BindEndpointAsync(new HostName(this._localIP), _TCPInputPort);
            DatagramSocket socket = new DatagramSocket();
            socket.BindServiceNameAsync(_UDPPort);
            socket.MessageReceived += this.DatagramMessageRecieved;
            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);
            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);
            Debug.WriteLine("done");
        }

        /*
        * this will make  
        */
        private void makeHost()
        {
            _hostIP = _localIP;
            _locksOut = new Dictionary<string, string>();
            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();

            Debug.WriteLine("This machine (IP: "+_localIP+") set to be the host");
            //ToDo add in other host responsibilities
        }

        public WorkspaceViewModel WorkspaceViewModel
        {
            get { return _workspaceViewModel; }
            set
            {
                _workspaceViewModel = value; 
                _workspaceViewModel.setNetworkConnector(this);
            }
        }
        private async Task addIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP) 
            {
                _otherIPs.Add(ip);
                await AddSocket(ip);
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

        private async Task AddSocket(string ip)
        {
            DatagramSocket UDPsocket = new DatagramSocket();
            UDPsocket.ConnectAsync(new HostName(ip), _UDPPort);
            DataWriter UDPwriter = new DataWriter(UDPsocket.OutputStream);
            _UDPOutSockets.Add(new Tuple<DatagramSocket, DataWriter>(UDPsocket, UDPwriter));

            StreamSocket TCPsocket = new StreamSocket();
            await TCPsocket.ConnectAsync(new HostName(ip), _TCPOutputPort);
            DataWriter TCPwriter = new DataWriter(TCPsocket.OutputStream);
            _TCPOutSockets.Add(new Tuple<StreamSocket, DataWriter>(TCPsocket, TCPwriter));
            _addressToStreamSockets.Add(ip,TCPsocket);

            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = new Tuple<DataWriter, DataWriter>(UDPwriter,TCPwriter);
            }
            else
            {
                _addressToWriter.Add(ip, new Tuple<DataWriter, DataWriter>(UDPwriter, TCPwriter));
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
            Debug.Write("attempting to TCP send message: "+message+" to IP: "+recievingIP);
            try
            {
                DataWriter writer = new DataWriter(_addressToStreamSockets[recievingIP].OutputStream);
                writer.WriteUInt32(writer.MeasureString(message));
                writer.WriteString(message);
                await writer.StoreAsync();
                writer.DetachStream();
                writer.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught during TCP message send TO IP " + recievingIP + " with error code: " + e.Message);
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
            await this.SendUDPMessage(message, _addressToWriter[ip].Item1);
        }
        public async Task SendUDPMessage(string message, DataWriter writer)
        {
            Debug.Write("attempting to UDP send message: " + message);
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
        private async Task SendUpdateForNode(string nodeId, string sendToIP)
        {
            //TODO make this method get the current status of node nodeId and send full TCP update to IP adress sendToIP
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
                        await SendUDPMessage(message, _addressToWriter[ip].Item1);
                        return;
                    }
                    break;
                case PacketType.Both:
                    await this.SendMessage(ip, message, PacketType.UDP);
                    await this.SendMessage(ip, message, PacketType.TCP);
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
                case "0"://inital request = "I'm joining with my IP, who's the host?"
                    await this.addIP(message);
                    if (_hostIP != null)
                    {
                        await this.SendTCPMessage("SPECIAL1:" + _hostIP, ip);
                    }
                    if (_hostIP == _localIP)
                    {
                        _joiningMembers.Add(message,new Tuple<bool,List<Packet>>(false,new List<Packet>()));//add new joining member
                    }
                    break;
                case "1":// response to initial request = "The host is the following person" ex: message = "10.10.10.10"
                    if (_hostIP != message)
                    {
                        _hostIP = message;
                        if (message == _localIP)
                        {
                            this.makeHost();
                        }
                        Debug.WriteLine("Host returned and SET to be: "+message);
                    }
                    break;
                case "2":

                    break;
                case "3":

                    break;
                case "4":

                    break;
                case "5"://HOST ONLY  request from someone to checkout a lock = "may I have a lock for the following id number" ex: message = "6"
                    if (_hostIP == _localIP)
                    {
                        if (true)//TODO make into 'this contains an object with key number: message'
                        {
                            if (!_locksOut.ContainsKey(message))
                            {
                                _locksOut.Add(message,ip);
                            }
                            await SendMessage(ip, "SPECIAL6:" +message+"="+ _locksOut[message], packetType);
                            return;
                        }
                        else
                        {
                            Debug.WriteLine("ERROR: Recieved a request for a lock for id: " + message +
                                        " which is an invalid id");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ERROR: Recieved a request for a lock for id: " + message +
                                        " when this machine isn't the Host");
                        return;
                    }
                    break;
                case "6"://Response from Lock get request = "the id number has a lock holder of the following IP"  ex: message = "6=10.10.10.10"
                    var parts = message.Split("=".ToCharArray());
                    if (parts.Length != 2)
                    {
                        Debug.WriteLine("Recieved Lock request response that was incorrectly formatted.  message: "+message);
                        return;
                    }
                    string lockId = parts[0];
                    string lockHolder = parts[1];
                    if (false)//TODO make into 'this does not contain an object with key number: lockId'
                    {
                        Debug.WriteLine("ERROR: Recieved a response from lock request with message: " + message +
                                        " which has an invalid id");
                        return;
                    }
                    if (lockHolder != _localIP)
                    {
                        //TODO Cancel movement of node
                        //then
                        SendMessage(_hostIP, "SPECIAL8:" + lockId, PacketType.TCP);
                        return;
                    }
                    return;
                    break;
                case "7"://Returning lock  ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (_locksOut.ContainsKey(message))
                        {
                            _locksOut.Remove(message);
                        }
                        else
                        {
                            Debug.WriteLine("Recieved a return lock request with message: " + message +
                                        " when the lock wasnt out"); 
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ERROR: Recieved a return lock request with message: " + message +
                                        " when this machine isn't the Host");
                        return;
                    }
                    break;
                case "8"://Request full node update for certain id -- HOST ONLY ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (false) //TODO make into 'this does not contain an object with key number: message'
                        {
                            Debug.WriteLine("ERROR: Recieved a request for a node update with: " + message +
                                            " which has an invalid id");
                            return;
                        }
                        else
                        {
                            await this.SendUpdateForNode(message, ip);
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ERROR: Recieved a request for a full node update with message: " + message +
                                        " when this machine isn't the Host");
                        return;
                    }
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
            Dictionary<string, string> properties = parseOutProperties(message);
            if (properties.ContainsKey("id"))
            {
                int id = Int32.Parse(properties["id"]);
                if (id == 0 && _localIP == _hostIP) //if unID'd node
                {
                    id = _id++;
                    properties["id"] = id.ToString();
                        //TODO send mass TCP message to everyone, instantiating a new node
                    await SendMassTCPMessage(MakeSubMessageFromDict(properties));
                }
                _workspaceViewModel.moveNode(properties);
            }
            else
            {
                Debug.WriteLine("ERROR: properties of message didn't contain ID.  message: "+message);
                return;
            }


            Debug.WriteLine(_localIP + " handled message: " + message);
        }

        private Dictionary<string, string> parseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(",".ToCharArray());
            Dictionary<string,string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                string[] subParts = part.Split('=');
                if (subParts.Length != 2)
                {
                    Debug.WriteLine("Error, property formatted wrong in message: " + message);
                    continue;
                }
                props.Add(subParts[0], subParts[1]);
            }
            return props;
        }

        private string MakeSubMessageFromDict(Dictionary<string, string> dict)
        {
            string m = "<";
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                m += kvp.Key + "=" + kvp.Value + ",";
            }
            m = m.Substring(0, m.Length - 1) + ">";
            return m;
        }
        public async void moveNode(string id, int x, int y)//COMPLETELY TEMPORARY FOR TESTING PURPOSES
        {
            Dictionary<string,string> dict = new Dictionary<string, string>();
            dict.Add("id", id);
            dict.Add("x", x.ToString());
            dict.Add("y", y.ToString());
            this.SendMassUDPMessage(this.MakeSubMessageFromDict(dict));
        }

        public async Task<string> makeNode(int x, int y, double width, double height)//COMPLETELY TEMPORARY FOR TESTING PURPOSES
        {
            int id = 0;
            if (_localIP == _hostIP)
            {
                id = _id++;
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("x", x.ToString());
            dict.Add("y", y.ToString());
            dict.Add("width", width.ToString());
            dict.Add("height", height.ToString());
            dict.Add("id", id.ToString());
            string m = this.MakeSubMessageFromDict(dict);

            if (_localIP != _hostIP)
            {
                await SendTCPMessage(m, _hostIP);
            }
            else
            {
                await this.SendMassTCPMessage(m);
            }
            return id.ToString();
        }
    }
}
