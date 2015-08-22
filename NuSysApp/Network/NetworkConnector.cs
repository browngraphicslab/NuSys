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
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Security.Authentication.Web.Provider;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;


namespace NuSysApp
{
    public class NetworkConnector
    {
        #region Private Members
        private const string _UDPPort = "2156";
        private const string _TCPInputPort = "302";
        private const string _TCPOutputPort = "302";
        private HashSet<Tuple<DatagramSocket, DataWriter>> _UDPOutSockets; //the set of all UDP output sockets and the writers that send their data
        private Dictionary<string, Tuple<bool, List<Packet>>> _joiningMembers; //the dictionary of members in the loading process.  HOST ONLY
        private HashSet<string> _otherIPs;//the set of all other IP's currently known about
        private string _hostIP;
        private string _localIP;
        private DispatcherTimer _pingTimer;
        private DatagramSocket _UDPsocket;
        private StreamSocketListener _TCPlistener;
        private Dictionary<string, DataWriter> _addressToWriter; //A Dictionary of UDP socket writers that correspond to IP's
        private bool _caughtUp = false;
        private Dictionary<string, int> _pingResponses;

        private static volatile NetworkConnector _instance;
        private static readonly object _syncRoot = new Object();
        #endregion Private Members

        #region Public Members
        public enum PacketType
        {
            UDP,
            TCP,
            Both
        }

        public static NetworkConnector Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new NetworkConnector();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        private NetworkConnector()//pls keep this private or shit won't work anymore
        {
            this.Init();// Constructor can't contain async code, so it delegates to helper method
        }
   
        /*
        * Returns whether the current connector is caught up with the overall network.  only used in waiting room
        */
        public bool IsReady()//TODO temporary
        {
            return _caughtUp;
        }

        public bool ModelLocked { get; set; }

        /*
         * gets and sets the workspace model that the network connector communicates with
         */
        public ModelIntermediate ModelIntermediate { get;set; }
        /*
        * Essentially an async constructor
        */
        private async void Init()
        {
            _localIP  = NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null 
            && h.IPInformation.NetworkAdapter != null).RawName;

            Debug.WriteLine("local IP: " + _localIP);

            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();
            _addressToWriter = new Dictionary<string,DataWriter>();
            _UDPOutSockets = new HashSet<Tuple<DatagramSocket, DataWriter>>();
            _otherIPs = new HashSet<string>();
            _pingResponses = new Dictionary<string, int>();

            var ips = GetOtherIPs();
            if (ips.Count == 1)
            {
                this.MakeHost();
            }
            else
            {
                foreach (string ip in ips)
                {
                    await this.AddIP(ip);
                }
            }

            //initializes the incoming sockets for TCP and UDP
            _TCPlistener = new StreamSocketListener();
            _TCPlistener.ConnectionReceived += this.TCPConnectionRecieved;
            await _TCPlistener.BindEndpointAsync(new HostName(this._localIP), _TCPInputPort);

            _UDPsocket = new DatagramSocket();
            await _UDPsocket.BindServiceNameAsync(_UDPPort);
            _UDPsocket.MessageReceived += this.DatagramMessageRecieved;
            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);
        }

        /*
        * this will make this network connection the host, forcibly
        */
        private void MakeHost()//TODO temporary
        {
            _hostIP = _localIP;
            _joiningMembers = new Dictionary<string, Tuple<bool, List<Packet>>>();
            _caughtUp = true;
            _pingResponses = new Dictionary<string, int>();
            Debug.WriteLine("This machine (IP: " + _localIP + ") set to be the host");

            //TODO add in other host responsibilities
        }

        /*
        * method called every timer tick (2 seconds for host, 1 seconds for non-host)
        */
        private async void PingTick(object sender, object args)
        {
            var toDelete = new List<string>();
            var keys = _pingResponses.Keys.ToArray();
            foreach (var ip in keys)
            {
                if (_pingResponses[ip] <=2)
                {
                    _pingResponses[ip]++;
                    try
                    {
                        await SendPing(ip, PacketType.UDP);
                    }
                    catch (KeyNotFoundException e)
                    {
                        StartTimer();
                        break;
                    }
                }
                else
                {
                    toDelete.Add(ip);
                }
            }
            foreach (string s in toDelete)
            {
                Debug.WriteLine("IP: " + s + " failed ping twice.  Removing from network");
                await RemoveIP(s);
                if (_hostIP == s)
                {
                    //TODO STOP EVERYONE AND RESET HOST
                    await SendMassTCPMessage("SPECIAL1:" + _localIP);
                    MakeHost();
                }
                await TellRemoveRemoteIP(s);
                await Disconnect(s);
            }
            if (toDelete.Count > 0)
            {
                StartTimer();
            }
        }

        /*
        * method called whenever a ping is recieved
        */
        public void Pingged(string ip)//Did I spell the past-tense of ping incorrectly?
        {
            if (_pingResponses.ContainsKey(ip))
            {
                _pingResponses[ip] = 0;
            }
            else
            {
                Debug.WriteLine("ERROR: Got 'ping' from IP: " + ip + " when there is no such known IP. LOL.");
            }
        }

        /*
        * this sends a ping to the specified IP
        */
        public async Task SendPing(string ip, PacketType packetType)
        {
            await SendMessage(ip, "SPECIAL11:", packetType);
        }

        /*
        * this ends a restarts a timer.  It starts it with the list of people to ping updating according to the current list of network members
        */
        private async Task StartTimer()
        {
            if (_hostIP != null)
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await this.EndTimer();
                    _pingResponses = new Dictionary<string, int>();
                    _pingTimer = new DispatcherTimer();
                    _pingTimer.Tick += PingTick;
                    if (_hostIP == _localIP)
                    {
                        _pingTimer.Interval = new TimeSpan(0, 0, 0, 2);
                        foreach (string ip in _otherIPs)
                        {
                            _pingResponses.Add(ip, 0);
                        }
                    }
                    else
                    {
                        _pingTimer.Interval = new TimeSpan(0, 0, 0, 1);
                        _pingResponses.Add(_hostIP, 0);
                    }
                    _pingTimer.Start();
                });
            }
        }

        /*
        * this ends the ping timer if it is running
        */
        private async Task EndTimer()
        {
            if (_pingTimer != null && _pingTimer.IsEnabled)
            {
                _pingTimer.Stop();
            }   
        }
        public string LocalIP//Returns the local IP
        {
            get { return _localIP; }
        }

        /*
        * adds an IP address to the list of IPs and instantiates the outgoing sockets
        */
        private async Task AddIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP) 
            {
                _otherIPs.Add(ip);
                await AddSocket(ip);
                await StartTimer();
            }
        }

        /*
        * removes this local ip from the php script that keeps track of all the members on the server
        */
        public async Task Disconnect(string ip = null)//called by the closing of the application
        {
            if (ip == null)
            {
                ip = _localIP;
            }
            var URL = "http://aint.ch/nusys/clients.php";
            var urlParameters = "?action=remove&ip=" + ip;
            var client = new HttpClient {BaseAddress = new Uri(URL)};
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(urlParameters).Result;
            if (_localIP == ip)
            {
                this.TellRemoveLocalIP();
            }
        }

        /*
        * to tell everyone else in the network to remove the specified IP from their lists
        */
        private async Task TellRemoveRemoteIP(string ip)
        {
            await SendMassTCPMessage("SPECIAL9:" + ip);
        }

        /*
        * to tell everyone else in the network to remove this local IP from their list of IP's
        */
        private async Task TellRemoveLocalIP()
        {
            if (_otherIPs.Count > 0)
            {
                if (_localIP == _hostIP)
                {
                    //TODO tell everyone to stop actions and wait
                    await SendMassTCPMessage("SPECIAL1:" + _otherIPs.ToArray()[0]);//if this is the host, tell everyone who the new host is because I'M OUT
                }
                await SendMassTCPMessage("SPECIAL9:" + _localIP);//tell everyone else to remove myself from their IP list
            }
        }
        /*
        * Remove an IP from local list of IP's
        */
        private async Task RemoveIP(string ip)
        {
            lock (_otherIPs)
            {
                _otherIPs.Remove(ip); //remove from stright list
                if (_addressToWriter.ContainsKey(ip))
                {
                    _addressToWriter.Remove(ip); //remove the datagram socket data writer
                }
                foreach (var tup in _UDPOutSockets)
                {
                    if (tup.Item1.Information.RemoteAddress.RawName == ip)
                    {
                        _UDPOutSockets.Remove(tup); //remove the outgoing socket
                        break;
                    }
                }
                var set = new HashSet<KeyValuePair<string, string>>(); //create a list of lcoks that need to be removed
                
                ModelIntermediate.RemoveIPFromLocks(ip);
                _pingResponses.Remove(ip);
                if (_joiningMembers.ContainsKey(ip))
                {
                    _joiningMembers.Remove(ip); //if that IP was joining, remove it
                }
                var s = "";
                foreach (string i in _otherIPs)
                {
                    s += i + ", ";
                }
                Debug.WriteLine("Removed IP: " + ip + ".  List now is: " + s);
            }
        }
        /*
        * called when a TCP Connection has been made.  essentially the incoming message method 
        */
        private async void TCPConnectionRecieved(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var reader = new DataReader(args.Socket.InputStream);
            var ip = args.Socket.Information.RemoteAddress.RawName;//get the remote IP address
            string message;
            try
            {
                var fieldCount = await reader.LoadAsync(sizeof (uint));
                if (fieldCount != sizeof (uint))
                {
                    Debug.WriteLine("TCP connection recieved FROM IP "+ip+" but socket closed before full stream was read");
                    await RemoveIP(ip);
                    await SendMassTCPMessage("SPECIAL9:" + ip);
                    return;
                }
                var stringLength = reader.ReadUInt32();
                var actualLength = await reader.LoadAsync(stringLength);//Read the incoming message
                message = reader.ReadString(actualLength);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception caught during TCP connection recieve FROM IP " + ip + " with error code: " + e.Message);
                return;
            }
            Debug.WriteLine("TCP connection recieve FROM IP " + ip + " with message: " + message);
            await this.MessageRecieved(ip,message,PacketType.TCP);//Process the message
        }
        /*
        * a method for creating a new ID
        */
        private string GetID(string senderIP)
        {
            if (_localIP == _hostIP)
            {
                var hash = senderIP.Replace(@".", "") + "#";
                var now = DateTime.UtcNow.Ticks.ToString();
                return hash + now;
            }
            else
            {
                Debug.WriteLine("Non-host tried to make an ID.  Returning a string of 'not#host'");
                return "not#host";
            }
        }
        /*
        * adds self to php script list of IP's 
        * called once at the beginning to get the list of other IP's on the network
        */
        private List<string> GetOtherIPs()
        {
            if (WaitingRoomView.IsLocal)
            {
                List<string> list = new List<string>();
                list.Add(_localIP);
                return list;
            }
            else 
            {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=add&ip=" + _localIP;

                var client = new HttpClient {BaseAddress = new Uri(URL)};

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var people = "";

                var response = client.GetAsync(urlParameters).Result;
                if (response.IsSuccessStatusCode)
                {
                    var d = response.Content.ReadAsStringAsync().Result; //gets the response from the php script
                    people = d;
                }
                else
                {
                    Debug.WriteLine("{0} ({1})", (int) response.StatusCode, response.ReasonPhrase);
                }
                Debug.WriteLine("in workspace: " + people);
                var split = people.Split(",".ToCharArray());

                var ips = split.ToList();
                return ips;
            }
        }
        /*
        * adds a new pair of sockets for a newly joining IP
        */
        private async Task AddSocket(string ip)
        {
            var UDPsocket = new DatagramSocket();
            UDPsocket.ConnectAsync(new HostName(ip), _UDPPort);
            var UDPwriter = new DataWriter(UDPsocket.OutputStream);
            _UDPOutSockets.Add(new Tuple<DatagramSocket, DataWriter>(UDPsocket, UDPwriter));

            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = UDPwriter;//adds the datagram sockets to the dictionary of them
            }
            else
            {
                _addressToWriter.Add(ip, UDPwriter);
            }
        }
        /*
        * the incoming UDP packet method that reads and formats the message
        */
        private async void DatagramMessageRecieved(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
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
                Debug.WriteLine("Exception caught during message recieve FROM IP " + ip + " with error code: " +
                                e.Message);
                return;
            }
            //Debug.WriteLine("UDP packet recieve FROM IP " + ip + " with message: " + message);
            await this.MessageRecieved(ip, message, PacketType.UDP);//process the message
        }
        /*
        * sends a TCP message to specific recieving IP
        */
        private async Task SendTCPMessage(string message, string recievingIP)
        {
            await SendTCPMessage(message, recievingIP, _TCPOutputPort);
        }
        /*
        * Actual method call that every single outgoing TCP goes through.  LOWEST LEVEL SENDER
        */
        private async Task SendTCPMessage(string message, string recievingIP, string outport)
        {
            //Debug.WriteLine("attempting to send TCP message: "+message+" to IP: "+recievingIP);
            try
            {
                var TCPsocket = new StreamSocket();
                await TCPsocket.ConnectAsync(new HostName(recievingIP), outport);
                var writer = new DataWriter(TCPsocket.OutputStream);

                writer.WriteUInt32(writer.MeasureString(message));
                writer.WriteString(message);
                await writer.StoreAsync();//awaiting recieve
                writer.Dispose();//disposes writer and socket after sending message
                TCPsocket.Dispose();
                Debug.WriteLine("Sent TCP message: " + message + " to IP: " + recievingIP);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught during TCP message send TO IP " + recievingIP + " with error code: " + e.Message);
                return;
            }
        }
        /*
        * sends UDP packets to everyone except self.  
        */
        private async Task SendMassUDPMessage(string message)
        {
            foreach (var tup in this._UDPOutSockets)//iterates through everyone
            {
                await this.SendUDPMessage(message, tup.Item2);
            }
        }
        /*
        * sends TCP Streams to everyone except self.  
        */
        private async Task SendMassTCPMessage(string message)
        {
            foreach (var ip in _otherIPs)
            {
                await this.SendTCPMessage(message, ip, _TCPOutputPort);
            }
        }

        /*
        * sends UDP packets to specified IP
        */
        private async Task SendUDPMessage(string message, string ip)
        {
            await this.SendUDPMessage(message, _addressToWriter[ip]);
        }

        /*
        * sends TCP message to specified IP
        */
        private async Task SendUDPMessage(string message, DataWriter writer)
        {
            writer.WriteString(message);
            await writer.StoreAsync();
        }

        /*
        * general message sending method
        */
        private async Task SendMessage(string ip, string message, PacketType packetType)
        {
            await SendMessage(ip, message, packetType, false);
        }
        /*
        * general all-purpose message sending method.  HIGHEST LEVEL SENDER.  can specify anything
        */
        public async Task SendMessage(string ip, string message, PacketType packetType, bool mass, bool self = false)//USE WITH CAUTION
        {
            switch (packetType)
            {
                case PacketType.TCP:
                    if (mass)
                    {
                        await SendMassTCPMessage(message);
                    }
                    else
                    {
                        await SendTCPMessage(message, ip);
                    }
                    break;
                case PacketType.UDP:
                    if (mass)
                    {
                        await SendMassUDPMessage(message);
                    }
                    else
                    {
                        await SendUDPMessage(message, _addressToWriter[ip]);
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
            if (self)
            {
                await MessageRecieved(_localIP, message, packetType);//handle message directly if sending to self
            }
        }

        /*
        * sends message to the host, or the self if self is host.  Auto-property packetType is TCP
        */
        private async Task SendMessageToHost(string message, PacketType packetType = PacketType.TCP)
        {
            if (_localIP == _hostIP)
            {
                await MessageRecieved(_localIP, message, packetType);
                return;
            }
            await SendMessage(_hostIP, message, packetType, false);
        }

        /*
        * general, all-purpose message processing method.  called directly from TCP and UDP message recievings
        */
        private async Task MessageRecieved(string ip, string message, PacketType packetType)
        {
            if (message.Length > 0)//if message exists
            {
                if (message.Substring(0, 7) != "SPECIAL") //if not a special message
                {
                    var miniStrings = message.Split("&&".ToCharArray()); //break up message into subparts
                    foreach (var subMessage in miniStrings)
                    {
                        if (subMessage.Length > 0)
                        {
                            await this.HandleSubMessage(ip, subMessage, packetType); //handle each submessage
                        }
                    }
                }
                else
                {
                    try
                    {
                        await this.HandleSubMessage(ip, message, packetType);
                    }
                    catch (KeyNotFoundException e)
                    {
                        Debug.WriteLine("ERROR: Message recieved tried to access a dictionary when remote IP isn't known");
                        //go back to waiting room or reconnect
                        return;
                    }
                }
            }
            else
            {
                throw new IncorrectFormatException(message);
            }
            return;

        }
        /*
        * handles individual sub-messages
        */
        private async Task HandleSubMessage(string ip, string message, PacketType packetType)
        {
            var type = message.Substring(0, 7);
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

        /*
        * Handles SPECIAL messages.  Read each switch case for specifics
        */
        private async Task HandleSpecialMessage(string ip, string message,PacketType packetType)
        {
            string origMessage = message;
            var indexOfColon = message.IndexOf(":");
            if (indexOfColon == -1)
            {
                throw new IncorrectFormatException(message);
                return;
            }
            var type = message.Substring(0, indexOfColon);
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
                        //_joiningMembers.Add(message, new Tuple<bool, List<Packet>>(false, new List<Packet>()));//add new joining member
                        var m = await ModelIntermediate.GetFullWorkspace();
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
                    break;
                case "1":// response to initial request = "The host is the following person" ex: message = "10.10.10.10"
                    if (_hostIP != message)
                    {
                        _hostIP = message;
                        if (message == _localIP)
                        {
                            this.MakeHost();
                            await SendMassTCPMessage("SPECIAL1:" + _localIP);
                        }
                        await StartTimer();
                        Debug.WriteLine("Host returned and SET to be: "+message);
                        return;
                    }
                    break;
                case "2"://the message sent from host to tell other workspace to catch up.  message is formatted just like regular messages
                    if (_localIP == _hostIP)
                    {
                        throw new HostException(origMessage, ip);
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
                            if (_joiningMembers.ContainsKey(ip) || true)//TODO re-implement joining members and remove this '|| true' statement
                            {
                                if (false && _joiningMembers[ip].Item1)//TODO similiar, remove the "false&&"
                                {
                                    var ret = "";
                                    foreach (var p in _joiningMembers[ip].Item2)
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
                                    await SendTCPMessage("SPECIAL12:" + ModelIntermediate.GetAllLocksToSend(),ip);
                                    //foreach (var p in _joiningMembers[ip].Item2)  TODO similiar above, uncomment this stuff
                                    //{
                                        //await p.Send(ip);
                                    //}
                                    //_joiningMembers.Remove(ip);//remove the joining member
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
                            throw new NotHostException(origMessage, ip);
                            return;
                        }
                    }
                    else
                    {
                        throw new NotHostException(origMessage, ip);
                    }
                    break;
                case "4": //Sent by Host only, "you are caught up and ready to join". message is simply the number of catch-up UDP packets also being sent
                    if (_localIP == _hostIP)
                    {
                        throw new HostException(origMessage, ip);
                        return;
                    }
                    this._caughtUp = true;
                    Debug.WriteLine("Ready to Join Workspace");
                    return;     
                case "5"://HOST ONLY  request from someone to checkout a lock = "may I have a lock for the following id number" ex: message = "6"
                    if (_hostIP == _localIP)
                    {
                        if (!ModelIntermediate.Locks.ContainsKey(message))
                        {
                            ModelIntermediate.Locks.Add(message, ip);
                        }
                        else
                        {
                            ModelIntermediate.Locks[message] = ip;
                        }
                        //await HandleSpecialMessage(_localIP,"SPECIAL6:" + message + "=" + ModelIntermediate.Locks[message],PacketType.TCP);
                        ModelIntermediate.SetAtomLock(message, ModelIntermediate.Locks[message]);
                        await SendMassTCPMessage("SPECIAL6:" + message + "=" + ModelIntermediate.Locks[message]);
                        return;
                    }
                    else
                    {
                        throw new NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "6"://Response from Lock get request = "the id number has a lock holder of the following IP"  ex: message = "6=10.10.10.10"
                    var parts = message.Split("=".ToCharArray());
                    if (parts.Length != 2 && parts.Length != 1)
                    {
                        throw new IncorrectFormatException(origMessage);
                        return;
                    }
                    if (parts.Length == 1)
                    {
                        string[] p = new string[2];
                        p[0] = parts[0];
                        p[1] = "";
                        parts = p;
                    }
                    var lockId = parts[0];
                    var lockHolder = parts[1];
                    ModelIntermediate.SetAtomLock(lockId, lockHolder);
                    return;
                    break;
                case "7"://Returning lock  ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (ModelIntermediate.Locks.ContainsKey(message))
                        {
                            ModelIntermediate.Locks[message] = "";
                        }
                        else
                        {
                            ModelIntermediate.Locks.Add(message, "");
                        }
                        await SendMessage(ip, "SPECIAL6:"+message+"=", PacketType.TCP, true, true);
                        return;
                    }
                    else
                    {
                        throw new NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "8"://Request full node update for certain id -- HOST ONLY ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (!ModelIntermediate.HasAtom(message))
                        {
                            throw new InvalidIDException(message);
                            return;
                        }
                        else
                        {
                            await SendUpdateForNode(message, ip);
                            return;
                        }
                    }
                    else
                    {
                        throw new NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "9"://Tell others to remove IP from self ex: message = "10.10.10.10"
                    RemoveIP(message);
                    break;
                case "10":// Request to delete a node.  HOST ONLY.   ex: message = an ID
                    if (ModelIntermediate.HasAtom(message))
                    {
                        if (_hostIP == _localIP)
                        {
                            await SendMassTCPMessage("SPECIAL10:" + message);
                        }
                        await ModelIntermediate.RemoveNode(message);
                        return;
                    }
                    else
                    {
                        throw new InvalidIDException(message);
                        return;
                    }

                    break;
                case "11"://a simple 'ping'.  Will respond to ping with a 'NO' meaning 'dont reply'.  ex: message = "" or "NO"
                    this.Pingged(ip);
                    if (message != "NO")
                    {
                        await SendMessage(ip, "SPECIAL11:NO", packetType);
                    }
                    break;
                case "12"://A full update from the host about the current locks
                    if (message != "")
                    {
                        await ModelIntermediate.ForceSetLocks(message);
                    }
                    else
                    {
                        throw new IncorrectFormatException(origMessage);
                    }
                    break;
            }
        }


        /*
        * sends TCP update to someone with the entire current state of a node
        */
        private async Task SendUpdateForNode(string nodeId, string sendToIP)
        {
            Dictionary<string, string> dict = await ModelIntermediate.GetNodeState(nodeId);
            if (dict != null)
            {
                string message = MakeSubMessageFromDict(dict);
                await SendTCPMessage(message, sendToIP);
            }
        }


        /*
        * PUBLIC request for deleting a nod 
        */
        public async Task RequestDeleteAtom(string id)
        {
            if (ModelIntermediate.HasLock(id))
            {
                await SendMessageToHost("SPECIAL10:" + id); //tells host to delete the node
            }
        }

        /*
        * handles and proccesses a regular sub-message
        */
        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
        {
            if (_hostIP == _localIP)//this HOST ONLY block is to special case for the host getting a 'make-node' request
            {
                if (message.IndexOf("OLDSQLID") != -1)
                {
                    Dictionary<string, string> dict = ParseOutProperties(message);
                    dict["id"] = dict["OLDSQLID"];
                    dict.Remove("OLDSQLID");
                    string m = MakeSubMessageFromDict(dict);
                    await HandleRegularMessage(ip, m, packetType);
                    await SendMassTCPMessage(m);
                }
                else
                {
                    if (message.IndexOf("id=0" + Constants.CommaReplacement) != -1)
                    {
                        string id = GetID(ip);
                        message = message.Replace(("id=0" + Constants.CommaReplacement),
                            "id=" + id + Constants.CommaReplacement);
                        await HandleRegularMessage(ip, message, packetType);
                        await SendMassTCPMessage(message);
                        return;
                    }
                    if (message.IndexOf("id=0>") != -1)
                    {
                        string id = GetID(ip);
                        message = message.Replace(@"id=0>", "id=" + id + '>');
                        await HandleRegularMessage(ip, message, packetType);
                        await SendMassTCPMessage(message);
                        return;
                    }
                }
            }
            if (_localIP == _hostIP)//if host, add a new packet and store it in every joining member's stack of updates
            {
                foreach (var  kvp in _joiningMembers)
                    // keeps track of messages sent durig initial loading into workspace
                {
                    kvp.Value.Item2.Add(new Packet(message, packetType));
                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
                    {
                        var tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
                        _joiningMembers[kvp.Key] = tup;
                    }
                }
            }
            if (message[0] == '<' && message[message.Length - 1] == '>')
            {
                ModelLocked = true;
                await ModelIntermediate.HandleMessage(message);
                ModelLocked = false;
            }
            else
            {
                throw new IncorrectFormatException(message);
            }
        }

        /*
        * parses message to dictionary of properties.  POSSIBLE DEPRICATED
        */
        private Dictionary<string, string> ParseOutProperties(string message)//TODO check if this can be deleted.  if not, check that it works
        {
            message = message.Substring(1, message.Length - 2);

            var parts = message.Split(Constants.CommaReplacement.ToCharArray());
            var props = new Dictionary<string, string>();
            foreach (var part in parts)

            {
                var subParts = part.Split('=');
                if (subParts.Length != 2)
                {
                    continue;
                }
                props.Add(subParts[0], subParts[1]);
            }
            return props;
        }

        /*
        * makes a message from a dictionary of properties.  dict must have an ID
        */
        private string MakeSubMessageFromDict(Dictionary<string, string> dict)
        {
            var m = "<";
            foreach (var kvp in dict)
            {
                m += kvp.Key + "=" + kvp.Value + Constants.CommaReplacement;
            }
            m = m.Substring(0, Math.Max(m.Length - Constants.CommaReplacement.Length, 0)) + ">";
            return m;
        }

        /*
        * PUBLIC general method to update everyone from an Atom update.  sends mass udp packet
        */
        public async Task QuickUpdateAtom(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("id"))
            {
                if (ModelIntermediate.HasAtom(properties["id"]))
                {
                    string message = MakeSubMessageFromDict(properties);
                    await SendMassUDPMessage(message);
                }
                else
                {
                    throw new InvalidIDException(properties["id"]);
                    return;
                }
            }
            else
            {
                throw new NoIDException();
                return;
            }
        }

        /*
        * PUBLIC general method to create Node
        */
        public async Task RequestMakeNode(string x, string y, string nodeType, string data=null, string oldID = null)
        {
            if (x != "" && y != "" && nodeType != "")
            {
                var s = "";
                if (data != null && data!="null" && data!="")
                {
                    s = Constants.CommaReplacement+"data=" + data;
                }
                if (oldID != null)
                {
                    s += Constants.CommaReplacement + "OLDSQLID=" + oldID;
                }
                await SendMessageToHost("<id=0"+ Constants.CommaReplacement+"x=" + x + Constants.CommaReplacement+"y=" + y + Constants.CommaReplacement+"type=node"+ Constants.CommaReplacement+"nodeType=" + nodeType + s +">");
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }

        /*
        * PUBLIC general method to create Linq
        */
        public async Task RequestMakeLinq(string id1, string id2)
        {
            if (id1 != "" && id2 != "")
            {
                await SendMessageToHost("<id=0"+ Constants.CommaReplacement+"id1=" + id1 + Constants.CommaReplacement+"id2=" + id2 + Constants.CommaReplacement+"type=linq>");
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }

        public async Task RequestLock(string id)
        {
            if (ModelIntermediate.HasAtom(id))
            {
                Debug.WriteLine("Requesting lock for ID: "+id);
            }
            else
            {
                Debug.WriteLine("Requesting lock for ID: "+id+" although it doesn't exist yet");
            }
            await SendMessageToHost("SPECIAL5:" + id, PacketType.TCP);
        }

        public async Task ReturnLock(string id)
        {
            if (ModelIntermediate.HasAtom(id))
            {
                Debug.WriteLine("Returning lock for ID: " + id);
            }
            else
            {
                Debug.WriteLine("Attempted to return lock with ID: "+id+" When no such ID exists");
            }
            await SendMessageToHost("SPECIAL7:" + id);
            await SendMassTCPMessage(MakeSubMessageFromDict(await ModelIntermediate.GetNodeState(id)));
        }

        public class InvalidIDException : System.Exception
        {
            public InvalidIDException(string id) : base(String.Format("The ID {0}  was used but is invalid",id)){}
        }
        public class IncorrectFormatException : System.Exception
        {
            public IncorrectFormatException(string message) : base(String.Format("The message '{0}' is incorrectly formatted or unrecognized",message)) { }
        }

        public class NotHostException : Exception
        {
            public NotHostException(string message, string remoteIP)
                : base(String.Format("The message {0} was sent to a non-host from IP: {1} when it is a host-only message", message, remoteIP)){}
        }

        public class HostException : Exception
        {
            public HostException(string message, string remoteIP) : base(String.Format("The message {0} was sent to this machine, THE HOST, from IP: {1} when it is meant for only non-hosts", message, remoteIP)) { }
        }
        public class UnknownIPException : Exception
        {
            public UnknownIPException(string ip) : base(String.Format("The IP {0} was used when it is not recgonized", ip)) { }
        }

        public class NoIDException : Exception{}
        public class InvalidCreationArgumentsException : Exception { }
        private class Packet //private class to store messages for later
        {
            private readonly PacketType _type;
            public Packet(string message, PacketType type)//set all the params
            {
                Message = message;
                _type = type;
            }

            public string Message { get; }

            /*
            *send message by passing in an address
            */
            public async Task Send(string address)//TODO temporary and send later on 
            {
                switch (_type)
                {
                    case PacketType.TCP:
                        await NetworkConnector.Instance.SendTCPMessage(Message, address);
                        break;
                    case PacketType.UDP:
                        await NetworkConnector.Instance.SendUDPMessage(Message, address);
                        break;
                }
            }
        }
    }
}
