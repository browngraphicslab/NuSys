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
        public string ID { get; private set; }

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
        private bool _colorSet = false;
        private ElementController _controller = null;
        #endregion Private Variables
        public NetworkUser(string id, Dictionary<string,object> dict = null)
        {
            ID = id;
            Name = id;
        }
        private Color GetColor()
        {
            try
            {
                var idHash = WaitingRoomView.Encrypt(ID);
                long number = Math.Abs(BitConverter.ToInt64(idHash,0));
                long r1 = BitConverter.ToInt64(idHash,1);
                long r2 = BitConverter.ToInt64(idHash,2); ;

                var mod = 250;

                int r = (int)Math.Abs(((int)number % mod));
                int b = (int)Math.Abs((r1 * number) % mod);
                int g = (int)Math.Abs((r2 * number) % mod);
                _color = Color.FromArgb((byte)200, (byte)r, (byte)g, (byte)b);
                _colorSet = true;
                /*
                var number = Int64.Parse(ID.Replace(@".", ""));
                var start = 2*(Int64.Parse(IP[IP.Length - 1].ToString()) + 1);

                number += start*2*number; 

                var mod = 250 - start;

                int r = (int)Math.Abs(start + ((int) number%mod));
                int b = (int)Math.Abs(start + ((int) (number*Int64.Parse(IP[IP.Length - 1].ToString())% mod)));
                int g = (int)Math.Abs(start + ((int) ((start*number*r )% mod)));
                _color = Color.FromArgb((byte) 200, (byte) r, (byte) g, (byte) b);*/
            }
            catch (Exception e)
            {
                _color = Colors.Black;
            }
            return _color;
        }
        public void Remove()
        {
            OnUserRemoved?.Invoke();
        }
        public void SetUserController(ElementController controller)
        {
            if (controller != _controller)
            {
                if (_controller != null)
                {
                    _controller.SetNetworkUser(null);
                }
                if(controller != null)
                {
                    controller.SetNetworkUser(this);
                }
                _controller = controller;
            }
        }
    }
}
