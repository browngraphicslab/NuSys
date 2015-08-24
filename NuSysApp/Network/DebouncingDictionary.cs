using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace NuSysApp.Network
{
    public class DebouncingDictionary
    {
        private Dictionary<string, string> _dict;
        private bool _timing = false;
        private DispatcherTimer _timer;
        private Atom _atom;
        private bool _sendNextTCP = false;
        public DebouncingDictionary(Atom atom)
        {
            _dict = new Dictionary<string, string>();
            _atom = atom;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += SendMessage;

        }
        public DebouncingDictionary(Atom atom, int milliSecondDebounce)
        {
            _dict = new Dictionary<string, string>();
            _atom = atom;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, milliSecondDebounce);
            _timer.Tick += SendMessage;
        }

        public void MakeNextSendTCP()
        {
            _sendNextTCP = true;
        }

        public void Add(string id, string value)
        {
            if (!NetworkConnector.Instance.ModelLocked && (_atom.CanEdit == Atom.EditStatus.Yes || _atom.CanEdit == Atom.EditStatus.Maybe))
            {
                if (_atom.CanEdit == Atom.EditStatus.Maybe)
                {
                    NetworkConnector.Instance.RequestLock(_atom.ID);
                    NetworkConnector.Instance.ModelIntermediate.CheckLocks(_atom.ID);
                }
                if (!_timing)
                {
                    _timing = true;
                    _dict.Add(id, value);
                    _timer.Start();
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

        private async void SendMessage(object sender, object e)
        {
            _timer.Stop();
            if (_atom.CanEdit == Atom.EditStatus.Yes || _atom.CanEdit == Atom.EditStatus.Maybe)
            {
                _dict.Add("id", _atom.ID);
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
            _timing = false;
            _dict.Clear();
        }
    }
}
