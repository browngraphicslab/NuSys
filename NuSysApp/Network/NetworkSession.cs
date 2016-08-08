using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Sms;
using Windows.System.Threading;
using NusysIntermediate;

namespace NuSysApp
{
    public class NetworkSession
    {
        #region Public Members
        public string LocalIP{get { return _networkClient.LocalIP; }}
        public int PingFrequency {
            get
            {
                return _pingFrequency;
            } 
            set {
                _pingFrequency = value;
                _pingTimer?.Change(0, value);
            }
        }

        public HashSet<string> NetworkMembers
        {
            get { return _networkMembers; }
        }

        public delegate void PingEventHandler();
        public event PingEventHandler OnPing;

        public delegate void ClientDroppedEventHandler(string ip);
        public event ClientDroppedEventHandler OnClientDrop;

        public delegate void MessageRecievedEventHandler(Message message, NetworkClient.PacketType type,string ip);
        public event MessageRecievedEventHandler OnMessageRecieved;

        #endregion Public Members
        #region Private Members
        private NetworkClient _networkClient;
        private Timer _pingTimer;
        private int _pingFrequency = 1000;
        private HashSet<string> _networkMembers; 
        #endregion Private Members

        public NetworkSession(ICollection<string> startingIPs)
        {
            _networkMembers = new HashSet<string>(startingIPs);
        }
        public async Task Init()
        {
            _networkClient = new NetworkClient();
            await _networkClient.Init();
            _networkClient.OnNewMessage += MessageRecieved;
            _networkClient.OnTCPClientDrop += TCPDrop;
            _pingTimer = new Timer(PingEvent, null,0,_pingFrequency);
        }

        private void PingEvent(object state){OnPing?.Invoke();}
        private void TCPDrop(string ip) { OnClientDrop?.Invoke(ip); }

        private async void MessageRecieved(string ip, Message message, NetworkClient.PacketType packetType)
        {
            OnMessageRecieved?.Invoke(message, packetType,ip);
        }

        public async Task SendRequestMessage(Message message, ICollection<string> ips,
            NetworkClient.PacketType packetType)
        {
            if (ips.Contains(LocalIP))
            {
                ips.Remove(LocalIP);
            }
            await _networkClient.SendMessage(message, ips.ToArray(), packetType);
        }

        public async Task SendRequestMessage(Message message, string ip, NetworkClient.PacketType packetType)
        {
            if (ip == LocalIP)
                return;
            await _networkClient.SendMessage(message, ip, packetType);
        }

        public void AddIP(string ip)
        {
            if (!_networkMembers.Contains(ip))
                _networkMembers.Add(ip);
            Debug.WriteLine("added network member "+ip+"  .");
        }

        public bool RemoveIP(string ip)
        {
            if (_networkMembers.Contains(ip))
            {
                _networkMembers.Remove(ip);
                return true;
            }
            return false;
        }
    }
}
