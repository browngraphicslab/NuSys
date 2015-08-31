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
        public InkNodeViewModel(InkModel model, WorkspaceViewModel vm): base(model, vm)
        {
            this.Model.ID = id;
            var view = new InkNodeView2(this);
            this.View = view;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize;
            this.NodeType = NodeType.Ink;
            this.IsSelected = false;
            this.IsEditing = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175,173,216,230));
            view.PromoteStrokes((model as InkModel).PolyLines.ToArray());
        }
    }
}
