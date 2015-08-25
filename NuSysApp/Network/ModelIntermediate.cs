using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
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
                    string id = props["id"];
                    if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                    {
                        Sendable n = WorkSpaceModel.IDToSendableDict[id];
                        await n.UnPack(props);
                    }
                    //else if (_gloablInkDict.ContainsKey(id))
                    //{

                    //}
                    else
                    {
                        if (props.ContainsKey("type") && props["type"] == "ink")
                        {
                            if (props.ContainsKey("inkType") && props["inkType"] == "global")
                            {
                                if (props.ContainsKey("globalInkType") && props["globalInkType"] == "partial")
                                {
                                    Line l = ParseToLineSegment(props);
                                    if (l == null) return;

                                    if (WorkSpaceModel.PartialLines.ContainsKey(id))
                                    {
                                        WorkSpaceModel.PartialLines[id].Add(l);
                                    }
                                    else
                                    {
                                        ObservableCollection<Line> ol = new ObservableCollection<Line>();
                                        WorkSpaceModel.PartialLines.Add(id, new ObservableCollection<Line>());
                                        WorkSpaceModel.PartialLines[id].Add(l);
                                    }
                                }
                                else if (props.ContainsKey("globalInkType") && props["globalInkType"] == "full")
                                {
                                    if (props.ContainsKey("previousID") &&
                                        WorkSpaceModel.PartialLines.ContainsKey(props["previousID"]))
                                    {
                                        ObservableCollection<Line> oc = WorkSpaceModel.PartialLines[props["previousID"]];
                                        foreach(Line l in oc)
                                        {
                                            ((InqCanvas) l.Parent).Children.Remove(l);
                                        }
                                        WorkSpaceModel.PartialLines.Remove(props["previousID"]);
                                    }
                                    if (props.ContainsKey("data"))
                                    {
                                        List<InqLine> lines = ParseToPolyline(props["data"]);

                                    }
                                }

                            }
                        }
                        else if (props.ContainsKey("type") && props["type"] == "group")
                        {
                            Node node1 = null;
                            Node node2 = null;
                            double x = 0;
                            double y = 0;
                            if (props.ContainsKey("id1") && props.ContainsKey("id2") && WorkSpaceModel.IDToSendableDict.ContainsKey(props["id1"]) && WorkSpaceModel.IDToSendableDict.ContainsKey(props["id2"]))
                            {
                                node1 = (Node)WorkSpaceModel.IDToSendableDict[props["id1"]];
                                node2 = (Node)WorkSpaceModel.IDToSendableDict[props["id2"]];
                            }
                            if (props.ContainsKey("x"))
                            {
                                double.TryParse(props["x"], out x);
                            }
                            if (props.ContainsKey("y"))
                            {
                                double.TryParse(props["y"], out y);
                            }
                            await WorkSpaceModel.CreateGroup(id, node1, node2, x, y);
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
                                    case NodeType.Text:
                                    case NodeType.Richtext:
                                        if (!props.ContainsKey("text"))
                                        {
                                            props.Add("text", d);
                                        }
                                        else
                                        {
                                            props["text"] = d;
                                        }
                                        break;
                                    case NodeType.Image:
                                        try
                                        {
                                            data = ParseToByteArray(d);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Node Creation ERROR: Data could not be parsed into a byte array");
                                        }
                                        break;
                                    case NodeType.PDF:
                                        try
                                        {
                                            data = ParseToByteArray(d);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Node Creation ERROR: Data could not be parsed into a byte array");
                                        }
                                        break;
                                }
                            }
                            await WorkSpaceModel.CreateNewNode(props["id"], type, x, y, data);
                            if (props.ContainsKey("data"))
                            {
                                props.Remove("data");
                            }
                            await WorkSpaceModel.IDToSendableDict[props["id"]].UnPack(props);
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
                           
                            if (WorkSpaceModel.IDToSendableDict.ContainsKey(id1) && (WorkSpaceModel.IDToSendableDict.ContainsKey(id2)))
                            {
                                WorkSpaceModel.CreateLink((Atom)WorkSpaceModel.IDToSendableDict[id1], (Atom)WorkSpaceModel.IDToSendableDict[id2], id);
                            }
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
                if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                {
                    WorkSpaceModel.RemoveNode(id);
                }
            });
        }
        public bool HasAtom(string id)
        {
            return WorkSpaceModel.IDToSendableDict.ContainsKey(id);
        }
        public async Task SetAtomLock(string id, string ip)
        {
            if (!HasAtom(id))
            {
                Debug.WriteLine("got lock update from unknown node");
                return;
            }
            await WorkSpaceModel.Locks.Set(id, ip);
        }

        private byte[] ParseToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }
        private List<InqLine> ParseToPolyline(string s)
        {

            List<InqLine> polys = new List<InqLine>();
            string[] parts = s.Split("><".ToCharArray());
            foreach (string part in parts)
            {
                InqLine line = new InqLine();
                string[] subparts = part.Split(" ".ToCharArray());
                foreach (string subpart in subparts)
                {
                    if (subpart.Length > 0 && subpart != "polyline")
                    {
                        if (subpart.Substring(0, 6) == "points")
                        {
                            string innerPoints = subpart.Substring(8, subpart.Length - 9);
                            string[] points = innerPoints.Split(new string[] { ";" }, StringSplitOptions.None);
                            foreach (string p in points)
                            {
                                if (p.Length > 0)
                                {
                                    string[] coords = p.Split(new string[] { "," }, StringSplitOptions.None);
                                    //Point point = new Point(double.Parse(coords[0]), double.Parse(coords[1]));
                                    Point parsedPoint = new Point(Int32.Parse(coords[0]), Int32.Parse(coords[1]));
                                    line.AddPoint(parsedPoint);
                                }
                            }
                        }
                        else if (subpart.Substring(0, 9) == "thickness")
                        {
                            string sp = subpart.Substring(11, subpart.Length - 13);
                            line.StrokeThickness = double.Parse(sp);
                        }
                        else if (subpart.Substring(0, 6) == "stroke")
                        {
                            string sp = subpart.Substring(8, subpart.Length - 10);
                            line.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 1));
                            //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                        }
                    }
                }
                if (line.Points.Count > 0)
                {
                    polys.Add(line);
                }
            }
            return polys;
        }
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(new string[] { Constants.CommaReplacement }, StringSplitOptions.None);
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    string[] subParts = part.Split(new string[] { "=" },2, StringSplitOptions.None);
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
            if (WorkSpaceModel.IDToSendableDict.Count > 0)
            {
                string ret = "";
                foreach (KeyValuePair<string, Sendable> kvp in WorkSpaceModel.IDToSendableDict)
                {
                    ret += '<';
                    Sendable atom = kvp.Value;
                    Dictionary<string, string> parts = await atom.Pack();
                    foreach (KeyValuePair<string, string> tup in parts)
                    {
                        ret += tup.Key + '=' + tup.Value + Constants.CommaReplacement;
                    }
                    ret += "id=" + atom.ID + ">"+Constants.AndReplacement;
                }
                ret = ret.Substring(0, ret.Length - 2);
                return ret;
            }
            return "";
        }

        public async Task ClearLocks()
        {
            List<string> locks = new List<string>();
            locks.AddRange(WorkSpaceModel.Locks.LocalLocks);
            while (locks.Count > 0)
            {
                string l = locks.First();
                locks.Remove(l);
                await NetworkConnector.Instance.ReturnLock(l);
            }
        }

        public bool HasLock(string id)
        {
            return WorkSpaceModel.Locks.ContainsID(id) && WorkSpaceModel.Locks.Value(id) == NetworkConnector.Instance.LocalIP;
        }
        
        public async Task CheckLocks(string id)
        {
            Debug.WriteLine("Checking locks");
            HashSet<string> locksNeeded = LocksNeeded(id);
            List<string> locksToReturn = new List<string>();
            foreach (string lockID in WorkSpaceModel.Locks.LocalLocks)
            {
                if (!locksNeeded.Contains(lockID))
                {
                    locksToReturn.Add(lockID);
                }
            }
            while (locksToReturn.Count > 0)
            {
                string l = locksToReturn.First();
                locksToReturn.Remove(l);
                await NetworkConnector.Instance.ReturnLock(l);
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
                return await WorkSpaceModel.IDToSendableDict[id].Pack();
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
            string[] strings = s.Split(new string[] { "&" }, StringSplitOptions.None);
            foreach (string kvpString in strings)
            {
                string[] kvpparts = kvpString.Split(new string[] { ":" }, StringSplitOptions.None);
                dict.Add(kvpparts[0], kvpparts[1]);
            }
            return dict;
        }

        private Line ParseToLineSegment(Dictionary<string,string> props)
        {
            Line l = new Line();
            if (props.ContainsKey("x1") && props.ContainsKey("y1") && props.ContainsKey("x2") && props.ContainsKey("y2"))
            {
                l.X1 = Double.Parse(props["x1"]);
                l.X2 = Double.Parse(props["x2"]);
                l.Y1 = Double.Parse(props["y1"]);
                l.Y2 = Double.Parse(props["y2"]);
            }
            else
            {
                return null;
            }
            return l;
        }
    }
}
