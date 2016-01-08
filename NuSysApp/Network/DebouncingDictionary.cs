using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class DebouncingDictionary
    {
        private ConcurrentDictionary<string, object> _dict; 
        private bool _timing = false;
        private Timer _timer;
        private bool _sendNextTCP = false;
        private int _milliSecondDebounce = 20;
        private string _id;
        public DebouncingDictionary(string id)
        {
            _timer = new Timer(SendMessage, null, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _id = id;
        }

        public DebouncingDictionary(string id, int milliSecondDebounce)
        {
            _timer = new Timer(SendMessage, null, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _milliSecondDebounce = _milliSecondDebounce;
            _id = id;
        }

        public void MakeNextMessageTCP()
        {
            _sendNextTCP = true;
        }

        public void Add(string id, object value)
        {
            Debug.WriteLine("adding to debounce dict");
            if (!_timing)
            {
                _timing = true;
                _dict.TryAdd(id, value);
                _timer = new Timer(SendMessage, null, 0, _milliSecondDebounce);
            }
            else
            {
                if (_dict.ContainsKey(id))
                {
                    _dict[id] = value;
                    return;
                }
                _dict.TryAdd(id, value);
            }
        }

        private async void SendMessage(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            var packetType = NetworkClient.PacketType.UDP;
            if (_sendNextTCP)
            {
                _sendNextTCP = false;
                packetType = NetworkClient.PacketType.TCP;
                //await NetworkConnector.Instance.QuickUpdateAtom(new Dictionary<string, object>(_message), NetworkConnector.PacketType.TCP);
            }
            Dictionary<string, object> d = _dict.ToDictionary(kvp => kvp.Key,kvp => kvp.Value);
            if (d.ContainsKey("id"))
            {
                Debug.WriteLine("Debounce dictionary had a previous 'id' value.  It was overritten with the original ID");
            }
            d["id"] = _id;
            var message = new Message(d);
            var request = new SendableUpdateRequest(message);
            Debug.WriteLine("sending debounce dict");
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request, packetType);
            _timing = false;
            _dict.Clear();
        }
    }
}
