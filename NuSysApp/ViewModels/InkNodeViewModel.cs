using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Windows.UI.Xaml.Media;



namespace NuSysApp
{
    /// <summary>
    /// class for ink drawing nodes
    /// s
    /// parameters: workspaceviewmodel
    /// </summary>
    public class InkNodeViewModel : NodeViewModel
    {
        public InkNodeViewModel(InkModel model, WorkspaceViewModel vm, string id): base(model, vm,id)
        {
            this.Model.ID = id;
            this.View = new InkNodeView2(this);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize;
            this.NodeType = NodeType.Ink;
            this.IsSelected = false;
            this.IsEditing = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175,173,216,230));
        }
    }
}
