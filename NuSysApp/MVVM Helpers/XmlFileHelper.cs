using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
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

        public void CreateXml()
        {

        }

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
            string currType = node.Attributes.GetNamedItem("nodeType").Value;
            double X = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
            double Y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
            double width = Convert.ToDouble(node.Attributes.GetNamedItem("width").Value);
            double height = Convert.ToDouble(node.Attributes.GetNamedItem("height").Value);
            NodeViewModel nodeVM = new TextNodeViewModel(vm, string.Empty, 0); //Just a filler - gets reassigned in all cases
            switch (currType)
            {
                case "text":
                    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                       () =>
                       {
                          //string text = node.Attributes.GetNamedItem("text").Value; TO DO: Uncomment this when we get rid of the encoding in the textnode
                           nodeVM = vm.CreateNewNode(NodeType.Text, X, Y).Result;
                           nodeVM.Width = width;
                           nodeVM.Height = height;
                           nodeVM.ID = ID;
                       });
                    break;
                case "Image":
                    nodeVM = vm.CreateNewNode(NodeType.Document, X, Y).Result;
                    break;
                case "Pdf":
                    nodeVM = vm.CreateNewNode(NodeType.Document, X, Y).Result;
                    break;
                case "ink":
                    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            nodeVM = vm.CreateNewNode(NodeType.Ink, X, Y).Result;
                            nodeVM.Width = width;
                            nodeVM.Height = height;
                            nodeVM.ID = ID;
                        });
                    break;
                default://case "RichText":
                   nodeVM= vm.CreateNewNode(NodeType.Text, X, Y).Result;
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
                        string type = node.Attributes.GetNamedItem("nodeType").Value;
                        double x = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
                        double y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
                        double w = Convert.ToDouble(node.Attributes.GetNamedItem("width").Value);
                        double h = Convert.ToDouble(node.Attributes.GetNamedItem("height").Value);
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            GroupViewModel groupVm = new GroupViewModel(vm, ID);

                            foreach (XmlNode child in node.ChildNodes) //Groups have child nodes
                            {
                                NodeViewModel newVM = this.CreateNodeFromXml(vm, child);
                                groupVm.AddNode(newVM);
                            }
                            //groupVm.Width = w;
                            //groupVm.Height = h;
                            vm.NodeViewModelList.Add(groupVm);
                            vm.AtomViewList.Add(groupVm.View);
                            vm.PositionNode(groupVm, x, y);
                            
                        });
                        break;
                    case "Node":
                        this.CreateNodeFromXml(vm, node);
                        /*string currType = node.Attributes.GetNamedItem("nodeType").Value;
                        double X = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
                        double Y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
                        double width = Convert.ToDouble(node.Attributes.GetNamedItem("width").Value);
                        double height = Convert.ToDouble(node.Attributes.GetNamedItem("height").Value);
                        switch (currType)
                        {
                            case "text":
                                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                   () =>
                                   {
                                       //string text = node.Attributes.GetNamedItem("text").Value; TO DO: Uncomment this when we get rid of the encoding in the textnode
                                       NodeViewModel newTextVm = vm.CreateNewNode(NodeType.Text, X, Y).Result;
                                       newTextVm.Width = width;
                                       newTextVm.Height = height;
                                       newTextVm.ID = ID;
                                   });
                                break;
                            case "Image":
                                vm.CreateNewNode(NodeType.Document, X, Y);
                                break;
                            case "Pdf":
                                vm.CreateNewNode(NodeType.Document, X, Y);
                                break;
                            case "ink":
                                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                    () =>
                                    {
                                        NodeViewModel newInkVm = vm.CreateNewNode(NodeType.Ink, X, Y).Result;
                                        newInkVm.Width = width;
                                        newInkVm.Height = height;
                                        newInkVm.ID = ID;
                                    });
                                break;
                            case "RichText":
                                vm.CreateNewNode(NodeType.Text, X, Y);
                                break;    
                        }*/
                        break;
                    case "Link":
                        int atomID1 = Convert.ToInt32(node.Attributes.GetNamedItem("atomID1").Value);
                        int atomID2 = Convert.ToInt32(node.Attributes.GetNamedItem("atomID2").Value);
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                AtomViewModel atom1Vm = vm.Model.AtomDict[atomID1];
                                AtomViewModel atom2Vm = vm.Model.AtomDict[atomID2];
                                LinkViewModel newLinkVm = vm.CreateNewLink(atom1Vm, atom2Vm);
                                newLinkVm.ID = ID;
                            });
                        break;
                }

            }
        }
    }
}
