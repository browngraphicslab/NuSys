using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SQLite.Net.Async;
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class XmlFileHelper
    {

        private List<Dictionary<string, string>> _atomUpdateDicts = new List<Dictionary<string, string>>();

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
            string currType = node.Attributes.GetNamedItem("nodeType").Value;
            string X = node.Attributes.GetNamedItem("x").Value;
            string Y = node.Attributes.GetNamedItem("y").Value;
            string width = node.Attributes.GetNamedItem("width").Value;
            string height = node.Attributes.GetNamedItem("height").Value;

            NodeViewModel nodeVM = null; //Just a filler - gets reassigned in all cases


            Dictionary<string, string> dict = new Dictionary<string, string>();

            var query = vm.myDB.DBConnection.Table<Content>().Where(v => v.assocAtomID == ID);
            var res = await query.FirstOrDefaultAsync();

            byte[] byteData = res.Image;

            switch (currType)
            {
                case "Text":
                    //string text = node.Attributes.GetNamedItem("text").Value; TO DO: Uncomment this when we get rid of the encoding in the textnode
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null , ID);
                    break;
                case "Image":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Image.ToString(), null, ID);
                    break;
                case "Pdf":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null, ID);
                    break;
                case "ink":
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null, ID);
                    break;
                default:
                    await NetworkConnector.Instance.RequestMakeNode(X, Y, NodeType.Text.ToString(), null, ID);
                    break;
            }

            dict.Add("image", Encoding.UTF8.GetString(byteData));
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

            //dict.Add("image", System.Text.Encoding.UTF8.GetString(res.Data));

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
                        string atomID1 = Convert.ToString(node.Attributes.GetNamedItem("atomID1").Value);
                        string atomID2 = Convert.ToString(node.Attributes.GetNamedItem("atomID2").Value);
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, async() =>
                            {
                                AtomViewModel atom1Vm = vm.Model.AtomDict[atomID1];
                                AtomViewModel atom2Vm = vm.Model.AtomDict[atomID2];
                                //LinkViewModel newLinkVm = vm.CreateNewLink(ID, atom1Vm, atom2Vm);
                                //newLinkVm.ID = ID;

                                // create node annotation and attach it to the link
                                if (node.HasChildNodes)
                                {
                                    XmlNode attachedAnnotation = node.ChildNodes[0];
                                    string clippedParentID = attachedAnnotation.Attributes.GetNamedItem("ClippedParent").Value;
                                    await this.CreateNodeFromXml(vm, attachedAnnotation);
                                    //nodeVm.ClippedParent = vm.Model.AtomDict[clippedParentID];
                                }
                            });
                        break;
                }
            }

            foreach (Dictionary<string, string> dict in _atomUpdateDicts)
            {
                NetworkConnector.Instance.QuickUpdateAtom(dict);
            }
        }
    }
}
