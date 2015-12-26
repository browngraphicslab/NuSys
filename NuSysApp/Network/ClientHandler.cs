using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace NuSysApp.Network
{
    class ClientHandler
    {
        #region Events and Handlers

        public delegate void RemoveIPEventHandler(string ip);
        public event RemoveIPEventHandler OnRemoveIP;

        public delegate void NewMessageEventHandler(string ip, string message, NetworkConnector.PacketType packetType);
        public event NewMessageEventHandler OnNewMessage;

        public delegate void NewIPEventHandler(string ip);
        public event NewIPEventHandler OnNewIP;

        public delegate void AllLocksSetHandler(string message);
        public event AllLocksSetHandler OnAllLocksSet;

        public delegate void LockUpdateRecievedHandler(string lockID, string lockholder);
        public event LockUpdateRecievedHandler OnLockUpdateRecieved;

        public delegate void DeleteRequestRecievedHandler(string id);
        public event DeleteRequestRecievedHandler OnDeleteRequestRecieved;

        #endregion Events and Handlers

        private const string _UDPPort = "2156";
        private const string _TCPInputPort = "302";
        private const string _TCPOutputPort = "302";
        private ConcurrentDictionary<Tuple<DatagramSocket, DataWriter>, bool> _UDPOutSockets; //the set of all UDP output sockets and the writers that send their data
        private ConcurrentDictionary<string, Tuple<bool, List<Packet>>> _joiningMembers; //the dictionary of members in the loading process.  HOST ONLY
        private HashSet<string> _otherIPs;//the set of all other IP's currently known about
        private string _hostIP;
        private string _localIP;
        private Timer _pingTimer;
        private Timer _phpPingTimer;
        private DatagramSocket _UDPsocket;
        private StreamSocketListener _TCPlistener;
        private ConcurrentDictionary<string, DataWriter> _addressToWriter; //A Dictionary of UDP socket writers that correspond to IP's
        private ConcurrentDictionary<string, int> _pingResponses;
        private NetworkConnector _networkConnector;

        public ClientHandler(NetworkConnector networkConnector)
        {
            _networkConnector = networkConnector;
            Init();
        }

        private async void Init()
        {
            _localIP = NetworkInformation.GetHostNames().FirstOrDefault(h => h.IPInformation != null
           && h.IPInformation.NetworkAdapter != null).RawName;

            Debug.WriteLine("local IP: " + _localIP);

            _joiningMembers = new ConcurrentDictionary<string, Tuple<bool, List<Packet>>>();
            _addressToWriter = new ConcurrentDictionary<string, DataWriter>();
            _UDPOutSockets = new ConcurrentDictionary<Tuple<DatagramSocket, DataWriter>, bool>();
            _otherIPs = new HashSet<string>();
            _pingResponses = new ConcurrentDictionary<string, int>();
            _phpPingTimer = new Timer(SendPhpPing, null, 0, 1000);

            var ips = await GetOtherIPs();
            if (ips.Count == 1)
            {
                this.MakeHost();
            }
            else
            {
                foreach (string ip in ips)
                {
                    AddIP(ip);
                }
            }

            //initializes the incoming sockets for TCP and UDP
            _TCPlistener = new StreamSocketListener();
            _TCPlistener.ConnectionReceived += this.TCPConnectionRecieved;
            await _TCPlistener.BindEndpointAsync(new HostName(this._localIP), _TCPInputPort);

            _UDPsocket = new DatagramSocket();
            await _UDPsocket.BindServiceNameAsync(_UDPPort);
            _UDPsocket.MessageReceived += this.DatagramMessageRecieved;
            await this.SendMassTCPMessage("SPECIALJOINING:" + this._localIP);
        }
        /*
        * this will make this network connection the host, forcibly
        */
        private void MakeHost()//TODO temporary
        {
            _hostIP = _localIP;
            _joiningMembers = new ConcurrentDictionary<string, Tuple<bool, List<Packet>>>();
            _pingResponses = new ConcurrentDictionary<string, int>();
            Debug.WriteLine("This machine (IP: " + _localIP + ") set to be the host");

            //TODO add in other host responsibilities
        }
        /*
        *return lcoal IP
        */

        public string LocalIP()
        {
            return _localIP;
        }
        /*
        * returns whether this machine is the host
        */
        public bool IsHost()
        {
            return _localIP == _hostIP;
        }

        /*
        * method called every timer tick (2 seconds for host, 1 seconds for non-host)
        */
        private async void PingTick(object state)
        {


            var toDelete = new List<string>();
            var keys = _pingResponses.Keys.ToArray();
            foreach (var ip in keys)
            {
                if (_pingResponses[ip] <= 2)
                {
                    _pingResponses[ip]++;
                    try
                    {
                        await SendPing(ip, NetworkConnector.PacketType.UDP);
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
                try
                {
                    await RemoveIP(s);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to remove IP: " + s);
                }
                if (_hostIP == s)
                {
                    //TODO STOP EVERYONE AND RESET HOST
                    await SendMassTCPMessage("SPECIALJOINING_RESPONSE:" + _localIP);
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
        public async Task SendDeleteMessage(string id)
        {
            await SendMessageToHost("SPECIALDELETE_REQUEST:" + id);
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
        private async Task SendPing(string ip, NetworkConnector.PacketType packetType)
        {
            await SendMessage(ip, "SPECIALPING:", packetType);
        }

        private void SendPhpPing(object state)
        {
            Task.Run(async () =>
            {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=ping&ip=" + _localIP;

                var client = new HttpClient { BaseAddress = new Uri(URL) };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(urlParameters);
            });
        }
        /*
        * this ends and restarts a timer.  It starts it with the list of people to ping updating according to the current list of network members
        */
        private void StartTimer()
        {
            if (_hostIP != null)
            {

                this.EndTimer();
                _pingResponses = new ConcurrentDictionary<string, int>();
                if (_hostIP == _localIP)
                {
                    _pingTimer = new Timer(PingTick, null, 0, 2000);
                    foreach (string ip in _otherIPs)
                    {
                        _pingResponses.TryAdd(ip, 0);
                    }
                }
                else
                {
                    _pingTimer = new Timer(PingTick, null, 0, 1000);
                    _pingResponses.TryAdd(_hostIP, 0);
                }
            }
        }

        /*
        * this ends the ping timer if it is running
        */
        private void EndTimer()
        {
            if (_pingTimer != null)
            {
                _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        /*
        * adds an IP address to the list of IPs and instantiates the outgoing sockets
        */
        private void AddIP(string ip)
        {
            if (!_otherIPs.Contains(ip) && ip != this._localIP)
            {
                _otherIPs.Add(ip);
                AddSocket(ip);
                StartTimer();
            }
            OnNewIP?.Invoke(ip);
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
            var client = new HttpClient { BaseAddress = new Uri(URL) };
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync(urlParameters);
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
            ThreadPool.RunAsync(async delegate { await SendMassTCPMessage("SPECIALREMOVE_IP_REQUEST:" + ip); });
        }

        /*
        * to tell everyone else in the network to remove this local IP from their list of IP's
        */
        private async Task TellRemoveLocalIP()
        {
            ThreadPool.RunAsync(async delegate {
                if (_otherIPs.Count > 0)
                {
                    if (_localIP == _hostIP)
                    {
                        //TODO tell everyone to stop actions and wait
                        await SendMassTCPMessage("SPECIALJOINING_RESPONSE:" + _otherIPs.ToArray()[0]);//if this is the host, tell everyone who the new host is because I'M OUT
                    }
                    await SendMassTCPMessage("SPECIALREMOVE_IP_REQUEST:" + _localIP);//tell everyone else to remove myself from their IP list
                }
            });
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
                    DataWriter removeWriter;
                    _addressToWriter.TryRemove(ip, out removeWriter); //remove the datagram socket data writer
                }
                foreach (var tup in _UDPOutSockets.Keys)
                {
                    if (tup.Item1.Information.RemoteAddress.RawName == ip)
                    {
                        bool r;
                        _UDPOutSockets.TryRemove(tup, out r); //remove the outgoing socket
                        break;
                    }
                }
                var set = new HashSet<KeyValuePair<string, string>>(); //create a list of lcoks that need to be removed

                OnRemoveIP?.Invoke(ip);
                int response;
                _pingResponses.TryRemove(ip, out response);
                if (_joiningMembers.ContainsKey(ip))
                {
                    Tuple<bool, List<Packet>> member;
                    _joiningMembers.TryRemove(ip, out member); //if that IP was joining, remove it
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
                        await RemoveIP(ip);
                        if (_hostIP == ip)
                        {
                            await SendMassTCPMessage("SPECIALJOINING_RESPONSE:" + _localIP);
                            MakeHost();
                        }
                        await SendMassTCPMessage("SPECIALREMOVE_IP_REQUEST:" + ip);
                        return;
                    }
                    var stringLength = reader.ReadUInt32();
                    var actualLength = await reader.LoadAsync(stringLength);//Read the incoming message
                    message = reader.ReadString(actualLength);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception caught during TCP connection recieve FROM IP " + ip + " with error code: " + e.Message);
                    return;
                }
                Debug.WriteLine("TCP connection recieve FROM IP " + ip + " with message: " + message);
                await this.MessageRecieved(ip, message, NetworkConnector.PacketType.TCP);//Process the message
            });
        }
        /*
        * a method for creating a new ID
        */
        public string GetID(string senderIP = null)
        {
            if (senderIP == null)
            {
                senderIP = _localIP;
            }
            var hash = senderIP.Replace(@".", "") + "#";
            var now = DateTime.UtcNow.Ticks + (new Random().Next(0, 1000000000));
            return hash + now;
        }
        /*
        * adds self to php script list of IP's 
        * called once at the beginning to get the list of other IP's on the network
        */
        private async Task<List<string>> GetOtherIPs()
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

                var client = new HttpClient { BaseAddress = new Uri(URL) };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var people = "";

                var response = await client.GetAsync(urlParameters);
                if (response.IsSuccessStatusCode)
                {
                    var d = response.Content.ReadAsStringAsync().Result; //gets the response from the php script
                    people = d;
                }
                else
                {
                    Debug.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
                Debug.WriteLine("in workspace: " + people);
                var split = people.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                var ips = split.ToList();
                Debug.WriteLine("other Ips:" + ips);
                return ips;
            }
        }
        /*
        * adds a new pair of sockets for a newly joining IP
        */
        private async Task AddSocket(string ip)
        {
            var UDPsocket = new DatagramSocket();
            await UDPsocket.ConnectAsync(new HostName(ip), _UDPPort);
            var UDPwriter = new DataWriter(UDPsocket.OutputStream);
            _UDPOutSockets.TryAdd(new Tuple<DatagramSocket, DataWriter>(UDPsocket, UDPwriter), true);

            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = UDPwriter;//adds the datagram sockets to the dictionary of them
            }
            else
            {
                _addressToWriter.TryAdd(ip, UDPwriter);
            }
        }

        /*
        * the incoming UDP packet method that reads and formats the message
        */
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
                    Debug.WriteLine("Exception caught during message recieve FROM IP " + ip + " with error code: " +
                                    e.Message);
                    return;
                }
                //Debug.WriteLine("UDP packet recieve FROM IP " + ip + " with message: " + message);
                await this.MessageRecieved(ip, message, NetworkConnector.PacketType.UDP);//process the message
            });
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
        public async Task SendMassUDPMessage(string message)
        {
            foreach (var tup in this._UDPOutSockets.Keys)//iterates through everyone
            {
                await this.SendUDPMessage(message, tup.Item2);
            }
        }

        /*
        * sends TCP Streams to everyone except self.  
        */
        public async Task SendMassTCPMessage(string message)
        {
            foreach (var ip in _otherIPs)
            {
                await this.SendTCPMessage(message, ip, _TCPOutputPort);
            }
        }

        /*
        * Sends Mass Message of specified Type
        */
        public async Task SendMassMessage(string message, NetworkConnector.PacketType packetType)
        {
            switch (packetType)
            {
                case NetworkConnector.PacketType.TCP:
                    await SendMassTCPMessage(message);
                    break;
                case NetworkConnector.PacketType.UDP:
                    await SendMassUDPMessage(message);
                    break;
                case NetworkConnector.PacketType.Both:
                    await SendMassUDPMessage(message);
                    await SendMassTCPMessage(message);
                    break;
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
        private async Task SendMessage(string ip, string message, NetworkConnector.PacketType packetType)
        {
            await SendMessage(ip, message, packetType, false);
        }
        /*
        * general all-purpose message sending method.  HIGHEST LEVEL SENDER.  can specify anything
        */
        public async Task SendMessage(string ip, string message, NetworkConnector.PacketType packetType, bool mass, bool self = false)//USE WITH CAUTION
        {
            switch (packetType)
            {
                case NetworkConnector.PacketType.TCP:
                    if (mass)
                    {
                        await SendMassTCPMessage(message);
                    }
                    else
                    {
                        await SendTCPMessage(message, ip);
                    }
                    break;
                case NetworkConnector.PacketType.UDP:
                    if (mass)
                    {
                        await SendMassUDPMessage(message);
                    }
                    else
                    {
                        await SendUDPMessage(message, _addressToWriter[ip]);
                    }
                    break;
                case NetworkConnector.PacketType.Both:
                    await this.SendMessage(ip, message, NetworkConnector.PacketType.UDP);
                    await this.SendMessage(ip, message, NetworkConnector.PacketType.TCP);
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
        public async Task SendMessageToHost(string message, NetworkConnector.PacketType packetType = NetworkConnector.PacketType.TCP)
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
        private async Task MessageRecieved(string ip, string message, NetworkConnector.PacketType packetType)
        {
            if (message.Length > 0)//if message exists
            {
                if (message.Substring(0, 7) != "SPECIAL") //if not a special message
                {
                    OnNewMessage?.Invoke(ip,message,packetType);
                }
                else
                {
                    try
                    {
                        await HandleSpecialMessage(ip, message.Substring(7), packetType);
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
                throw new NetworkConnector.IncorrectFormatException(message);
            }
            return;

        }

        /*
        * Handles SPECIAL messages.  Read each switch case for specifics
        */
        private async Task HandleSpecialMessage(string ip, string message, NetworkConnector.PacketType packetType)
        {
            string origMessage = message;
            var indexOfColon = message.IndexOf(":");
            if (indexOfColon == -1)
            {
                throw new NetworkConnector.IncorrectFormatException(message);
                return;
            }
            var type = message.Substring(0, indexOfColon);
            message = message.Substring(indexOfColon + 1);
            switch (type)
            {
                case "JOINING"://inital request = "I'm joining with my IP, who's the host?"
                    AddIP(message);
                    if (_hostIP != null)
                    {
                        await this.SendTCPMessage("SPECIALJOINING_RESPONSE:" + _hostIP, ip);
                    }
                    if (_hostIP == _localIP && message != _localIP && !_joiningMembers.ContainsKey(message))
                    {
                        //_joiningMembers.Add(message, new Tuple<bool, List<Packet>>(false, new List<Packet>()));//add new joining member
                        var m = await _networkConnector.GetFullWorkspace();
                        if (m.Length > 0)
                        {
                            await SendTCPMessage("SPECIALCATCH_UP:" + m, ip);
                        }
                        else
                        {
                            await SendTCPMessage("SPECIALCAUGHT_UP:0", ip);
                        }
                        return;
                    }
                    break;
                case "JOINING_RESPONSE":// response to initial request = "The host is the following person" ex: message = "10.10.10.10"
                    if (_hostIP != message)
                    {
                        _hostIP = message;
                        if (message == _localIP)
                        {
                            this.MakeHost();
                            await SendMassTCPMessage("SPECIALJOINING_RESPONSE:" + _localIP);
                        }
                        StartTimer();
                        Debug.WriteLine("Host returned and SET to be: " + message);
                        return;
                    }
                    break;
                case "CATCH_UP"://the message sent from host to tell other workspace to catch up.  message is formatted just like regular messages
                    if (_localIP == _hostIP)
                    {
                        throw new NetworkConnector.HostException(origMessage, ip);
                        return;
                    }
                    await MessageRecieved(ip, message, packetType);
                    await SendMessageToHost("SPECIALCATCH_UP_RESPONSE:DONE");
                    return;
                    break;
                case "CATCH_UP_RESPONSE":// response to catch up = "I am done catching up" ex: message = "DONE"
                    if (_localIP == _hostIP)
                    {
                        if (message == "DONE")
                        {
                            if (_joiningMembers.ContainsKey(ip) || true)//TODO re-implement joining members later
                            {
                                if (false && _joiningMembers[ip].Item1)//TODO fix these illogical statements
                                {
                                    var ret = "";
                                    foreach (var p in _joiningMembers[ip].Item2)
                                    {
                                        ret += p.Message + Constants.AndReplacement;
                                    }
                                    ret = ret.Substring(0, ret.Length - Constants.AndReplacement.Length);
                                    await SendTCPMessage("SPECIALCATCH_UP:" + ret, ip);
                                    _joiningMembers[ip].Item2.Clear();
                                    return;
                                }
                                else
                                {
                                    //await SendTCPMessage("SPECIALCAUGHT_UP:" + _joiningMembers[ip].Item2.Count, ip);
                                    await SendTCPMessage("SPECIALCAUGHT_UP:" + 0, ip);//TODO remove this line and uncomment above line
                                    await SendTCPMessage("SPECIALFULL_LOCK_UPDATE:" + _networkConnector.GetAllLocksToSend(), ip);
                                    //while(_joiningMembers[ip].Item2.Count>0)
                                    //{
                                    //await _joiningMembers[ip].Item2[0].Send(ip); TODO Uncomment this stuff
                                    //}
                                    //_joiningMembers.Remove(ip);//remove the joining member
                                    return;
                                }
                            }
                            else
                            {
                                Debug.WriteLine("ERROR: The host recieved caught-up message from somebody who wasn't known to be joining.  from IP: " + ip);
                                return;
                            }
                        }
                        else
                        {
                            throw new NetworkConnector.IncorrectFormatException("invalid format in catch up response");
                            return;
                        }
                    }
                    else
                    {
                        throw new NetworkConnector.NotHostException(origMessage, ip);
                    }
                    break;
                case "CAUGHT_UP": //Sent by Host only, "you are caught up and ready to join". message is simply the number of catch-up UDP packets also being sent
                    if (_localIP == _hostIP)
                    {
                        throw new NetworkConnector.HostException(origMessage, ip);
                        return;
                    }
                    Debug.WriteLine("Ready to Join Workspace");
                    return;
                case "LOCK_REQUEST"://HOST ONLY  request from someone to checkout a lock = "may I have a lock for the following id number" ex: message = "6"
                    if (_hostIP == _localIP)
                    {
                        await _networkConnector.Locks.Set(message, ip);
                        //await HandleSpecialMessage(_localIP,"SPECIALLOCK_SET:" + message + "=" + ModelIntermediate.Locks[message],PacketType.TCP);
                        OnLockUpdateRecieved?.Invoke(message, _networkConnector.Locks.Value(message));
                        await SendMassTCPMessage("SPECIALLOCK_SET:" + message + "=" + _networkConnector.Locks.Value(message));
                        return;
                    }
                    else
                    {
                        throw new NetworkConnector.NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "LOCK_SET"://Response from Lock get request = "the id number has a lock holder of the following IP"  ex: message = "6=10.10.10.10"
                    var parts = message.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2 && parts.Length != 1)
                    {
                        throw new NetworkConnector.IncorrectFormatException(origMessage);
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
                    OnLockUpdateRecieved?.Invoke(lockId, lockHolder);
                    return;
                    break;
                case "LOCK_RETURN"://Returning lock  ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (_networkConnector.HasSendableID(message))
                        {
                            await _networkConnector.Locks.Set(message, "");
                            await SendMessage(ip, "SPECIALLOCK_SET:" + message + "=", NetworkConnector.PacketType.TCP, true, true);
                            return;
                        }
                        else
                        {
                            throw new NetworkConnector.InvalidIDException(message);
                            return;
                        }
                    }
                    else
                    {
                        throw new NetworkConnector.NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "NODE_UPDATE_REQUEST"://Request full node update for certain id -- HOST ONLY ex: message = "6"
                    if (_localIP == _hostIP)
                    {
                        if (!_networkConnector.HasSendableID(message))
                        {
                            throw new NetworkConnector.InvalidIDException(message);
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
                        throw new NetworkConnector.NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "REMOVE_IP_REQUEST"://Tell others to remove IP from self ex: message = "10.10.10.10"
                    RemoveIP(message);
                    break;
                case "DELETE_REQUEST":// Request to delete a node.  HOST ONLY.   ex: message = an ID
                    if (_networkConnector.HasSendableID(message))
                    {
                        if (_hostIP == _localIP)
                        {
                            await SendMassTCPMessage("SPECIALDELETE_REQUEST:" + message);
                        }
                        OnDeleteRequestRecieved?.Invoke(message);
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("tried to delete ID " + message + " when it doesn't locally exist");
                    }
                    break;
                case "PING"://a simple 'ping'.  Will respond to ping with a 'NO' meaning 'dont reply'.  ex: message = "" or "NO"
                    this.Pingged(ip);
                    if (message != "NO")
                    {
                        await SendMessage(ip, "SPECIALPING:NO", packetType);
                    }
                    break;
                case "FULL_LOCK_UPDATE"://A full update from the host about the current locks
                    if (message != "")
                    {
                        OnAllLocksSet?.Invoke(message);
                    }
                    break;
                default:
                    throw new NetworkConnector.IncorrectFormatException("SPECIAL message was unknown");
                    break;
            }
        }
        public async Task RequestLock(string id)
        {
            await SendMessageToHost("SPECIALLOCK_REQUEST:" + id);
        }

        public async Task ReturnLock(string id)
        {
            await SendMessageToHost("SPECIALLOCK_RETURN:" + id);
        }
        /*
        * sends TCP update to someone with the entire current state of a node
        */
        private async Task SendUpdateForNode(string nodeId, string sendToIP)
        {
            var dict = await _networkConnector.GetNodeState(nodeId);
            if (dict != null)
            {
                string message = _networkConnector.MakeSubMessageFromDict(dict);
                await SendTCPMessage(message, sendToIP);
            }
        }
        private class Packet //private class to store messages for later
        {
            private readonly NetworkConnector.PacketType _type;
            private ClientHandler _clientHandler;
            public Packet(string message, ClientHandler n, NetworkConnector.PacketType type)//set all the params
            {
                _clientHandler = n;
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
                    case NetworkConnector.PacketType.TCP:
                        await _clientHandler.SendTCPMessage(Message, address);
                        break;
                    case NetworkConnector.PacketType.UDP:
                        await _clientHandler.SendUDPMessage(Message, address);
                        break;
                }
            }
        }
    }
}
