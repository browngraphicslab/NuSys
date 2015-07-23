﻿using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    /// <summary>
    /// 
    /// NodeViewModel class
    /// 
    /// Parameters: takes in the workspace
    /// </summary>
    public abstract class NodeViewModel : AtomViewModel
    {
        #region Private Members      

        private Color _color; //currently unused

        private MatrixTransform _transform;

        #endregion Private Members

        protected NodeViewModel(WorkspaceViewModel vm): base(vm)
        {
        }

        #region Node Manipulations

        public override void Remove()
        {
            WorkSpaceViewModel.DeleteNode(this);
        }

        public void Translate(double dx, double dy)
        {
            var transMat = ((MatrixTransform) this.View.RenderTransform).Matrix;
            transMat.OffsetX += dx;
            transMat.OffsetY += dy;
            Transform = new MatrixTransform();
            this.Transform.Matrix = transMat;
            this.UpdateAnchor();
            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

        /// <summary>
        /// Updates the anchor points (central points) of the node when it is transformed. Also updates the attached links.
        /// </summary>
        public override void UpdateAnchor()
        {
            this.AnchorX = (int) (this.X + this.Transform.Matrix.OffsetX + this.Width/2);
            this.AnchorY = (int) (this.Y + this.Transform.Matrix.OffsetY + this.Height/2);
            this.Anchor = new Point(this.AnchorX, this.AnchorY);
        }

        /// <summary>
        /// Resizes the node. Eventually this should use a scale transformation instead.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Resize(double dx, double dy)
        {
            this.Width += dx;
            this.Height += dy;
            this.UpdateAnchor();
        }

        #endregion Node Manipulations

        #region Public Properties

        public MatrixTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;

                RaisePropertyChanged("Transform");
            }
        }

        /// <summary>
        /// color of node
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;

                RaisePropertyChanged("Color");
            }
        }

        #endregion Public Properties
    }
}