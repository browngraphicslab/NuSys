using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private readonly TextNode _node;
        private string _data;
        #endregion Private Members

        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel) : base(workSpaceViewModel)
        {
            _node = new TextNode("Hello oOrld", 0);
     
            //_node.Text = this.Data;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.View = new TextNodeView2(this);
            this.Data = "Enter text here";
        }

        #region Public Properties

        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                RaisePropertyChanged("Data");
                _node.Text = _data; //Remove once model is actually integrated
            }
        }


        #endregion Public Properties
    }
}