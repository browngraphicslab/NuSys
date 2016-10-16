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
        
        #endregion Public Variables

        #region Private Variables
        private Color _color;
        private bool _colorSet = false;
        private LibraryElementController _controller = null;
        private string _currEditingControllerId = null;
        #endregion Private Variables


        public NetworkUser(string userId = null)
        {
            UserID = userId;
        }

        private Color GetColor()
        {
            _color = MediaUtil.GetHashColorFromString(DisplayName);
            _colorSet = true;
            return _color;
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

        public void SetNodeCurrentlyEditing(string controllerId)
        {
            if (controllerId != _currEditingControllerId)
            {
                if (_currEditingControllerId != null)
                {
                    SessionController.Instance.UserController.RemoveUser(_currEditingControllerId, this.UserID);
                    //SessionController.Instance.ControllerIdToUserIdList[controllerId].Remove(this.UserID);
                }
                //if (controllerId != null)
                //{
                //    SessionController.Instance.ControllerIdToUserIdList[controllerId].Remove(this.UserID);
                //}
                _currEditingControllerId = controllerId;
            }
        }
    }
}
