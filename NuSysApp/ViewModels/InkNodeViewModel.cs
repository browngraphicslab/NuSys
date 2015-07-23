using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;



namespace NuSysApp
{
    /// <summary>
    /// class for ink drawing nodes
    /// 
    /// parameters: workspaceviewmodel
    /// </summary>
    public class InkNodeViewModel : NodeViewModel
    {

        public InkNodeViewModel(WorkspaceViewModel vm): base(vm)
        {
            this.View = new InkNodeView(this);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DEFAULT_NODE_SIZE; //using the constants class to set size instead of always using xaml
            this.Height = Constants.DEFAULT_NODE_SIZE; // same here
            this.IsSelected = false; // sets the entire node to be not selected so it can be manipulated when initially created (see more in NuStarterProject/MISC/Constants.cs)
            this.IsEditing = false; //sets the ink to initially be disabled 
        }

       
    }
}
