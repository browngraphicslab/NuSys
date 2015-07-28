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

        public InkNodeViewModel(WorkspaceViewModel vm): base(vm)
        {
            this.View = new InkNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize; 
            this.IsSelected = false;
            this.IsEditing = false; 
        }

       
    }
}
