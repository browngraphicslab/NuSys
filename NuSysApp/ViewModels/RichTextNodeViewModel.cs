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

        public RichTextNodeViewModel(WorkspaceViewModel workSpaceViewModel, string id) : base(workSpaceViewModel, id)
        {
            _node = new RichTextNode("Hello oOrld", id);
            this.Model = _node;
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.View = new RichTextNodeView2(this);
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
                var rtb = (RichEditBox) this.View.FindName("textBlock"); //TO DO: GET RID OF THIS. NEVER GRAB THE VIEW FROM WITHIN THE VIEWMODEL!! (- Nick)
                rtb.Document.SetText(TextSetOptions.FormatRtf, _data);
            }
        }

        public override string CreateXML()
        { 
            string XML = "";
            RichTextNode currModel = (RichTextNode) _node;
            XML = XML + "<" + " id='" + currModel.ID + "' x='" + (int)currModel.Transform.Matrix.OffsetX +
                    "' y='" + (int)currModel.Transform.Matrix.OffsetY + "' width='" + (int)currModel.Width + "' height='" + (int)currModel.Height +
                    "'Text='" + currModel.Text + "'content='" + currModel.Content + "'>";
            return XML;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            RichTextNode currModel = (RichTextNode)this.Model;

            //XmlElement 
            XmlElement richTextNode = doc.CreateElement(string.Empty, currModel.GetType().ToString(), string.Empty); //TODO: Change how we determine node type for name
            doc.AppendChild(richTextNode);

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                richTextNode.SetAttributeNode(attr);
            }

            //Text
            XmlAttribute text = doc.CreateAttribute("text");
            text.Value = currModel.Text;
            richTextNode.SetAttributeNode(text);

            return richTextNode;
        }
    }
    #endregion Public Properties

}