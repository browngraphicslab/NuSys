using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        #endregion Private Members

        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel, string text, int id) : base(workSpaceViewModel, id)
        {
            this.Model = new TextNode(text ?? "Enter text here", id);
            this.View = new TextNodeView2(this);  
            this.Transform = new MatrixTransform();
            this.Width =500; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = Constants.NodeType.text;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 255, 235, 205));
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


        public override XmlElement WriteXML(XmlDocument doc)
        {

            TextNode currModel = (TextNode)this.Model;

            //XmlElement 
            XmlElement textNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach(XmlAttribute attr in basicXml)
            {
                textNode.SetAttributeNode(attr);
            }

            //Text (TODO: Uncomment this section when we figure out how to store just the string of the textnode)
            ////XmlAttribute text = doc.CreateAttribute("text");
            ////text.Value = currModel.Text;
            ////textNode.SetAttributeNode(text);

            return textNode;       
        }

        #endregion Public Properties
    }
}