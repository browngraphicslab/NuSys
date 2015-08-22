using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class ModelIntermediate
    {
        public WorkSpaceModel WorkSpaceModel{get;}
        public WorkSpaceModel.LockDictionary Locks { get { return WorkSpaceModel.Locks; } } 
        public ModelIntermediate(WorkSpaceModel wsm)
        {
            WorkSpaceModel = wsm;
        }
        public async Task HandleMessage(string s)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Dictionary<string, string> props = ParseOutProperties(s);
                if (props.ContainsKey("id"))
                {
                    string id = props["id"]; //since we called parse properties, it MUST have an id
                    if (WorkSpaceModel.IDToAtomDict.ContainsKey(id))
                    {
                        Atom n = WorkSpaceModel.IDToAtomDict[id];
                        n.UnPack(props);
                    }
                    //else if (_gloablInkDict.ContainsKey(id))
                    //{

                    //}
                    else
                    {
                        if (props.ContainsKey("type") && props["type"] == "ink")
                        {

                        }
                        else if (props.ContainsKey("type") && props["type"] == "group")
                        {
                            Node node1 = null;
                            Node node2 = null;
                            if (props.ContainsKey("id1") && props.ContainsKey("id2") && WorkSpaceModel.IDToAtomDict.ContainsKey(props["id1"]) && WorkSpaceModel.IDToAtomDict.ContainsKey(props["id2"]))
                            {
                                node1 = (Node)WorkSpaceModel.IDToAtomDict[props["id1"]];
                                node2 = (Node)WorkSpaceModel.IDToAtomDict[props["id2"]];
                            }
                            await WorkSpaceModel.CreateGroup(id, node1, node2);
                        }
                        else if (props.ContainsKey("type") && props["type"] == "node")
                        {
                            NodeType type = NodeType.Text;
                            double x = 0;
                            double y = 0;
                            object data = null;
                            if (props.ContainsKey("nodeType"))
                            {
                                string t = props["nodeType"];
                                type = (NodeType)Enum.Parse(typeof(NodeType), t);
                            }
                            if (props.ContainsKey("x"))
                            {
                                double.TryParse(props["x"], out x);
                            }
                            if (props.ContainsKey("y"))
                            {
                                double.TryParse(props["y"], out y);
                            }
                            if (props.ContainsKey("data") && props.ContainsKey("nodeType"))
                            {
                                string d = props["data"];
                                switch (type)
                                {
                                    case NodeType.Ink:
                                        try
                                        {
                                            data = ParseToPolyline(d);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Node Creation ERROR: Data could not be parsed into a polyline");
                                        }
                                        break;
                                    case NodeType.Image:
                                        try
                                        {
                                            data = ParseToByteArray(d);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Node Creation ERROR: Data could not be parsed into a Image");
                                        }
                                        break;
                                }
                            }
                            await WorkSpaceModel.CreateNewNode(props["id"], type, x, y, data);
                            await this.HandleMessage(s);
                        }
                        else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
                        {
                            string id1 = "null";
                            string id2 = "null";
                            if (props.ContainsKey("id1"))
                            {
                                id1 = props["id1"];
                            }
                            else
                            {
                                Debug.WriteLine("Could not create link");
                                return;
                            }
                            if (props.ContainsKey("id2"))
                            {
                                id2 = props["id2"];
                            }
                            else
                            {
                                Debug.WriteLine("Could not create link");
                                return;
                            }
                           
                            if (WorkSpaceModel.IDToAtomDict.ContainsKey(id1) && (WorkSpaceModel.IDToAtomDict.ContainsKey(id2)))
                            {
                                WorkSpaceModel.CreateLink(WorkSpaceModel.IDToAtomDict[id1], WorkSpaceModel.IDToAtomDict[id2], id);
                                //avm1 = WorkSpaceModel.IDToAtomeDict[id1];
                            }

                            //LinkViewModel vm = await _workspaceViewModel.CreateNewLink(id);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("ID was not found in property list of message: " + s);
                }
            });
        }
        public async Task RemoveNode(string id)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (WorkSpaceModel.IDToAtomDict.ContainsKey(id))
                {
                    WorkSpaceModel.RemoveNode(id);
                }
            });
        }
        public bool HasAtom(string id)
        {
            return WorkSpaceModel.IDToAtomDict.ContainsKey(id);
        }
        public async Task SetAtomLock(string id, string ip)
        {
            if (!HasAtom(id))
            {
                Debug.WriteLine("got lock update from unknown node");
                return;
            }
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await WorkSpaceModel.Locks.Set(id, ip);
                if (NetworkConnector.Instance.LocalIP == ip)
                {
                    WorkSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.Yes;
                }
                else if (ip == "")
                {
                    WorkSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.Maybe;
                }
                else
                {
                    WorkSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.No;
                }
            });
        }

        private byte[] ParseToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }
        private Polyline[] ParseToPolyline(string s)
        {
            List<Polyline> polys = new List<Polyline>();
            string[] parts = s.Split("><".ToCharArray());
            foreach (string part in parts)
            {
                Polyline poly = new Polyline();
                string[] subparts = part.Split(" ".ToCharArray());
                foreach (string subpart in subparts)
                {
                    if (subpart.Length > 0 && subpart != "polyline")
                    {
                        if (subpart.Substring(0, 6) == "points")
                        {
                            string innerPoints = subpart.Substring(8, subpart.Length - 9);
                            string[] points = innerPoints.Split(";".ToCharArray());
                            foreach (string p in points)
                            {
                                if (p.Length > 0)
                                {
                                    string[] coords = p.Split(",".ToCharArray());
                                    //Point point = new Point(double.Parse(coords[0]), double.Parse(coords[1]));
                                    poly.Points.Add(new Point(Int32.Parse(coords[0]), Int32.Parse(coords[1])));
                                }
                            }
                        }
                        else if (subpart.Substring(0, 9) == "thickness")
                        {
                            string sp = subpart.Substring(11, subpart.Length - 12);
                            poly.StrokeThickness = double.Parse(sp);
                        }
                        else if (subpart.Substring(0, 6) == "stroke")
                        {
                            string sp = subpart.Substring(8, subpart.Length - 10);
                            poly.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 1));
                            //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                        }
                    }
                }
                if (poly.Points.Count > 0)
                {
                    polys.Add(poly);
                }
            }
            return polys.ToArray();
        }
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(Constants.CommaReplacement.ToCharArray());
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    string[] subParts = part.Split("=".ToCharArray(), 2);
                    if (subParts.Length != 2)
                    {
                        Debug.WriteLine("Error, property formatted wrong in message: " + message);
                        continue;
                    }
                    if (!props.ContainsKey(subParts[0]))
                    {
                        props.Add(subParts[0], subParts[1]);
                    }
                    else
                    {
                        props[subParts[0]] = subParts[1];
                    }
                }
            }
            return props;
        }
        private HashSet<string> LocksNeeded(string id)
        {
            if (HasAtom(id))
            {
                HashSet<string> set = new HashSet<string>();
                set.Add(id);//TODO make this method return a set of all associated atoms needing to be locked as well.
                return set;
            }
            return new HashSet<string>();
        }
        public async Task<string> GetFullWorkspace()
        {
            if (WorkSpaceModel.IDToAtomDict.Count > 0)
            {
                string ret = "";
                foreach (KeyValuePair<string, Atom> kvp in WorkSpaceModel.IDToAtomDict)
                {
                    ret += '<';
                    Atom atom = kvp.Value;
                    Dictionary<string, string> parts = await atom.Pack();
                    foreach (KeyValuePair<string, string> tup in parts)
                    {
                        ret += tup.Key + '=' + tup.Value + Constants.CommaReplacement;
                    }
                    ret += "id=" + atom.ID + ">&&";
                }
                ret = ret.Substring(0, ret.Length - 2);
                return ret;
            }
            return "";
        }

        public bool HasLock(string id)
        {
            return WorkSpaceModel.Locks.ContainsID(id) && WorkSpaceModel.Locks.Value(id) == NetworkConnector.Instance.LocalIP;
        }
        
        public async Task CheckLocks(string id)
        {
            Debug.WriteLine("Checking locks");
            HashSet<string> locksNeeded = LocksNeeded(id);
            foreach (string lockID in WorkSpaceModel.Locks.LocalLocks)
            {
                if (!locksNeeded.Contains(lockID))
                {
                    await NetworkConnector.Instance.ReturnLock(lockID);
                }
            }
        }

        public void RemoveIPFromLocks(string ip)
        {
            if (WorkSpaceModel.Locks.ContainsHolder(ip))
            {
                foreach (KeyValuePair<string, string> kvp in WorkSpaceModel.Locks)
                {
                    if (kvp.Value == ip)
                    {
                        SetAtomLock(kvp.Key, "");
                        if (!WorkSpaceModel.Locks.ContainsHolder(ip))
                        {
                            return;
                        }
                    }
                }
            }
        }
        public async Task ForceSetLocks(string message)
        {
            WorkSpaceModel.Locks.Clear();
            foreach (KeyValuePair<string, string> kvp in StringToDict(message))
            {
                await SetAtomLock(kvp.Key, kvp.Value);
            }
        }

        public string GetAllLocksToSend()
        {
            return DictToString(WorkSpaceModel.Locks);
        }
        public async Task<Dictionary<string, string>> GetNodeState(string id)
        {
            if (HasAtom(id))
            {
                return await WorkSpaceModel.IDToAtomDict[id].Pack();
            }
            else
            {
                return null;
            }
        }

        private string DictToString(IEnumerable<KeyValuePair<string, string>> dict)
        {
            string s = "";
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                s += kvp.Key + ":" + kvp.Value + "&";
            }
            s = s.Substring(0, Math.Max(s.Length - 1,0));
            return s;
        }

        private Dictionary<string, string> StringToDict(string s)
        {
            Dictionary<string,string> dict = new Dictionary<string, string>();
            string[] strings = s.Split("&".ToCharArray());
            foreach (string kvpString in strings)
            {
                string[] kvpparts = kvpString.Split(":".ToCharArray());
                dict.Add(kvpparts[0], kvpparts[1]);
            }
            return dict;
        } 
    }
}
