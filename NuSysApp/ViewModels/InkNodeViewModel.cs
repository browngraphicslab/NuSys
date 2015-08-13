using System;
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
            this.View = new InkNodeView2(this);
            this.Model = new Node(0);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize; 
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public override string CreateXML()
        {
            string XML = "";
            Node currModel = (Node)this.Model;
            XML = XML + "<" + " id='" + currModel.ID + "' x='" + (int)currModel.Transform.Matrix.OffsetX +
                    "' y='" + (int)currModel.Transform.Matrix.OffsetY + "' width='" + (int)currModel.Width + "' height='" + (int)currModel.Height +
                    "'content='" + currModel.Content + "'>";
            return XML;

    }
}
}
