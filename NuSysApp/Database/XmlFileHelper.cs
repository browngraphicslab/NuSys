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

namespace NuSysApp
{
    public class XmlFileHelper
    {

        private List<Dictionary<string, string>> _atomUpdateDicts = new List<Dictionary<string, string>>();
        List<string> _createdAtomList = new List<string>();
        bool _allAtomCreated = false;

        public XmlFileHelper()
        {
            this.ID = "1";
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

        public async Task CreateNodeFromXml(WorkspaceViewModel vm, XmlNode node)
        {
            string ID = node.Attributes.GetNamedItem("id").Value;
            _createdAtomList.Add(ID);
            string currType = node.Attributes.GetNamedItem("nodeType").Value;
            string X = node.Attributes.GetNamedItem("x").Value;
            string Y = node.Attributes.GetNamedItem("y").Value;
            string width = node.Attributes.GetNamedItem("width").Value;
            string height = node.Attributes.GetNamedItem("height").Value;

            NodeViewModel nodeVM = null; //Just a filler - gets reassigned in all cases

            Dictionary<string, string> dict = new Dictionary<string, string>();

            var query = vm.myDB.DBConnection.Table<Content>().Where(v => v.assocAtomID == ID);
            var res = await query.FirstOrDefaultAsync();

            byte[] byteData = null;
            string byteToString = null;

            if (res != null)
            {
                byteData = res.Data;

                if (currType == "Text")
                {
                    byteToString = System.Text.Encoding.UTF8.GetString(byteData);
                }
                else
                {
                    byteToString = Convert.ToBase64String(byteData);
                }

            }
            
            switch (currType)
            {
                case "Text":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), byteToString, ID);
                    break;
                case "Image":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Image.ToString(), byteToString, ID);
                    break;
                case "PDF":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.PDF.ToString(), byteToString, ID);
                    break;
                case "Ink":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null, ID);
                    break;
                default:
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null, ID);
                    break;
            }

            dict.Add("width", width);
            dict.Add("height", height);
            dict.Add("id", ID);
            _atomUpdateDicts.Add(dict);
        }

        public async Task ParseXml(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;
            SQLiteAsyncConnection dbConnection = vm.myDB.DBConnection;

            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name;
                string ID = Convert.ToString(node.Attributes.GetNamedItem("id").Value);

                switch (AtomType)
                {
                    case "Group":
                        double x = Convert.ToDouble(node.Attributes.GetNamedItem("x").Value);
                        double y = Convert.ToDouble(node.Attributes.GetNamedItem("y").Value);
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                //GroupViewModel groupVm = new GroupViewModel(vm, ID);
                                //vm.Model.AtomDict.Add(ID, groupVm);
                                //foreach (XmlNode child in node.ChildNodes) //Groups have child nodes
                                //{
                                //    await this.CreateNodeFromXml(vm, child);
                                //}
                                //vm.NodeViewModelList.Add(groupVm);
                                //vm.AtomViewList.Add(groupVm.View);
                                //vm.PositionNode(groupVm, x, y);
                            });
                        break;
                    case "Node":
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                await this.CreateNodeFromXml(vm, node);
                            });
                        break;
                    case "Link":
                        break;
                }
            }

            int counter = 0;
            List<string> createdAtomListCopy = _createdAtomList;

            while (createdAtomListCopy.Count != 0)
            {
                CheckNodeCreation(vm, createdAtomListCopy);
            }
            await this.CreateLinks(vm, doc);

            foreach (Dictionary<string, string> dict in _atomUpdateDicts)
            {
                NetworkConnector.Instance.QuickUpdateAtom(dict);
            }
        }

        public async Task CheckNodeCreation(WorkspaceViewModel vm, List<string> copy)
        {
            foreach (string id in _createdAtomList)
            {
                if (vm.Model.IDToSendableDict.ContainsKey(id))
                {
                    copy.Remove(id);
                }
            }
        }

        public async Task CreateLinks(WorkspaceViewModel vm, XmlDocument doc)
        {
            XmlElement parent = doc.DocumentElement;
            XmlNodeList NodeList = parent.ChildNodes;

            foreach (XmlNode node in NodeList)
            {
                string AtomType = node.Name;
                string ID = Convert.ToString(node.Attributes.GetNamedItem("id").Value);

                switch (AtomType)
                {
                    case "Group":
                        break;
                    case "Node":
                        break;
                    case "Link":
                        string id1 = node.Attributes.GetNamedItem("atomID1").Value;
                        string id2 = node.Attributes.GetNamedItem("atomID2").Value;
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async () =>
                            {
                                await NetworkConnector.Instance.RequestMakeLinq(id1, id2);

                                // create node annotation and attach it to the link
                                //if (node.HasChildNodes)
                                //{
                                //    XmlNode attachedAnnotation = node.ChildNodes[0];
                                //    string clippedParentID = attachedAnnotation.Attributes.GetNamedItem("ClippedParent").Value;
                                //    await this.CreateNodeFromXml(vm, attachedAnnotation);
                                //    //nodeVm.ClippedParent = vm.Model.AtomDict[clippedParentID];
                                //}
                            });
                        break;
                }
            }
        }
    }
}
