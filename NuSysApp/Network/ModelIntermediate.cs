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
        public HashSet<string> _deletedIDs; 
        private Dictionary<string, Action<string>> _creationCallbacks;
        private HashSet<string> _sendablesLocked;
        public ModelIntermediate(WorkSpaceModel wsm)
        {
            WorkSpaceModel = wsm;
            _creationCallbacks = new Dictionary<string, Action<string>>();
             _sendablesLocked = new HashSet<string>();
            _deletedIDs = new HashSet<string>();

        }
        public async Task HandleMessage(Dictionary<string,string> props)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (props.ContainsKey("id"))
                {
                    string id = props["id"];//get id from dictionary
                    _sendablesLocked.Add(id);
                    if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                    {
                        Sendable n = WorkSpaceModel.IDToSendableDict[id];//if the id exists, get the sendable
                        await n.UnPack(props);//update the sendable with the dictionary info
                    }
                    else//if the sendable doesn't yet exist
                    {

                        if (!_deletedIDs.Contains(id))
                        {
                            await HandleCreateNewSendable(id, props); //create a new sendable
                            if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                            {
                                await HandleMessage(props);
                            }
                            if (_creationCallbacks.ContainsKey(id))
                                //check if a callback is waiting for that sendable to be created
                            {
                                _creationCallbacks[id].DynamicInvoke(id);
                                _creationCallbacks.Remove(id);
                            }
                        }
                    }
                    _sendablesLocked.Remove(id);
                }
                else
                {
                    Debug.WriteLine("ID was not found in property list of message: ");
                }
            });
        }

        public bool IsSendableLocked(string id)
        {
            return _sendablesLocked.Contains(id);
        }
        public async Task HandleCreateNewSendable(string id, Dictionary<string,string> props)
        {
            if (props.ContainsKey("type") && props["type"] == "ink")
            {
                    await HandleCreateNewInk(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "group")
            {
                await HandleCreateNewGroup(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "emptygroup")
            {
                await HandleCreateNewEmptyGroup(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "node")
            {
                await HandleCreateNewNode(id, props);
            }
            else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
            {
                await HandleCreateNewLink(id, props);
            }
            else if (props.ContainsKey("type") && (props["type"] == "pin"))
            {
                await HandleCreateNewPin(id, props);
            }
        }

        public async Task HandleCreateNewPin(string id, Dictionary<string, string> props)
        {
            double x = 0;
            double y = 0;
            if (props.ContainsKey("x") && props.ContainsKey("y"))
            {
                try
                {
                    x = double.Parse(props["x"]);
                    y = double.Parse(props["y"]);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Pin creation failed because coordinates could not be parsed to doubles");
                }
                await WorkSpaceModel.CreateNewPin(id, x, y);
            }
        }
        public async Task HandleCreateNewLink(string id, Dictionary<string, string> props)
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
        public async Task HandleCreateNewNode(string id, Dictionary<string, string> props)
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
                            data = ParseToPolyline(d, id);
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
                            data = d;
                        }
                        else
                        {
                            props["text"] = d;
                            data = d;
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
        }

        public async Task HandleCreateNewEmptyGroup(string id, Dictionary<string, string> props)
        {
            double x = 0;
            double y = 0;
        
            if (props.ContainsKey("x"))
            {
                double.TryParse(props["x"], out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props["y"], out y);
            }
            await WorkSpaceModel.CreateEmptyGroup(id, x, y);
        }

        public async Task HandleCreateNewGroup(string id, Dictionary<string, string> props)
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
        public async Task HandleCreateNewInk(string id, Dictionary<string, string> props)
        {
            if (props.ContainsKey("canvasNodeID") && (HasSendableID(props["canvasNodeID"]) || props["canvasNodeID"]== "WORKSPACE_ID"))
            {
                InqCanvasModel canvas;
                if (props["canvasNodeID"] != "WORKSPACE_ID")
                {
                    canvas = ((Node) WorkSpaceModel.IDToSendableDict[props["canvasNodeID"]]).InqCanvas;
                }
                else
                {
                    canvas = WorkSpaceModel.InqModel;
                }
                if (props.ContainsKey("inkType") && props["inkType"] == "partial")
                {
                    InqLine l = ParseToLineSegment(props);
                    if (l == null)
                    {
                        Debug.WriteLine("failed to parse message to inqline");
                        return;
                    }
                    canvas.AddTemporaryInqline(l, id);
                }
                else if (props.ContainsKey("inkType") && props["inkType"] == "full")
                {
                    if (props.ContainsKey("data"))
                    {
                        InqLine line = ParseToPolyline(props["data"], id);
                        WorkSpaceModel.IDToSendableDict.Add(id, line);
                        if (props.ContainsKey("previousID") &&
                            WorkSpaceModel.InqModel.PartialLines.ContainsKey(props["previousID"]))
                        {
                            canvas.OnFinalizedLine += delegate
                            {
                               canvas.RemovePartialLines(props["previousID"]);
                            };
                        }
                        canvas.FinalizeLine(line);
                    }
                }
            }
            else
            {
                Debug.WriteLine("Ink creation failed because no canvas ID was given or the ID wasn't valid");
            }
        }
        public async Task RemoveSendable(string id)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                {
                    WorkSpaceModel.RemoveSendable(id);
                }
                _deletedIDs.Add(id);
            });
        }
        public bool HasSendableID(string id)
        {
            return WorkSpaceModel.IDToSendableDict.ContainsKey(id);
        }
        public async Task SetAtomLock(string id, string ip)
        {
            if (!HasSendableID(id))
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
        private InqLine ParseToPolyline(string s, string id)
        {
            return InqLine.ParseToPolyline(s, id);
        }
        private HashSet<string> LocksNeeded(List<string> ids)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (string id in ids)
            {
                if (HasSendableID(id))
                {
                    set.Add(id);
                        //TODO make this method return a set of all associated atoms needing to be locked as well.
                    return set;
                }
            }
            return new HashSet<string>();
        }
        public async Task<string> GetFullWorkspace()
        {
            LinkedList<Sendable> list = new LinkedList<Sendable>();
            Dictionary<string,Sendable> set = new Dictionary<string, Sendable>();

            foreach (KeyValuePair<string, Sendable> kvp in WorkSpaceModel.IDToSendableDict)
            {
                set.Add(kvp.Key,kvp.Value);
            }

            while(set.Count > 0)
            {
                Sendable s = set[set.Keys.First()];
                if (s.GetType() != typeof (Link) || (!set.ContainsKey(((Link) s).InAtomID) &&
                    !set.ContainsKey(((Link) s).OutAtomID)))
                {
                    list.AddLast(s);
                    set.Remove(s.ID);
                }
                else
                {
                    
                }
            }
            if (WorkSpaceModel.IDToSendableDict.Count > 0)
            {
                string ret = "";
                while(list.Count > 0)
                {
                    ret += '<';
                    Sendable atom = list.First.Value;
                    list.RemoveFirst();
                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        Dictionary<string, string> parts = await atom.Pack();
                        foreach (KeyValuePair<string, string> tup in parts)
                        {
                            ret += tup.Key + '=' + tup.Value + Constants.CommaReplacement;
                        }
                        ret += "id=" + atom.ID + ">" + Constants.AndReplacement;
                    });
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

        public void AddCreationCallback(string id, Action<string> d)
        {
            _creationCallbacks.Add(id, d);
        }
        public bool HasLock(string id)
        {
            var sendable = WorkSpaceModel.IDToSendableDict[id];
            bool isLine = sendable is InqLine;
            return isLine || (WorkSpaceModel.Locks.ContainsID(id) && WorkSpaceModel.Locks.Value(id) == NetworkConnector.Instance.LocalIP);
        }
        
        public async Task CheckLocks(List<string> ids)
        {
            Debug.WriteLine("Checking locks");
            HashSet<string> locksNeeded = LocksNeeded(ids);
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
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                WorkSpaceModel.Locks.Clear();
                foreach (KeyValuePair<string, string> kvp in StringToDict(message))
                {
                    await SetAtomLock(kvp.Key, kvp.Value);
                }
            });
        }

        public string GetAllLocksToSend()
        {
            return DictToString(WorkSpaceModel.Locks);
        }
        public async Task<Dictionary<string, string>> GetNodeState(string id)
        {
            if (HasSendableID(id))
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
            string[] strings = s.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvpString in strings)
            {
                string[] kvpparts = kvpString.Split(new string[] { ":" },2, StringSplitOptions.RemoveEmptyEntries);
                if (kvpparts.Length == 2)
                {
                    dict.Add(kvpparts[0], kvpparts[1]);
                }
            }
            return dict;
        }

        private InqLine ParseToLineSegment(Dictionary<string,string> props)
        {
            InqLine l = new InqLine("");
            if (props.ContainsKey("x1") && props.ContainsKey("y1") && props.ContainsKey("x2") && props.ContainsKey("y2"))
            {
                Point one = new Point(Double.Parse(props["x1"]), Double.Parse(props["y1"]));
                Point two = new Point(Double.Parse(props["x2"]), Double.Parse(props["y2"]));
                l.AddPoint(one);
                l.AddPoint(two);
                l.StrokeThickness = 2;
            }
            else
            {
                return null;
            }
            return l;
        }
    }
}
