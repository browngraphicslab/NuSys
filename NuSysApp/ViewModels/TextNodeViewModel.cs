using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private TextNode _node;
        private string _data;
        private UserControl _view;

        #endregion Private Members

        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel) : base(workSpaceViewModel)
        {
            _node = new TextNode("Hello oOrld", 0);
            this.Data = "Enter text here";
            _node.Text = this.Data;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DEFAULT_NODE_SIZE; //width set in /MISC/Constants.cs
            this.Height = Constants.DEFAULT_NODE_SIZE; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;

            this.View = new TextNodeView(this);
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