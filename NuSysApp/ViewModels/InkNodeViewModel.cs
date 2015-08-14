﻿using System;
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

        public InkNodeViewModel(WorkspaceViewModel vm): base(vm)
        {
            this.View = new InkNodeView2(this);
            this.Model = new Node(0);
            this.Transform = new MatrixTransform();
            this.Width = Constants.DefaultNodeSize; 
            this.Height = Constants.DefaultNodeSize;
            this.NodeType = Constants.NodeType.ink; 
            this.IsSelected = false;
            this.IsEditing = false;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            Atom currModel = this.Model;

            //Main XmlElement 
            XmlElement inkNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

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
