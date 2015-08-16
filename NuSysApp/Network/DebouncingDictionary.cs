using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuSysApp.Network
{
    class DebouncingDictionary
    {
        private Dictionary<string, string> _dict;
        private int _debounceTime = 100;
        private bool _timing = false;
        private Timer _timer;
        private string _atomID;
        public DebouncingDictionary(string atomID)
        {
            _dict = new Dictionary<string, string>();
            _atomID = atomID;
        }
        public DebouncingDictionary(string atomID, int milliSecondDebounce)
        {
            _debounceTime = milliSecondDebounce;
            _dict = new Dictionary<string, string>();
            _atomID = atomID;
        }

        public void SetDebounceTime(int milliSecondDebounce)
        {
            _debounceTime = milliSecondDebounce;
        }

        public void Add(string id, string value)
        {
            if (!_timing)
            {
                _timing = true;
                _dict.Add(id, value);
                _timer = new Timer(new TimerCallback(sendMessage), null, _debounceTime, Timeout.Infinite);
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

        private void sendMessage(object stateInfo)
        {
            string message = MakeSubMessageFromDict(_dict);
            //_workspaceModel.UpdateNetwork(message);
            _timing = false;
            _dict.Clear();
        }
        private string MakeSubMessageFromDict(Dictionary<string, string> dict)
        {
            string m = "<";
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                m += kvp.Key + "=" + kvp.Value + ",";
            }
            m+="id="+_atomID+">";
            return m;
        }
    }
}
