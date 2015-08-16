using System;
using System.Collections.Generic;
using System.Xml;
using Windows.UI.Xaml.Media;



namespace NuSysApp
{
    /// <summary>
    /// class for ink drawing nodes
    /// s
    /// parameters: workspaceviewmodel
    /// </summary>
    public class InkNodeViewModel : NodeViewModel
    {

        public InkNodeViewModel(WorkspaceViewModel vm, string id): base(vm,id)
        {
            this.View = new InkNodeView2(this);
            this.Model = new Node(id);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize; 
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public override string CreateXML()
        {
            string XML = "";
            Node currModel = (Node)this.Model;
            XML = XML + "<" + " id='" + currModel.ID + "' x='" + (int)currModel.Transform.Matrix.OffsetX +
                    "' y='" + (int)currModel.Transform.Matrix.OffsetY + "' width='" + (int)currModel.Width + "' height='" + (int)currModel.Height +
                    "'content='" + currModel.Content + "'>";
            return XML;

    }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            Node currModel = this.Model;

            //Main XmlElement 
            XmlElement inkNode = doc.CreateElement(string.Empty, currModel.GetType().ToString(), string.Empty); //TODO: Change how we determine node type for name
            doc.AppendChild(inkNode);

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                inkNode.SetAttributeNode(attr);
            }

            return inkNode;
        }
    }
}
