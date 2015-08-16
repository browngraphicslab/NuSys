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
            Debug.WriteLine(xmlDoc.OuterXml);
            return xmlDoc.OuterXml;
        }

        public XmlDocument StringToXml(string rawXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rawXml);
            return doc;
        }

        public void ParseXml(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;
            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name;
                string ID = node.Attributes.GetNamedItem("id").Value;
                switch (AtomType)
                {
                    case "Node":
                        string currType = node.Attributes.GetNamedItem("nodeType").Value;
                        int X = Convert.ToInt32(node.Attributes.GetNamedItem("x").Value);
                        int Y = Convert.ToInt32(node.Attributes.GetNamedItem("y").Value);
                        int width = Convert.ToInt32(node.Attributes.GetNamedItem("width").Value);
                        int height = Convert.ToInt32(node.Attributes.GetNamedItem("height").Value);
                        switch (currType)
                        {
                            case "text":
                                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                   () =>
                                   {
                                       //string text = node.Attributes.GetNamedItem("text").Value; TO DO: Uncomment this when we get rid of the encoding in the textnode
                                       NodeViewModel newNodeVm = vm.CreateNewNode(ID,NodeType.Text, X, Y).Result;
                                       newNodeVm.Width = width;
                                       newNodeVm.Height = height;
                                       newNodeVm.ID = ID;
                                   });
                                break;
                            case "Image":
                                vm.CreateNewNode(ID,NodeType.Document, X, Y);
                                break;
                            case "Pdf":
                                vm.CreateNewNode(ID,NodeType.Document, X, Y);
                                break;
                            case "Ink":
                                vm.CreateNewNode(ID,NodeType.Ink, X, Y);
                                break;
                            case "RichText":
                                vm.CreateNewNode(ID,NodeType.Text, X, Y);
                                break;
                        }
                        break;
                    case "Link":
                        int atom1Id = Convert.ToInt32(node.Attributes.GetNamedItem("atom1").Value);
                        int atom2Id = Convert.ToInt32(node.Attributes.GetNamedItem("atom2").Value);
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                // LinkViewModel newLinkVm = vm.CreateNewLink(atom1Id, atom2Id).Result; TO-DO: A dict to map the ID with atom
                                //newLinkVm.ID = ID;
                            });
                        break;
                }

            }
        }
    }
}
