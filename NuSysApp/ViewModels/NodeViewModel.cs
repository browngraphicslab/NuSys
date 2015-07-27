using System.Collections.ObjectModel;
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
            transMat.OffsetX += dx / WorkSpaceViewModel.CompositeTransform.ScaleX;
            transMat.OffsetY += dy / WorkSpaceViewModel.CompositeTransform.ScaleY;
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
        public virtual void Resize(double dx, double dy)
        {
            this.Width += dx / WorkSpaceViewModel.CompositeTransform.ScaleX;
            this.Height += dy / WorkSpaceViewModel.CompositeTransform.ScaleY;
            this.UpdateAnchor();
        }


        public void CreateAnnotation()
        {
            this.WorkSpaceViewModel.CheckForNodeLinkIntersections(this);
        }
        #endregion Node Manipulations

        #region Public Properties

        

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