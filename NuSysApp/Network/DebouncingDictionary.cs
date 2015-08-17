using System;
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
        public DebouncingDictionary(string atomID)
        {
            _dict = new Dictionary<string, string>();
            _atomID = atomID;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += sendMessage;

        }
        public DebouncingDictionary(string atomID, int milliSecondDebounce)
        {
            _dict = new Dictionary<string, string>();
            _atomID = atomID;
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, milliSecondDebounce);
            _timer.Tick += sendMessage;
        }


        public void Add(string id, string value)
        {
            if (!Globals.Network.WorkSpaceModel.Locked)
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

        private async void sendMessage(object sender, object e)
        {
            _timer.Stop();
            string message = MakeSubMessageFromDict(_dict);
            await Globals.Network.SendMassUDPMessage(message);
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
