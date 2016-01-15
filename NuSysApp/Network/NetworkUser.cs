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

                var start = 2*(Int32.Parse(IP[IP.Length - 1].ToString()) + 1);

                var mod = 250 - start;

                int r = Math.Abs(start + ((int) number%mod));
                int b = Math.Abs(start + ((int) (number*Int32.Parse(IP[IP.Length - 1].ToString())% mod)));
                int g = Math.Abs(start + ((int) (start*number*r*b*Int32.Parse(IP[IP.Length - 1].ToString())% mod)));
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
