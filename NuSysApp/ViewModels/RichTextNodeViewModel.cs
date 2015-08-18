using System;
using System.Collections.Generic;
using System.Xml;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class RichTextNodeViewModel : NodeViewModel
    {
        #region Private Members

        private RichTextNode _node;
        private string _data;

        #endregion Private Members

        public RichTextNodeViewModel(WorkspaceViewModel workSpaceViewModel, int id) : base(workSpaceViewModel, id)
        {
            _node = new RichTextNode("Hello oOrld", id);
            this.Model = _node;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = Constants.NodeType.richText;
            this.View = new RichTextNodeView2(this);
        }

        #region Public Properties

        /// <summary>
        /// data contained by text node
        /// </summary>
        public string Data
        {
            get { return ((RichTextNode)Model).Data; }
            set
            {
                ((RichTextNode)Model).Data = value;
                RaisePropertyChanged("Data");
                var rtb = (RichEditBox) this.View.FindName("textBlock"); //TO DO: GET RID OF THIS. NEVER GRAB THE VIEW FROM WITHIN THE VIEWMODEL!! (- Nick)
                rtb.Document.SetText(TextSetOptions.FormatRtf, _data);
            }
        }

    }
    #endregion Public Properties

}