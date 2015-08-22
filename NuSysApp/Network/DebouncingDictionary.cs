﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp.Network
{
    public class DebouncingDictionary
    {
        private Dictionary<string, string> _dict;
        private bool _timing = false;
        private DispatcherTimer _timer;
        private string _atomID;
        private Atom _atom;
        public DebouncingDictionary(Atom atom)
        {
            _dict = new Dictionary<string, string>();
            _atomID = atom.ID;
            _atom = atom;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += SendMessage;

        }
        public DebouncingDictionary(string atomID, int milliSecondDebounce)
        {
            _dict = new Dictionary<string, string>();
            _atomID = atomID;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, milliSecondDebounce);
            _timer.Tick += SendMessage;
        }


        public void Add(string id, string value)
        {
            if (!NetworkConnector.Instance.ModelLocked && (_atom.CanEdit == Atom.EditStatus.Yes || _atom.CanEdit == Atom.EditStatus.Maybe))
            {
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
                _dict.Add("id", _atomID);
                await NetworkConnector.Instance.QuickUpdateAtom(_dict);
            }
            _timing = false;
            _dict.Clear();
        }
    }
}
