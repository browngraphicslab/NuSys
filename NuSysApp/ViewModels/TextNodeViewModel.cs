using System;
using System.Collections.Generic;
using System.Xml;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : NodeViewModel
    {
        #region Private Members

        #endregion Private Members

        public TextNodeViewModel(WorkspaceViewModel workSpaceViewModel, string text) : base(workSpaceViewModel)
        {
            this.Model = new TextNode(text ?? "Enter text here", 0);
            this.View = new TextNodeView2(this);    
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; //width set in /MISC/Constants.cs
            this.Height = Constants.DefaultNodeSize; //height set in /MISC/Constants.cs
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.NodeType = Constants.NodeType.text;
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
            
            //Text
            XmlAttribute text = doc.CreateAttribute("text");
            text.Value = currModel.Text;
            textNode.SetAttributeNode(text);

            return textNode;
           
        }

        #endregion Public Properties
    }
}