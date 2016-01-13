using System;
using System.Collections.Generic;
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
        #endregion Public Variables
        #region Private Variables
        private Color _color;
        private bool _colorSet;
        #endregion Private Variables
        public NetworkUser(string ip)
        {
            IP = ip;
        }
        private Color GetColor()
        {
            try
            {
                var number = Int32.Parse(IP.Replace(@".", ""));
                int r = 10 + ((int) number%220);
                int g = 10 + ((int) (number*Int32.Parse(IP[IP.Length - 1].ToString())%220));
                int b = 10 + ((int) (number%Int32.Parse(IP[IP.Length - 1].ToString())%220));
                _color = Color.FromArgb((byte) 255, (byte) r, (byte) g, (byte) b);
            }
            catch (Exception e)
            {
                _color = Colors.DarkTurquoise;
            }
            return _color;
        }
    }
}
