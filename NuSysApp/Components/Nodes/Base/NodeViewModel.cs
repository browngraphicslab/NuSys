using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class NodeViewModel : AtomViewModel
    {
        private bool _isEditing, _isEditingInk;
        private CompositeTransform _inkScale;

        protected NodeViewModel(NodeModel model) : base(model)
        {
            InkScale = new CompositeTransform { ScaleX = 1, ScaleY = 1 };
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

        public override void Resize(double dx, double dy)
        {
            CompositeTransform ct = InkScale;
            ct.ScaleX *= (Width + dx / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleX) / Width;
            ct.ScaleY *= (Height + dy / SessionController.Instance.ActiveWorkspace.CompositeTransform.ScaleY) / Height;
            InkScale = ct;
            base.Resize(dx, dy);
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

        public NodeType NodeType
        {
            get { return ((NodeModel) Model).NodeType; }
        }

        public string ContentId
        {
            get { return ((NodeModel)Model).ContentId; }
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