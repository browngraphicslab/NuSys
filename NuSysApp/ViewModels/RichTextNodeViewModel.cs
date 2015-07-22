using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class RichTextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private RichTextNode _node;
        private List<Block> _data;
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
        public List<Block> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                RaisePropertyChanged("Data");
                RichTextBlock rtb = (RichTextBlock) this.View.FindName("textBlock");

                foreach (var block in _data)
                {
                    rtb.Blocks.Add(block);
                }
            }
        }

        public override UserControl View
        {
            get { return _view; }
            set
            {
                if (_view == value)
                {
                    return;
                }

                _view = value;

                RaisePropertyChanged("View");
            }
        }

        #endregion Public Properties
    }
}