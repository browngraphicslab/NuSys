using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    public class DebouncingDictionary
    {
        private ConcurrentDictionary<string, object> _dict; 
        private bool _timing = false;
        private Timer _timer;
        private int _milliSecondDebounce = 30;
        private string _id;
        private bool _updateLibraryElement = false;

        private ConcurrentDictionary<string, object> _serverDict;
        private int _milliSecondServerSaveDelay = 800;
        private Timer _serverSaveTimer;
        public DebouncingDictionary(string id, bool updateLibraryElement = false)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _id = id;
            _updateLibraryElement = updateLibraryElement;
        }

        public DebouncingDictionary(string id, int milliSecondDebounce, bool updateLibraryElement = false)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _milliSecondDebounce = _milliSecondDebounce;
            _id = id;
            _updateLibraryElement = updateLibraryElement;
        }

        public void Add(string id, object value)
        {
            if (!_timing)
            {
                _timing = true;
                _dict.TryAdd(id, value);
                if (_serverDict.ContainsKey(id))
                {
                    _serverDict[id] = value;
                }
                else
                {
                    _serverDict.TryAdd(id, value);
                }
                _timer?.Change(_milliSecondDebounce, _milliSecondDebounce);
                _serverSaveTimer?.Change(_milliSecondServerSaveDelay, _milliSecondServerSaveDelay);
            }
            else
            {
                if (_dict.ContainsKey(id))
                {
                    _dict[id] = value;
                    _serverDict[id] = value;
                }
                else
                {
                    _dict.TryAdd(id, value);
                    _serverDict.TryAdd(id, value);
                }
                _serverSaveTimer?.Change(_milliSecondServerSaveDelay, _milliSecondServerSaveDelay);
            }
        }

        private async void SendMessage(object state)
        {
            bool saveToServer = (bool) state;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Dictionary<string, object> d;
            if (saveToServer)
            {
                d = _serverDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                _serverDict.Clear();
                _serverSaveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                d = _dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            if (!_updateLibraryElement && d.ContainsKey("id"))
            {
                Debug.WriteLine("Debounce dictionary had a previous 'id' value.  It was overritten with the original ID");
            }
            if(_updateLibraryElement && d.ContainsKey("contentId"))
            {
                Debug.WriteLine("Debounce dictionary had a previous 'contentId' value.  It was overritten with the original ID");
            }
            d[_updateLibraryElement ? "contentId":"id"] = _id;
            var message = new Message(d);
            if (d.Count > 1)
            {
                Request request;
                if (_updateLibraryElement)
                {
                    request = new ChangeContentRequest(message);
                }
                else
                {
                    request = new SendableUpdateRequest(message, saveToServer);
                }
                SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            }
            _timing = false;
            _dict.Clear();
        }
    }
}
