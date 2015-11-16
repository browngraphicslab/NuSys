using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class DebouncingDictionary
    {
        private ConcurrentDictionary<string, string> _dict;
        private bool _timing = false;
        private Timer _timer;
        private Sendable _atom;
        private bool _sendNextTCP = false;
        private int _milliSecondDebounce = 15;

        public DebouncingDictionary(Sendable atom)
        {
            _dict = new ConcurrentDictionary<string, string>();
            _atom = atom;

        }
        public DebouncingDictionary(AtomModel atom, int milliSecondDebounce)
        {
            _dict = new ConcurrentDictionary<string, string>();
            _atom = atom;
            _milliSecondDebounce = _milliSecondDebounce;
        }

        public void MakeNextMessageTCP()
        {
            _sendNextTCP = true;
        }

        public void Add(string id, string value)
        {
            if (!NetworkConnector.Instance.IsSendableBeingUpdated(_atom.ID) && (_atom.CanEdit == AtomModel.EditStatus.Yes || _atom.CanEdit == AtomModel.EditStatus.Maybe))
            {
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
        }

        private async void SendMessage(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            if (_atom.CanEdit == AtomModel.EditStatus.Yes || _atom.CanEdit == AtomModel.EditStatus.Maybe)
            {
                _dict.TryAdd("id", _atom.ID);
                if (NetworkConnector.Instance.HasSendableID(_atom.ID))
                {
                    if (_sendNextTCP)
                    {
                        _sendNextTCP = false;
                        await NetworkConnector.Instance.QuickUpdateAtom(new Dictionary<string, string>(_dict), NetworkConnector.PacketType.TCP);
                    }
                    else
                    {
                        await NetworkConnector.Instance.QuickUpdateAtom(new Dictionary<string, string>(_dict));
                    }
                }
            }
            _timing = false;
            _dict.Clear();
        }
    }
}
