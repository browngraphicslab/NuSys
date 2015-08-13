using System;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private readonly TextNode _node;
        private string _data;
        #endregion Private Members

        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel, string text) : base(workSpaceViewModel)
        {
            this.Model = new TextNode(text ?? "Enter text here", 0);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.View = new TextNodeView2(this);
        }

        #region Public Properties

        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return ((TextNode)this.Model).Text; }
            set
            {
                ((TextNode)this.Model).Text = value;
                RaisePropertyChanged("Data");
            }
        }

        public override string CreateXML()
        {
            string XML = "";
            TextNode currModel = (TextNode)this.Model;
            XML = XML + "<" + " id='" + currModel.ID + "' x='" + (int)currModel.Transform.Matrix.OffsetX +
                    "' y='" + (int)currModel.Transform.Matrix.OffsetY + "' width='" + (int)currModel.Width + "' height='" + (int)currModel.Height +
                    "'Text='" + currModel.Text + "'content='" + currModel.Content + "'>";
            return XML;
        }

        #endregion Public Properties
    }
}