using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class FloatingMenuModel
    {
        #region Events and Handlers
        public delegate void SetActiveEventHandler(object source, AddToGroupEventArgs e);
        public event SetActiveEventHandler OnSetActive;
        #endregion Events and Handlers

        public FloatingMenuModel()
        {

        }
    }
}
