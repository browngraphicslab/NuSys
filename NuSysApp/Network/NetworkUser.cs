using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NusysIntermediate;

namespace NuSysApp
{
    public class NetworkUser : BaseClient
    {
        #region Public Variables
        public Color Color
        {
            get { return _colorSet ? _color : GetColor(); }
        }

        public delegate void UserRemovedEventHandler();
        public event UserRemovedEventHandler OnUserRemoved;
        #endregion Public Variables

        #region Private Variables
        private Color _color;
        private bool _colorSet = false;
        private LibraryElementController _controller = null;
        #endregion Private Variables


        public NetworkUser(string userId = null)
        {
            UserID = userId;
        }

        private Color GetColor()
        {
            _color = MediaUtil.GetHashColorFromString(UserID);
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
        }
    }
}
