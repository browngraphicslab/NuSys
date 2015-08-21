﻿using NuSysApp.Models;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.UI.Core;

namespace NuSysApp
{
    public class XmlFileHelper
    {
        public XmlFileHelper() { }

        [Column("ID"), PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Column("toXml")]
        public String toXml { get; set; }

        public string XmlToString(XmlDocument xmlDoc)
        {
            return xmlDoc.OuterXml;
        }

        public XmlDocument StringToXml(string rawXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rawXml);
            return doc;
        }

        public NodeViewModel CreateNodeFromXml(WorkspaceViewModel vm, XmlNode node)
        {
            int ID = Convert.ToInt16(node.Attributes.GetNamedItem("id").Value);
            string currType = node.Attributes.GetNamedItem("nodeType").Value;
            double X = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
            double Y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
            double width = Convert.ToDouble(node.Attributes.GetNamedItem("width").Value);
            double height = Convert.ToDouble(node.Attributes.GetNamedItem("height").Value);
            NodeViewModel nodeVM = null; //Just a filler - gets reassigned in all cases
            switch (currType)
            {
                case "text":
                    //string text = node.Attributes.GetNamedItem("text").Value; TO DO: Uncomment this when we get rid of the encoding in the textnode
                    nodeVM = vm.CreateNewNode(ID, NodeType.Text, X, Y).Result;
                    nodeVM.Width = width;
                    nodeVM.Height = height;
                    nodeVM.ID = ID;
                    break;
                case "Image":
                    nodeVM = vm.CreateNewNode(ID, NodeType.Document, X, Y).Result;
                    break;
                case "Pdf":
                    nodeVM = vm.CreateNewNode(ID, NodeType.Document, X, Y).Result;
                    break;
                case "ink":
                    nodeVM = vm.CreateNewNode(ID, NodeType.Ink, X, Y).Result;
                    nodeVM.Width = width;
                    nodeVM.Height = height;
                    nodeVM.ID = ID;
                    break;
                default:
                    nodeVM = vm.CreateNewNode(ID, NodeType.Text, X, Y).Result;
                    break;
            }
            return nodeVM;
        }

        public void ParseXml(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;
            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name;
                int ID = Convert.ToInt16(node.Attributes.GetNamedItem("id").Value);
                switch (AtomType)
                {
                    case "Group":
                        double x = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
                        double y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            GroupViewModel groupVm = new GroupViewModel(vm, ID);
                            //vm.Model.AtomDict.Add(ID, groupVm);
                            foreach (XmlNode child in node.ChildNodes) //Groups have child nodes
                            {
                                NodeViewModel newVM = this.CreateNodeFromXml(vm, child);
                                vm.AtomViewList.Remove(newVM.View); // View has to be removed from workspace's AtomViewList so it can be added to the group's AtomViewList
                                groupVm.AddNode(newVM);
                                ((Node)newVM.Model).ParentGroup = ((Group)groupVm.Model);
                                newVM.ParentGroup = groupVm;
                            }
                            vm.NodeViewModelList.Add(groupVm);
                            vm.AtomViewList.Add(groupVm.View);
                            vm.PositionNode(groupVm, x, y);
                        });
                        break;
                    case "Node":
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            NodeViewModel nodeVm = this.CreateNodeFromXml(vm, node);
                        });
                        break;
                    case "Link":
                        vm.currId = ID;
                        int atomID1 = Convert.ToInt32(node.Attributes.GetNamedItem("atomID1").Value);
                        int atomID2 = Convert.ToInt32(node.Attributes.GetNamedItem("atomID2").Value);
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                           // AtomViewModel atom1Vm = vm.Model.AtomDict[atomID1];
                            //AtomViewModel atom2Vm = vm.Model.AtomDict[atomID2];
                            //inkViewModel newLinkVm = vm.CreateNewLink(ID, atom1Vm, atom2Vm);
                           // newLinkVm.ID = ID;

                            // create node annotation and attach it to the link
                            if (node.HasChildNodes)
                            {
                                XmlNode attachedAnnotation = node.ChildNodes[0];
                                int clippedParentID = Convert.ToInt32(attachedAnnotation.Attributes.GetNamedItem("ClippedParent").Value);
                                NodeViewModel nodeVm = this.CreateNodeFromXml(vm, attachedAnnotation);
                                //nodeVm.ClippedParent = vm.Model.AtomDict[clippedParentID];
                            }
                        });
                        break;
                }
            }
        }
    }
}
