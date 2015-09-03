using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SQLite.Net.Async;
using SQLite.Net.Attributes;
using System.Diagnostics;
using System.Net;

namespace NuSysApp
{
    public class XmlFileHelper
    {
        private List<Dictionary<string, string>> _atomUpdateDicts;
        private List<string> _createdNodeList;
        private bool _allNodeCreated;

        public XmlFileHelper()
        {
            this.ID = "1"; // for testing purposes so that we can only load the last saved workspace
            _atomUpdateDicts = new List<Dictionary<string, string>>();
            _createdNodeList = new List<string>();
            _allNodeCreated = false;
        }

        [Column("ID")]
        public string ID { get; set; }

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

        /// <summary>
        /// Parse XML document to recreate the saved workspace
        /// </summary>
        /// <param name="vm">current workspace view model</param>
        /// <param name="doc">input XML document</param>
        /// <returns></returns>
        public async Task ParseXml(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;
            SQLiteAsyncConnection dbConnection = vm.myDB.DBConnection;

            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name,
                       ID       = Convert.ToString(node.Attributes.GetNamedItem("id").Value),
                       x, y;

                Dictionary<string, string> dict = new Dictionary<string, string>();

                switch (AtomType)
                {
                    case "Group":
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                x = node.Attributes.GetNamedItem("x").Value;
                                y = node.Attributes.GetNamedItem("y").Value;

                                List<string> NodeIdList = new List<string>();

                                /* have to split this into two because we need the first for-loop method to be 
                                   executed right away and not wait until the node creation is finished */
                                foreach (XmlNode child in node.ChildNodes)
                                {
                                    string nodeId = child.Attributes.GetNamedItem("id").Value;
                                    _createdNodeList.Add(nodeId);
                                    NodeIdList.Add(nodeId);
                                }
                                foreach (XmlNode child in node.ChildNodes)
                                {
                                    await this.CreateNodeFromXml(vm, child);
                                }

                                // create the group
                                await NetworkConnector.Instance.RequestMakeGroup(NodeIdList[0], NodeIdList[1], x, y, ID);

                                // append more nodes into the group if it contains more than two nodes
                                if (NodeIdList.Count > 2)
                                {
                                    Group group = vm.Model.IDToSendableDict[ID] as Group;
                                    for (int i = 2; i < NodeIdList.Count; i++)
                                    {
                                        Node currNode = vm.Model.IDToSendableDict[NodeIdList[i]] as Node;
                                        currNode.MoveToGroup(group);
                                    }
                                }
                            });
                        break;
                    case "Node":
                        _createdNodeList.Add(ID);
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                await this.CreateNodeFromXml(vm, node);
                               
                            });
                        Debug.WriteLine("node created");
                        break;
                    case "Pin":
                        x = node.Attributes.GetNamedItem("x").Value;
                        y = node.Attributes.GetNamedItem("y").Value;
                        string text = node.Attributes.GetNamedItem("text").Value;

                        dict.Add("text", text);
                        _atomUpdateDicts.Add(dict);

                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                await NetworkConnector.Instance.RequestMakePin(x, y, ID, dict);
                            });
                        break;
                    default:
                        break;
                }
            }
            Debug.WriteLine("Done switching");
            List<string> createdNodeListCopy = new List<string>();
            foreach (string id in _createdNodeList) { createdNodeListCopy.Add(id); }

            // loop through this method to make sure that all nodes have been reloaded
            while (_allNodeCreated == false)
            {
                this.CheckNodeCreation(vm, doc, createdNodeListCopy);

                // create all the links once all nodes have been created
                if (createdNodeListCopy.Count == 0)
                {
                    await this.CreateLinks(vm, doc);
                    _allNodeCreated = true;
                }
            }

            // this updates the properties (height, width, etc.) of each atom once it has been created
            foreach (Dictionary<string, string> dict in _atomUpdateDicts)
            {
                await NetworkConnector.Instance.QuickUpdateAtom(dict);
            }
        }

        /// <summary>
        /// Helper method used in ParseXml method that parses just the nodes
        /// </summary>
        /// <param name="vm">Current workspace view model</param>
        /// <param name="node">XML node that contains info of a node in the saved workspace</param>
        /// <returns></returns>
        public async Task CreateNodeFromXml(WorkspaceViewModel vm, XmlNode node)
        {
            string ID = node.Attributes.GetNamedItem("id").Value;
            string currType = node.Attributes.GetNamedItem("nodeType").Value;
            string X = node.Attributes.GetNamedItem("x").Value;
            string Y = node.Attributes.GetNamedItem("y").Value;
            string width = node.Attributes.GetNamedItem("width").Value;
            string height = node.Attributes.GetNamedItem("height").Value;

            NodeViewModel nodeVM = null; //Just a filler - gets reassigned in all cases

            Dictionary<string, string> dict = new Dictionary<string, string>();

            // look up the content of the current atom in the database
            var query = vm.myDB.DBConnection.Table<Content>().Where(v => v.assocAtomID == ID);
            var res = await query.FirstOrDefaultAsync();

            byte[] byteData = null;
            string byteToString = null;

            if (res != null)
            {
                byteData = res.Data;
                byteToString = (currType == "Text") ? (Encoding.UTF8.GetString(byteData)) : (Convert.ToBase64String(byteData));
            }
            dict.Add("width", width);
            dict.Add("height", height);
            switch (currType)
            {
                case "Text":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), byteToString, ID, dict);
                    break;
                case "Image":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Image.ToString(), byteToString, ID, dict);
                    break;
                case "PDF":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.PDF.ToString(), byteToString, ID, dict);
                    break;
                case "Ink":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Ink.ToString(), null, ID, dict);
                    break;
                case "Audio":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Audio.ToString(), byteToString, ID, dict);
                    break;
                default:
                    break;
            }

            // store the properties of each node since they are always set to default once it is reloaded
        }

        /// <summary>
        /// Helper method used in ParseXml method that parses just the links
        /// </summary>
        /// <param name="vm">Current workspace view model</param>
        /// <param name="doc">Input XML document</param>
        /// <returns></returns>
        public async Task CreateLinks(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name;
                string ID = Convert.ToString(node.Attributes.GetNamedItem("id").Value);

                switch (AtomType)
                {
                    case "Link":
                        string id1 = node.Attributes.GetNamedItem("atomID1").Value;
                        string id2 = node.Attributes.GetNamedItem("atomID2").Value;
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                await NetworkConnector.Instance.RequestMakeLinq(id1, id2, ID);

                                /* create node annotation and attach it to the link 
                                   TO-DO: uncomment when annotation is incorporated in nunetwork branch */
                                //if (node.HasChildNodes)
                                //{
                                //    XmlNode attachedAnnotation = node.ChildNodes[0];
                                //    string clippedParentID = attachedAnnotation.Attributes.GetNamedItem("ClippedParent").Value;
                                //    await this.CreateNodeFromXml(vm, attachedAnnotation);
                                //    //nodeVm.ClippedParent = vm.Model.AtomDict[clippedParentID];
                                //}
                            });
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Helper method used in ParseXml method that checks whether a node has been successfully reloaded
        /// in the workspace and if so, removes that node from the list if it
        /// </summary>
        /// <param name="vm">Current workspace view model</param>
        /// <param name="node">XML node that contains info of a node in the saved workspace</param>
        /// <param name="copy">List of all the nodes in the saved workspace</param>
        /// <returns></returns>
        public async Task CheckNodeCreation(WorkspaceViewModel vm, XmlDocument doc, List<string> copy)
        {
            foreach (string id in _createdNodeList)
            {
                if (vm.Model.IDToSendableDict.ContainsKey(id))
                {
                    copy.Remove(id);
                }
            }
        }
    }
}
