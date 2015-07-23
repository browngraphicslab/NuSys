using System;
using System.Collections.Generic;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class RichTextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private RichTextNode _node;
        private string _data;
        private UserControl _view;

        #endregion Private Members

        public RichTextNodeViewModel(WorkspaceViewModel workSpaceViewModel) : base(workSpaceViewModel)
        {
            _node = new RichTextNode("Hello oOrld", 0);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DEFAULT_NODE_SIZE; //width set in /MISC/Constants.cs
            this.Height = Constants.DEFAULT_NODE_SIZE; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;

            this.View = new RichTextNodeView(this);
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
                RichEditBox rtb = (RichEditBox) this.View.FindName("textBlock");
                rtb.Document.SetText(TextSetOptions.FormatRtf, _data);
            }
        }
        #endregion Public Properties
    }
}