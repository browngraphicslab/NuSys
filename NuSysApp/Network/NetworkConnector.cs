﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
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
        private bool _caughtUp = false;
        private ConcurrentDictionary<string, int> _pingResponses;

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

        /*
         * gets and sets the workspace model that the network connector communicates with
         */
        public ModelIntermediate ModelIntermediate { get; set; }
        /*
        * Essentially an async constructor
        */
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

            var ips = GetOtherIPs();
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
            await this.SendMassTCPMessage("SPECIAL0:" + this._localIP);

        }

        /*
        * this will make this network connection the host, forcibly
        */
        private void MakeHost()//TODO temporary
        {
            _hostIP = _localIP;
            _joiningMembers = new ConcurrentDictionary<string, Tuple<bool, List<Packet>>>();
            _caughtUp = true;
            _pingResponses = new ConcurrentDictionary<string, int>();
            Debug.WriteLine("This machine (IP: " + _localIP + ") set to be the host");

            //TODO add in other host responsibilities
        }

        /*
        * method called every timer tick (2 seconds for host, 1 seconds for non-host)
        */
        private async void PingTick(object state) {

            
            var toDelete = new List<string>();
            var keys = _pingResponses.Keys.ToArray();
            foreach (var ip in keys)
            {
                if (_pingResponses[ip] <= 2)
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
        private async Task SendPing(string ip, PacketType packetType)
        {
            await SendMessage(ip, "SPECIAL11:", packetType);
        }

        private void SendPhpPing( object state)
        {
              Task.Run(() =>
            {
                const string URL = "http://aint.ch/nusys/clients.php";
                var urlParameters = "?action=ping&ip=" + _localIP;

                var client = new HttpClient { BaseAddress = new Uri(URL) };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(urlParameters).Result;
            });
        }

        /*
        * this ends a restarts a timer.  It starts it with the list of people to ping updating according to the current list of network members
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
                        _pingResponses.GetOrAdd(ip, 0);
                    }
                }
                else
                {
                    _pingTimer = new Timer(PingTick, null, 0, 1000);
                    _pingResponses.GetOrAdd(_hostIP, 0);
                }
            }
        }

        /*
        * this ends the ping timer if it is running
        */
        private void EndTimer()
        {
            if (_pingTimer != null )
            {
                _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        public string LocalIP//Returns the local IP
        {
            get { return _localIP; }
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

                ModelIntermediate.RemoveIPFromLocks(ip);
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
                        await SendMassTCPMessage("SPECIAL1:" + _localIP);
                        MakeHost();
                    }
                    await SendMassTCPMessage("SPECIAL9:" + ip);
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
            await this.MessageRecieved(ip, message, PacketType.TCP);//Process the message
        }
        /*
        * a method for creating a new ID
        */
        private string GetID(string senderIP = null)
        {
            if (senderIP == null)
            {
                senderIP = _localIP;
            }
            var hash = senderIP.Replace(@".", "") + "#";
            var now = DateTime.UtcNow.Ticks.ToString();
            return hash + now;
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

                var client = new HttpClient { BaseAddress = new Uri(URL) };

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
        private void AddSocket(string ip)
        {
            var UDPsocket = new DatagramSocket();
            UDPsocket.ConnectAsync(new HostName(ip), _UDPPort);
            var UDPwriter = new DataWriter(UDPsocket.OutputStream);
            _UDPOutSockets.GetOrAdd(new Tuple<DatagramSocket, DataWriter>(UDPsocket, UDPwriter), true);

            if (_addressToWriter.ContainsKey(ip))
            {
                _addressToWriter[ip] = UDPwriter;//adds the datagram sockets to the dictionary of them
            }
            else
            {
                _addressToWriter.GetOrAdd(ip, UDPwriter);
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
            foreach (var tup in this._UDPOutSockets.Keys)//iterates through everyone
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
        * Sends Mass Message of specified Type
        */
        private async Task SendMassMessage(string message, PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.TCP:
                    await SendMassTCPMessage(message);
                    break;
                case PacketType.UDP:
                    await SendMassUDPMessage(message);
                    break;
                case PacketType.Both:
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
                    var miniStrings = message.Split(new string[] { Constants.AndReplacement }, StringSplitOptions.RemoveEmptyEntries); //break up message into subparts
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
        private async Task HandleSpecialMessage(string ip, string message, PacketType packetType)
        {
            string origMessage = message;
            var indexOfColon = message.IndexOf(":");
            if (indexOfColon == -1)
            {
                throw new IncorrectFormatException(message);
                return;
            }
            var type = message.Substring(0, indexOfColon);
            message = message.Substring(indexOfColon + 1);
            switch (type)
            {
                case "0"://inital request = "I'm joining with my IP, who's the host?"
                    AddIP(message);
                    if (_hostIP != null)
                    {
                        await this.SendTCPMessage("SPECIAL1:" + _hostIP, ip);
                    }
                    if (_hostIP == _localIP && message != _localIP && !_joiningMembers.ContainsKey(message))
                    {
                        //_joiningMembers.Add(message, new Tuple<bool, List<Packet>>(false, new List<Packet>()));//add new joining member
                        var m = await ModelIntermediate.GetFullWorkspace();
                        if (m.Length > 0)
                        {
                            await SendTCPMessage("SPECIAL2:" + m, ip);
                        }
                        else
                        {
                            await SendTCPMessage("SPECIAL4:0", ip);
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
                        StartTimer();
                        Debug.WriteLine("Host returned and SET to be: " + message);
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
                                    await SendTCPMessage("SPECIAL2:" + ret, ip);
                                    _joiningMembers[ip].Item2.Clear();
                                    return;
                                }
                                else
                                {
                                    //await SendTCPMessage("SPECIAL4:" + _joiningMembers[ip].Item2.Count, ip);
                                    await SendTCPMessage("SPECIAL4:" + 0, ip);//TODO remove this line and uncomment above line
                                    await SendTCPMessage("SPECIAL12:" + ModelIntermediate.GetAllLocksToSend(), ip);
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

                        await ModelIntermediate.Locks.Set(message, ip);
                        //await HandleSpecialMessage(_localIP,"SPECIAL6:" + message + "=" + ModelIntermediate.Locks[message],PacketType.TCP);
                        ModelIntermediate.SetAtomLock(message, ModelIntermediate.Locks.Value(message));
                        await SendMassTCPMessage("SPECIAL6:" + message + "=" + ModelIntermediate.Locks.Value(message));
                        return;
                    }
                    else
                    {
                        throw new NotHostException(origMessage, ip);
                        return;
                    }
                    break;
                case "6"://Response from Lock get request = "the id number has a lock holder of the following IP"  ex: message = "6=10.10.10.10"
                    var parts = message.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
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
                        if (ModelIntermediate.HasSendableID(message))
                        {
                            await ModelIntermediate.Locks.Set(message, "");
                            await SendMessage(ip, "SPECIAL6:" + message + "=", PacketType.TCP, true, true);
                            return;
                        }
                        else
                        {
                            throw new InvalidIDException(message);
                            return;
                        }
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
                        if (!ModelIntermediate.HasSendableID(message))
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
                    if (ModelIntermediate.HasSendableID(message))
                    {
                        if (_hostIP == _localIP)
                        {
                            await SendMassTCPMessage("SPECIAL10:" + message);
                        }
                        await ModelIntermediate.RemoveSendable(message);
                        return;
                    }
                    else
                    {
                        Debug.WriteLine("tried to delete ID "+message+" when it doesn't locally exist");
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
                    break;
            }
        }


        /*
        * sends TCP update to someone with the entire current state of a node
        */
        private async Task SendUpdateForNode(string nodeId, string sendToIP)
        {
            var dict = await ModelIntermediate.GetNodeState(nodeId);
            if (dict != null)
            {
                string message = MakeSubMessageFromDict(dict);
                await SendTCPMessage(message, sendToIP);
            }
        }

        /*
        * handles and proccesses a regular sub-message
        */
        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
        {
            /*if (_hostIP == _localIP)//this HOST ONLY block is to special case for the host getting a 'make-node' request
            {
                if (message.IndexOf("OLDSQLID") != -1)
                {
                    Dictionary<string, string> dict = ParseOutProperties(message);
                    dict["id"] = dict["OLDSQLID"];
                    dict.Remove("OLDSQLID");
                    string m = MakeSubMessageFromDict(dict);
                    await HandleRegularMessage(ip, m, packetType);
                    await SendMassTCPMessage(m);
                    return;
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
            }*/
            if (_localIP == _hostIP)//if host, add a new packet and store it in every joining member's stack of updates
            {
                foreach (var kvp in _joiningMembers)
                // keeps track of messages sent during initial loading into workspace
                {
                    kvp.Value.Item2.Add(new Packet(message, packetType));
                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
                    {
                        var tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
                        _joiningMembers[kvp.Key] = tup;
                    }
                }
            }
            if (message[0] == '<' && message[message.Length - 1] == '>' || true)
            {
                Dictionary<string, string> props = ParseOutProperties(message);
                if (props.ContainsKey("id"))
                {
                    await ModelIntermediate.HandleMessage(props);
                    if ((ModelIntermediate.HasSendableID(props["id"]) || (props.ContainsKey("nodeType") && props["nodeType"]==NodeType.PDF.ToString()))&& packetType == PacketType.TCP && _localIP == _hostIP)
                    {
                        await SendMassTCPMessage(message);
                    }
                }
                else
                {
                    throw new InvalidIDException("there was no id, that's why this error is occurring...");
                }
            }
            else
            {
                throw new IncorrectFormatException(message);
            }
        }

        /*
        * parses message to dictionary of properties
        */
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(new string[] { Constants.CommaReplacement }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    string[] subParts = part.Split(new string[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (subParts.Length != 2)
                    {
                        continue;
                    }
                    if (!props.ContainsKey(subParts[0]))
                    {
                        props.Add(subParts[0], subParts[1]);
                    }
                    else
                    {
                        props[subParts[0]] = subParts[1];
                    }
                }
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


        #region publicRequests
        /*
        * PUBLIC request for deleting a nod 
        */
        public async Task RequestDeleteSendable(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (ModelIntermediate.HasLock(id))
                {
                    await SendMessageToHost("SPECIAL10:" + id); //tells host to delete the node
                }
            });
        }

        /*
        * PUBLIC general method to update everyone from an Atom update.  sends mass udp packet
        */
        public async Task QuickUpdateAtom(Dictionary<string, string> properties, PacketType packetType = PacketType.UDP)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (properties.ContainsKey("id"))
                {
                    if (ModelIntermediate.HasSendableID(properties["id"]))
                    {
                        string message = MakeSubMessageFromDict(properties);
                        await SendMassMessage(message, packetType);
                        if (packetType == PacketType.TCP)
                        {
                            await HandleRegularMessage(_localIP, message, packetType);
                        }
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
            });
        }

        /*
        * PUBLIC general method to create Node
        */
        public async Task RequestMakeNode(string x, string y, string nodeType, string data = null, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (x != "" && y != "" && nodeType != "")
                {
                    Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                    string id = oldID == null ? GetID() : oldID;

                    if (props.ContainsKey("x"))
                    {
                        props.Remove("x");
                    }
                    if (props.ContainsKey("y"))
                    {
                        props.Remove("y");
                    }
                    if (props.ContainsKey("id"))
                    {
                        props.Remove("id");
                    }
                    if (props.ContainsKey("nodeType"))
                    {
                        props.Remove("nodeType");
                    }
                    if (props.ContainsKey("type"))
                    {
                        props.Remove("type");
                    }
                    props.Add("x", x);
                    props.Add("y", y);
                    props.Add("nodeType", nodeType);
                    props.Add("id", id);
                    props.Add("type", "node");
                    if (data != null && data != "null" && data != "")
                    {
                        props.Add("data", data);
                    }

                    if (callback != null)
                    {
                        ModelIntermediate.AddCreationCallback(id, callback);
                    }

                    string message = MakeSubMessageFromDict(props);

                    await SendMessageToHost(message);
                }
                else
                {
                    throw new InvalidCreationArgumentsException();
                    return;
                }
            });
        }

        /*
        * PUBLIC general method to create Pin
        */
        public async Task RequestMakePin(string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
 
                var props = properties == null ? new Dictionary<string, string>() : properties;
            string id = oldID == null ? GetID() : oldID;
            if (props.ContainsKey("x"))
            {
                props.Remove("x");
            }
            if (props.ContainsKey("y"))
            {
                props.Remove("y");
            }
            if (props.ContainsKey("id"))
            {
                props.Remove("id");
            }
            if (props.ContainsKey("type"))
            {
                props.Remove("type");
            }
            props.Add("x", x);
            props.Add("y", y);
            props.Add("id", id);
            props.Add("type", "pin");
            string message = MakeSubMessageFromDict(props);
            if (callback != null)
            {
                ModelIntermediate.AddCreationCallback(oldID, callback);
            }
            await SendMessageToHost(message);
        }

        /*
        * PUBLIC general method to create Group
        */
        public async Task RequestMakeGroup(string id1, string id2, string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            if (id1 != "" && id2 != "")
            {
                if (ModelIntermediate.HasSendableID(id1))
                {
                    if (ModelIntermediate.HasSendableID(id2))
                    {
                        Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                        string id = oldID == null ? GetID() : oldID;

                        if (props.ContainsKey("id1"))
                        {
                            props.Remove("id1");
                        }
                        if (props.ContainsKey("id2"))
                        {
                            props.Remove("id2");
                        }
                        if (props.ContainsKey("x"))
                        {
                            props.Remove("x");
                        }
                        if (props.ContainsKey("y"))
                        {
                            props.Remove("y");
                        }
                        if (props.ContainsKey("id"))
                        {
                            props.Remove("id");
                        }
                        if (props.ContainsKey("type"))
                        {
                            props.Remove("type");
                        }
                        props.Add("x", x);
                        props.Add("y", y);
                        props.Add("id1", id1);
                        props.Add("id2", id2);
                        props.Add("id", id);
                        props.Add("type", "group");
                        if (callback != null)
                        {
                            ModelIntermediate.AddCreationCallback(id, callback);
                        }
                        string message = MakeSubMessageFromDict(props);
                        await SendMessageToHost(message);
                    }
                    else
                    {
                        throw new InvalidIDException(id2);
                    }
                }
                else
                {
                    throw new InvalidIDException(id1);
                }
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }

        /*
       * PUBLIC general method to create Group
       */
        public async Task RequestMakeEmptyGroup( string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {

            var props = properties == null ? new Dictionary<string, string>() : properties;
            string id = oldID == null ? GetID() : oldID;

            if (props.ContainsKey("x"))
            {
                props.Remove("x");
            }
            if (props.ContainsKey("y"))
            {
                props.Remove("y");
            }
            if (props.ContainsKey("id"))
            {
                props.Remove("id");
            }
            if (props.ContainsKey("type"))
            {
                props.Remove("type");
            }
            props.Add("x", x);
            props.Add("y", y);
            props.Add("id", id);
            props.Add("type", "emptygroup");
            if (callback != null)
            {
                ModelIntermediate.AddCreationCallback(id, callback);
            }
            string message = MakeSubMessageFromDict(props);
            await SendMessageToHost(message);
        }

        /*
        * PUBLIC general method to create Linq
        */
        public async Task RequestMakeLinq(string id1, string id2, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            if (id1 == id2)
            {
                throw new InvalidCreationArgumentsException("the two ids for the link were identical");
            }
            if (id1 != "" && id2 != "" && ModelIntermediate.HasSendableID(id1) && ModelIntermediate.HasSendableID(id2))
            {
                Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                string id = oldID == null ? GetID() : oldID;

                if (props.ContainsKey("id1"))
                {
                    props.Remove("id1");
                }
                if (props.ContainsKey("id2"))
                {
                    props.Remove("id2");
                }
                if (props.ContainsKey("type"))
                {
                    props.Remove("type");
                }
                if (props.ContainsKey("id"))
                {
                    props.Remove("id");
                }

                props.Add("id1", id1);
                props.Add("id2", id2);
                props.Add("type", "link");
                props.Add("id", id);

                if (callback != null)
                {
                    ModelIntermediate.AddCreationCallback(oldID, callback);
                }

                string message = MakeSubMessageFromDict(props);

                await SendMessageToHost(message);
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }
        public async Task SendPartialLine(string id, string canvasNodeID, string x1, string y1, string x2, string y2)
        {
            ThreadPool.RunAsync(async delegate
            {
                Dictionary<string, string> props = new Dictionary<string, string>
            {
                {"x1", x1},
                {"x2", x2},
                {"y1", y1},
                {"y2", y2},
                {"id", id},
                {"canvasNodeID", canvasNodeID},
                {"type", "ink"},
                {"inkType", "partial"}
            };
                string m = MakeSubMessageFromDict(props);
                await SendMassUDPMessage(m);
            });
        }

        public async Task FinalizeGlobalInk(string previousID, string canvasNodeID,string data)
        {
            ThreadPool.RunAsync(async delegate
            {
                Dictionary<string, string> props = new Dictionary<string, string>
            {
                {"type", "ink"},
                {"inkType", "full"},
                {"canvasNodeID", canvasNodeID},
                {"id", GetID()},
                {"data", data},
                {"previousID", previousID}
            };
                string m = MakeSubMessageFromDict(props);
                await SendMessageToHost(m);
            });
        }
        public async Task RequestLock(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (ModelIntermediate.HasSendableID(id))
                {
                    Debug.WriteLine("Requesting lock for ID: " + id);
                }
                else
                {
                    Debug.WriteLine("Requesting lock for ID: " + id + " although it doesn't exist yet");
                }
                await SendMessageToHost("SPECIAL5:" + id, PacketType.TCP);
            });
        }

        public async Task ReturnLock(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (ModelIntermediate.HasSendableID(id))
                {
                    Debug.WriteLine("Returning lock for ID: " + id);
                }
                else
                {
                    Debug.WriteLine("Attempted to return lock with ID: " + id + " When no such ID exists");
                    throw new InvalidIDException(id);
                }
                await SendMessageToHost("SPECIAL7:" + id);
                await SendMassTCPMessage(MakeSubMessageFromDict(await ModelIntermediate.GetNodeState(id)));
            });
        }
        #endregion publicRequests
        #region customExceptions
        public class InvalidIDException : Exception
        {
            public InvalidIDException(string id) : base(String.Format("The ID {0}  was used but is invalid", id)) { }
        }
        public class IncorrectFormatException : Exception
        {
            public IncorrectFormatException(string message) : base(String.Format("The message '{0}' is incorrectly formatted or unrecognized", message)) { }
        }

        public class NotHostException : Exception
        {
            public NotHostException(string message, string remoteIP)
                : base(String.Format("The message {0} was sent to a non-host from IP: {1} when it is a host-only message", message, remoteIP))
            { }
        }

        public class HostException : Exception
        {
            public HostException(string message, string remoteIP) : base(String.Format("The message {0} was sent to this machine, THE HOST, from IP: {1} when it is meant for only non-hosts", message, remoteIP)) { }
        }
        public class UnknownIPException : Exception
        {
            public UnknownIPException(string ip) : base(String.Format("The IP {0} was used when it is not recgonized", ip)) { }
        }

        public class NoIDException : Exception
        {
            public NoIDException(string message) : base(message) { }
            public NoIDException() { }
        }

        public class InvalidCreationArgumentsException : Exception
        {
            public InvalidCreationArgumentsException(string message) : base(message) { }
            public InvalidCreationArgumentsException() { }
        }
        #endregion customExceptions
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
                        await Instance.SendTCPMessage(Message, address);
                        break;
                    case PacketType.UDP:
                        await Instance.SendUDPMessage(Message, address);
                        break;
                }
            }
        }
    }
}
