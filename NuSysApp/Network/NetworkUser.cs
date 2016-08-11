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
        private LibraryElementController _controller = null;
        #endregion Private Variables
        public NetworkUser(string id, Dictionary<string,object> dict = null)
        {
            ID = id;
            Name = id;
        }
        private Color GetColor()
        {
            _color = MediaUtil.GetHashColorFromString(ID);
            _colorSet = true;
            return _color;
        }
        public void Remove()
        {
            OnUserRemoved?.Invoke();
        }
        public void SetUserController(LibraryElementController controller)
        {
            //controller.SetNetworkUser(null);

            if (controller != _controller)
            {
                if (_controller != null)
                {
                    _controller.SetNetworkUser(null);
                }
                if (controller != null)
                {
                    controller.SetNetworkUser(this);
                }
                _controller = controller;
            }
            //controller.SetNetworkUser(null);

            //if (controller != _controller)
            //{
            //    if (_controller != null)
            //    {
            //        _controller.SetNetworkUser(null);
            //    }
            //    if(controller != null)
            //    {
            //        controller.SetNetworkUser(this);
            //    }
            //    _controller = controller;
            //}
        }
    }
}
