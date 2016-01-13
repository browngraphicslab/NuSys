using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    public class NetworkUser
    {
        #region Public Variables
        public string IP { get; private set; }

        public Color Color
        {
            get { return _colorSet ? _color : GetColor(); }
        }
        public string Name;

        public delegate void UserRemovedEventHandler();
        public event UserRemovedEventHandler OnUserRemoved;
        #endregion Public Variables
        #region Private Variables
        private Color _color;
        private bool _colorSet;
        #endregion Private Variables
        public NetworkUser(string ip)
        {
            IP = ip;
            SessionController.Instance.NuSysNetworkSession.NetworkMembers.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems.Contains(IP))
                {
                    OnUserRemoved?.Invoke();
                }
            };
        }
        private Color GetColor()
        {
            try
            {
                var number = Int32.Parse(IP.Replace(@".", ""));
                int r = 10 + ((int) number%210);
                int g = 10 + ((int) (number*Int32.Parse(IP[IP.Length - 1].ToString())%210));
                int b = 10 + ((int) (number*r*g*Int32.Parse(IP[IP.Length - 1].ToString())%245));
                _color = Color.FromArgb((byte) 200, (byte) r, (byte) g, (byte) b);
            }
            catch (Exception e)
            {
                _color = Colors.DarkTurquoise;
            }
            return _color;
        }
    }
}
