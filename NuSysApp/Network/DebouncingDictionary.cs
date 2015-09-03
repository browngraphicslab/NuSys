using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml;

namespace NuSysApp.Network
{
    public class DebouncingDictionary
    {
        private Dictionary<string, string> _dict;
        private bool _timing = false;
        private Timer _timer;
        private Sendable _atom;
        private bool _sendNextTCP = false;
        private int _milliSecondDebounce = 100;
        public DebouncingDictionary(Sendable atom)
        {
            _dict = new Dictionary<string, string>();
            _atom = atom;

        }
        public DebouncingDictionary(Atom atom, int milliSecondDebounce)
        {
            _dict = new Dictionary<string, string>();
            _atom = atom;
            _milliSecondDebounce = _milliSecondDebounce;
        }

        public void MakeNextMessageTCP()
        {
            _sendNextTCP = true;
        }

        public void Add(string id, string value)
        {
            if (!NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(_atom.ID) && (_atom.CanEdit == Atom.EditStatus.Yes || _atom.CanEdit == Atom.EditStatus.Maybe))
            {
                if (!_timing)
                {
                    _timing = true;
                    _dict.Add(id, value);
                    _timer = new Timer(SendMessage, null, Timeout.Infinite, _milliSecondDebounce);
                }
                else
                {
                    if (_dict.ContainsKey(id))
                    {
                        _dict[id] = value;
                        return;
                    }
                    _dict.Add(id, value);
                }
            }
        }

        private async void SendMessage(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            if (_atom.CanEdit == Atom.EditStatus.Yes || _atom.CanEdit == Atom.EditStatus.Maybe)
            {
                _dict.Add("id", _atom.ID);
                if (NetworkConnector.Instance.ModelIntermediate.HasSendableID(_atom.ID))
                {
                    if (_sendNextTCP)
                    {
                        _sendNextTCP = false;
                        await NetworkConnector.Instance.QuickUpdateAtom(_dict, NetworkConnector.PacketType.TCP);
                    }
                    else
                    {
                        await NetworkConnector.Instance.QuickUpdateAtom(_dict);
                    }
                }
            }
            _timing = false;
            _dict.Clear();
        }
    }
}
