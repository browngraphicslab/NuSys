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
        private HashSet<AtomModel> _modelsInUse = new HashSet<AtomModel>(); 
        #endregion Private Variables
        public NetworkUser(string ip)
        {
            IP = ip;
            SessionController.Instance.NuSysNetworkSession.OnNetworkUserDropped += delegate (NetworkUser user)
            {
                if (user.IP == IP)
                {
                    OnUserRemoved?.Invoke();
                    foreach (var atom in _modelsInUse)
                    {
                        atom.LastNetworkUser = null;
                    }
                    _modelsInUse.Clear();
                }
            };
        }
        private Color GetColor()
        {
            try
            {
                var number = Int64.Parse(IP.Replace(@".", ""));
                var start = 2*(Int64.Parse(IP[IP.Length - 1].ToString()) + 1);

                number += start*2*number; 

                var mod = 250 - start;

                int r = (int)Math.Abs(start + ((int) number%mod));
                int b = (int)Math.Abs(start + ((int) (number*Int64.Parse(IP[IP.Length - 1].ToString())% mod)));
                int g = (int)Math.Abs(start + ((int) ((start*number*r )% mod)));
                _color = Color.FromArgb((byte) 200, (byte) r, (byte) g, (byte) b);
            }
            catch (Exception e)
            {
                _color = Colors.Black;
            }
            return _color;
        }

        public void AddAtomInUse(AtomModel model)
        {
            foreach (var atom in _modelsInUse)
            {
                atom.LastNetworkUser = null;
            }
            _modelsInUse.Add(model);
        }

        public void RemoveAtomInUse(AtomModel model)
        {
            _modelsInUse.Remove(model);
        }
    }
}
