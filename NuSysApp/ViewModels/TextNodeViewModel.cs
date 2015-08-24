using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        #endregion Private Members
        public TextNodeViewModel(TextNode model, WorkspaceViewModel workSpaceViewModel, string text, string id) : base(model, workSpaceViewModel, id)
        {
           
            this.View = new TextNodeView2(this);  
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = NodeType.Text;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
            //this.View = new TextNodeView2(this);//TODO < whut is this? <IDK duuuude
            ((TextNode) this.Model).OnTextChanged += TextChangedHandler;
        }

       
        private void TextChangedHandler(object source, TextChangedEventArgs e)
        {
            this.Data = ((TextNode)this.Model).Text;
        }
       
        
        
        #region Public Properties

        private string _data;
        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                ((TextNode) this.Model).Text = value;
                RaisePropertyChanged("Data");
            }
        }

        #endregion Public Properties
    }
}