using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;


namespace NuSysApp
{
    public class NetworkConnector
    {
        private string _UDPPort = "2156";
        private string _TCPInputPort = "302";
        private string _TCPOutputPort = "302";
        private HashSet<Tuple<DatagramSocket, DataWriter>> _UDPOutSockets; //the set of all UDP output sockets and the writers that send their data
        private ConcurrentDictionary<string, Tuple<bool, List<Packet>>> _joiningMembers; //the dictionary of members in the loading process.  HOST ONLY
        private HashSet<string> _otherIPs;//the set of all other IP's currently known about
        private string _hostIP;
        private string _localIP;
        private DatagramSocket _UDPsocket;
        private StreamSocketListener _TCPlistener;
        private WorkSpaceModel _workSpaceModel;
        private Dictionary<string, DataWriter> _addressToWriter; //A Dictionary of UDP socket writers that correspond to IP's
        private Dictionary<string,string> _locksOut;//The hashset of locks currently given out.  the first string is the id number, the second string represents the IP that holds its lock
        private HashSet<string> _localLocks;
        private bool _caughtUp = false;
        public void Start()
        {
            Debug.WriteLine("Starting Network Connection");
        }

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
            public Packet(string message, PacketType type)//set all the params
            {
                _message = message;
                _type = type;
            }

            public string Message
            {
                get { return _message; } 
            }

            /*
            *send message by passing in an address
            */
            public async Task send(string address)//and send later on
            {
                switch (_type)
                {
                    case PacketType.TCP:
                        await Globals.Network.SendTCPMessage(_message, address);
                        break;
                    case PacketType.UDP:
                        await Globals.Network.SendUDPMessage(_message, address);
                        break;
                }
            }
        }

        public bool isReady()
        {
            return _caughtUp;
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
            _joiningMembers = new ConcurrentDictionary<string, Tuple<bool, List<Packet>>>();
            _addressToWriter = new Dictionary<string,DataWriter>();
            _UDPOutSockets = new HashSet<Tuple<DatagramSocket, DataWriter>>();
            _otherIPs = new HashSet<string>();
            _localLocks = new HashSet<string>();

            List<string> ips = GetOtherIPs();
            if (ips.Count == 1)
            {
                this.makeHost();
            }
            else
            {
                foreach (string ip in ips)
                {
                    await this.AddIP(ip);
                }
            }

            _TCPlistener = new StreamSocketListener();
            _TCPlistener.ConnectionReceived += this.TCPConnectionRecieved;
            await _TCPlistener.BindEndpointAsync(new HostName(this._localIP), _TCPInputPort);

            _UDPsocket = new DatagramSocket();
            await _UDPsocket.BindServiceNameAsync(_UDPPort);
            _UDPsocket.MessageReceived += this.DatagramMessageRecieved;
            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);
        }

        /*
        * this will make  
        */
        private void makeHost()
        {
            _hostIP = _localIP;
            _locksOut = new Dictionary<string, string>();
            _joiningMembers = new ConcurrentDictionary<string, Tuple<bool, List<Packet>>>();
            _caughtUp = true;
            Debug.WriteLine("This machine (IP: "+_localIP+") set to be the host");
            //ToDo add in other host responsibilities
        }

        public WorkSpaceModel WorkSpaceModel
        {
            set { _workSpaceModel = value; }
            get { return _workSpaceModel; }
        }
        public string LocalIP
        {
            get { return _localIP; }
        }
        private async Task AddIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP) 
            {
                _otherIPs.Add(ip);
                await AddSocket(ip);
            }
        }

        public async Task Disconnect()
        {
            string URL = "http://aint.ch/nusys/clients.php";
            string urlParameters = "?action=remove&ip=" + NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null && h.IPInformation.NetworkAdapter != null).RawName;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            this.TellRemoveIP(this.LocalIP);
        }
        private async Task TellRemoveIP(string ip)
        {
            if (_otherIPs.Count > 0)
            {
                if (_localIP == _hostIP)
                {
                    //TODO tell everyone to stop actions and wait
                    await SendMassTCPMessage("SPECIAL1:" + _otherIPs.ToArray()[0]);
                }
                await SendMassTCPMessage("SPECIAL9:" + _localIP);
            }
        }
        private async Task RemoveIP(string ip)
        {
            if (_otherIPs.Contains(ip))
            {
                _otherIPs.Remove(ip);
            }
            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter.Remove(ip);
            }
            foreach (Tuple<DatagramSocket,DataWriter> tup in _UDPOutSockets)
            {
                if (tup.Item1.Information.RemoteAddress.RawName == ip)
                {
                    _UDPOutSockets.Remove(tup);
                    break;
                }
            }
            HashSet<KeyValuePair<string,string>> set = new HashSet<KeyValuePair<string, string>>();
            if (_locksOut.ContainsValue(ip))
            {
                foreach (KeyValuePair<string, string> kvp in _locksOut)
                {
                    if (kvp.Value == ip)
                    {
                        set.Add(kvp);
                    }
                }
            }
            foreach (KeyValuePair<string, string> kvp in set)
            {
                _locksOut.Remove(kvp.Key);
            }
            Tuple<bool, List<Packet>> items;
            if (_joiningMembers.ContainsKey(ip))
            {
                _joiningMembers.TryRemove(ip, out items);
            }
            Debug.WriteLine("Removed IP: "+ip+".  List now is: "+_otherIPs.ToString());
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
                    await RemoveIP(ip);
                    await SendMassTCPMessage("SPECIAL9:" + ip);
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

        private string GetID(string senderIP)
        {
            string hash = senderIP.Replace(@".", "") + "#";
            string now = DateTime.UtcNow.Ticks.ToString();
            return hash + now;
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

            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = UDPwriter;
            }
            else
            {
                _addressToWriter.Add(ip, UDPwriter);
            }
        }
        private async void DatagramMessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string ip = args.RemoteAddress.RawName;
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
                Debug.WriteLine("Exception caught during message recieve FROM IP " + ip + " with error code: " +
                                e.Message);
                return;
            }
            //Debug.WriteLine("UDP packet recieve FROM IP " + ip + " with message: " + message);
            await this.MessageRecieved(ip, message, PacketType.UDP);
        }

        public async Task SendTCPMessage(string message, string recievingIP)
        {
            await SendTCPMessage(message, recievingIP, _TCPOutputPort);
        }
        public async Task SendTCPMessage(string message, string recievingIP, string outport)
        {
            Debug.WriteLine("attempting to send TCP message: "+message+" to IP: "+recievingIP);
            try
            {
                StreamSocket TCPsocket = new StreamSocket();
                await TCPsocket.ConnectAsync(new HostName(recievingIP), _TCPOutputPort);
                DataWriter writer = new DataWriter(TCPsocket.OutputStream);


                //DataWriter writer = _addressToWriter[recievingIP].Item2;
                writer.WriteUInt32(writer.MeasureString(message));
                writer.WriteString(message);
                await writer.StoreAsync();
                writer.Dispose();
                TCPsocket.Dispose();
                Debug.WriteLine("Sent TCP message: " + message + " to IP: " + recievingIP);
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
            await this.SendUDPMessage(message, _addressToWriter[ip]);
        }
        public async Task SendUDPMessage(string message, DataWriter writer)
        {
            //Debug.WriteLine("attempting to send UDP message: " + message);
            writer.WriteString(message);
            await writer.StoreAsync();
            //Debug.WriteLine("Sent UDP message: " + message);
        }
        private async Task MessageRecieved(string ip, string message, PacketType packetType)
        {
            if (message.Length > 0)
            {
                if (message.Substring(0, 7) != "SPECIAL")
                {
                    string[] miniStrings = message.Split("&&".ToCharArray());
                    foreach (string subMessage in miniStrings)
                    {
                        if (subMessage.Length > 0)
                        {
                            await this.HandleSubMessage(ip, subMessage, packetType);
                        }
                    }
                }
                else
                {
                    await this.HandleSubMessage(ip, message, packetType);
                }
            }
            else
            {
                Debug.WriteLine("Recieved message of length 0 from IP: "+ip);
            }
            return;

        }
        private async Task SendUpdateForNode(string nodeId, string sendToIP)
        {
            //TODO make this method get the current status of node nodeId and send full TCP update to IP adress sendToIP
        }

        public async Task SendMessage(string ip, string message, PacketType packetType)
        {
            await SendMessage(ip, message, packetType, false);
        }

        public async Task SendMessageToHost(string message, PacketType packetType = PacketType.TCP)
        {
            if (_localIP == _hostIP)
            {
                await MessageRecieved(_localIP, message, packetType);
                return;
            }
            await SendMessage(_hostIP, message, packetType, false);
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
                    await this.AddIP(message);
                    if (_hostIP != null)
                    {
                        await this.SendTCPMessage("SPECIAL1:" + _hostIP, ip);
                    }
                    if (_hostIP == _localIP && message != _localIP) ;
                    {
                        if (_joiningMembers.TryAdd(message, new Tuple<bool, List<Packet>>(false, new List<Packet>())))
                            //add new joining member
                        {
                            string m = _workSpaceModel.GetFullWorkspace();
                            if (m.Length > 0)
                            {
                                await SendTCPMessage("SPECIAL2:" + m, ip);
                            }
                            else
                            {
                                await SendTCPMessage("SPECIAL4:0",ip);
                            }
                            return;
                        }
                        else
                        {
                            Debug.WriteLine("Adding of joining member failed concurrency");
                            await SendTCPMessage("SPECIAL2:FAIL", ip);
                            await HandleSpecialMessage(_localIP,"SPECIAL9:" + ip,PacketType.TCP);
                            await SendMassTCPMessage("SPECIAL9:" + ip);
                            return;
                        }
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
                        return;
                    }
                    break;
                case "2"://the message sent from host to tell other workspace to catch up.  message is formatted just like regular messages
                    if (_localIP == _hostIP)
                    {
                        Debug.WriteLine("ERROR: host (this) recieved catch up message from IP: "+ip+".  This shouldn't happen EVER");
                        return;
                    }
                    await MessageRecieved(ip, message, packetType);
                    await SendMessageToHost("SPECIAL3:DONE");
                    return;
                    break;
                case "3":// response to catch up = "I am done catching up" ex: message = "DONE"
                    if (_localIP == _hostIP)
                    {
                        if (message == "DONE")
                        {
                            if (_joiningMembers.ContainsKey(ip))
                            {
                                if (_joiningMembers[ip].Item1)
                                {
                                    string ret = "";
                                    foreach (Packet p in _joiningMembers[ip].Item2)
                                    {
                                        ret += p.Message+"&&";
                                    }
                                    ret = ret.Substring(0, ret.Length - 2);
                                    await SendTCPMessage("SPECIAL2:" + ret,ip);
                                    _joiningMembers[ip].Item2.Clear();
                                    return;
                                }
                                else
                                {
                                    await SendTCPMessage("SPECIAL4:" + _joiningMembers[ip].Item2.Count, ip);
                                    foreach (Packet p in _joiningMembers[ip].Item2)
                                    {
                                        await p.send(ip);
                                    }
                                    Tuple<bool, List<Packet>> nothing;
                                    if (_joiningMembers.TryRemove(ip, out nothing))//remove the joining member
                                    {
                                        nothing = null; //seems so weird writing this
                                    }
                                    else
                                    {
                                        Debug.WriteLine("ERROR: Failed to remove joining member after they caught up.  probably concurrency error");
                                        return;
                                    }

                                    return;
                                }
                            }
                            else
                            {
                                Debug.WriteLine("ERROR: The host recieved caught-up message from somebody who wasn't known to be joining.  from IP: "+ip);
                                return;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("ERROR: Non-host recieved 'caught-up' message from IP: " + ip + ".  This shouldn't happen EVER");
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ERROR: Non-host recieved 'caught-up with what you wanted' message from IP: "+ip+ ".  This shouldn't happen EVER");
                        return;
                    }
                    break;
                case "4": //Sent by Host only, "you are caught up and ready to join". message is simply the number of catch-up UDP packets also being sent
                    if (_localIP == _hostIP)
                    {
                        Debug.WriteLine("ERROR: Host recieved 'caught-up' message from IP: " + ip + ".  This shouldn't happen EVER");
                        return;
                    }
                    this._caughtUp = true;
                    Debug.WriteLine("Ready to Join Workspace");
                    return;
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
                case "9"://Tell others to remove IP from self ex: message = "10.10.10.10"
                    RemoveIP(message);
                    break;
                case "10":
                    if (_workSpaceModel.HasAtom(message))
                    {
                        if (_hostIP == _localIP)
                        {
                            await SendMassTCPMessage("SPECIAL10:" + message);
                        }
                        _workSpaceModel.RemoveNode(message);
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("ERROR: delete requested for item that didn't exist.  Item requested for delete: "+message);
                        return;
                    }

                    break;
            }
        }

        public async Task RequestDeleteNode(string id)
        {
            await SendMessageToHost("SPECIAL10:" + id);
        }
        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
        {
            if (_hostIP == _localIP)//this HOST ONLY block is to special case for the host getting a 'make-node' request
            {
                if (message.IndexOf("id=0,") != -1)
                {
                    message = message.Replace(@"id=0,", "id=" + GetID(ip) + ',');
                    await HandleRegularMessage(ip, message, packetType);
                    await SendMassTCPMessage(message);
                    return;
                }
                if (message.IndexOf("id=0>") != -1)
                {
                    message = message.Replace(@"id=0>", "id=" + GetID(ip) + '>');
                    await HandleRegularMessage(ip, message, packetType);
                    await SendMassTCPMessage(message);
                    return;
                }
            }
            if (_localIP == _hostIP)
            {
                foreach (KeyValuePair<string, Tuple<bool, List<Packet>>>  kvp in _joiningMembers)
                    // keeps track of messages sent durig initial loading into workspace
                {
                    kvp.Value.Item2.Add(new Packet(message, packetType));
                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
                    {
                        Tuple<bool, List<Packet>> tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
                        _joiningMembers[kvp.Key] = tup;
                    }
                }
            }
            if (message[0] == '<' && message[message.Length - 1] == '>')
            {
                _workSpaceModel.HandleMessage(message);
            }


            //Debug.WriteLine(_localIP + " handled message: " + message);
        }

        private Dictionary<string, string> ParseOutProperties(string message)
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
    }
}
