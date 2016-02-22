using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class NodeViewModel : AtomViewModel
    {
        private bool _isEditing, _isEditingInk;
        private CompositeTransform _inkScale;
        private SolidColorBrush _userColor;

        protected NodeViewModel(ElementInstanceController controller) : base(controller)
        {
            InkScale = new CompositeTransform { ScaleX = 1, ScaleY = 1 };

            controller.UserChanged += delegate (NetworkUser user)
            {
                _userColor = user != null ? new SolidColorBrush(user.Color) : new SolidColorBrush(Colors.Transparent);
            };
        }
        
        #region Node Manipulations

        public override void Remove()
        {
            if (IsSelected)
            {
                //TODO: re-add
                SessionController.Instance.ActiveWorkspace.ClearSelection();
            }
        }

        public override void SetSize(double width, double height)
        {
            var dx = width/Width -1;
            var dy = height/Height -1;
   
            CompositeTransform t = InkScale;
            t.ScaleX += dx;
            t.ScaleY += dy;
            InkScale = t;
            base.SetSize(width, height);
        }

        public void ToggleEditing()
        {
            IsEditing = !IsEditing;
        }

        public void ToggleEditingInk()
        {
            IsEditingInk = !IsEditingInk;
        }

   
        #endregion Node Manipulations

        #region Public Properties

        public SolidColorBrush UserColor
        {
            get { return _userColor; }
            set
            {
                if (_userColor == value)
                {
                    return;
                }
                
                RaisePropertyChanged("UserColor");
            }

        }
        
        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (_isEditing == value)
                {
                    return;
                }
                _isEditing = value;
                RaisePropertyChanged("IsEditing");
            }
        }

        public bool IsEditingInk
        {
            get { return _isEditingInk; }
            set
            {
                if (_isEditingInk == value)
                {
                    return;
                }
                _isEditingInk = value;
                RaisePropertyChanged("IsEditingInk");
            }
        }

        public ElementType ElementType
        {
            get { return ((ElementInstanceModel) Model).ElementType; }
        }

        public string ContentId
        {
            get { return ((ElementInstanceModel)Model).ContentId; }
        }

        public CompositeTransform InkScale
        {
            get { return _inkScale; }
            set
            {
                _inkScale = value;
                RaisePropertyChanged("InkScale");
            }
        }

        #endregion Public Properties
    }
}