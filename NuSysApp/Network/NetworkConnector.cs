﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public class NetworkConnector
    {
        #region Private Members

        private static volatile NetworkConnector _instance;
        private static readonly object _syncRoot = new Object();
        #endregion Private Members

        #region Public Members

        private ClientHandler _clientHandler;
        public WorkSpaceModel.LockDictionary Locks { get { return WorkSpaceModel.Locks; } }
        private ConcurrentDictionary<string, bool> _deletedIDs;
        private ConcurrentDictionary<string, Action<string>> _creationCallbacks;
        private ConcurrentDictionary<string, bool> _sendablesBeingUpdated;
        public enum PacketType
        {
            UDP,
            TCP,
            Both
        }

        public static NetworkConnector Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new NetworkConnector();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        private NetworkConnector()//pls keep this private or shit won't work anymore
        {
            _creationCallbacks = new ConcurrentDictionary<string, Action<string>>();
            _sendablesBeingUpdated = new ConcurrentDictionary<string, bool>();
            _deletedIDs = new ConcurrentDictionary<string, bool>();
            _clientHandler = new ClientHandler(this);
            _clientHandler.OnNewMessage += MessageRecieved;
            _clientHandler.OnAllLocksSet += async delegate (string s) { await ForceSetLocks(s); };
            _clientHandler.OnLockUpdateRecieved += async delegate (string id, string holder) { await SetAtomLock(id, holder); };
            _clientHandler.OnRemoveIP += async delegate(string ip) { RemoveIPFromLocks(ip); };
            _clientHandler.OnDeleteRequestRecieved += async delegate(string id) { RemoveSendable(id); };
        }

        private async void MessageRecieved(string ip, string message, PacketType packetType)
        {
            var matches = Regex.Match(message, "(?:({[^}]+}) *)*");
            string[] miniStrings = matches.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();

            //var miniStrings = message.Split(new string[] { Constants.AndReplacement }, StringSplitOptions.RemoveEmptyEntries); //break up message into subparts
            foreach (var subMessage in miniStrings)
            {
                if (subMessage.Length > 0)
                {
                    Dictionary<string, string> props = ParseOutProperties(subMessage);
                    await HandleMessage(props); //handle each submessage
                    if ((HasSendableID(props["id"]) || (props.ContainsKey("nodeType") && props["nodeType"] == NodeType.PDF.ToString())) && packetType == PacketType.TCP && _clientHandler.IsHost())
                    {
                        await _clientHandler.SendMassTCPMessage(message);
                    }
                }
            }
        }

        /*
        * removes this local ip from the php script that keeps track of all the members on the server
        */

        public async Task Disconnect(string ip = null) //called by the closing of the application
        {
            _clientHandler.Disconnect(ip);
        }

        /*
        * handles and proccesses a regular sub-message
        */
        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
        {
            if (_clientHandler.IsHost())//if host, add a new packet and store it in every joining member's stack of updates
            {
                /*
                foreach (var kvp in _joiningMembers)
                // keeps track of messages sent during initial loading into workspace
                {
                    kvp.Value.Item2.Add(new Packet(message, packetType));
                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
                    {
                        var tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
                        _joiningMembers[kvp.Key] = tup;
                    }
                }*/
            }

            Dictionary<string, string> props = ParseOutProperties(message);
            if (props.ContainsKey("id"))
            {
                await HandleMessage(props);
                if ((HasSendableID(props["id"]) || (props.ContainsKey("nodeType") && props["nodeType"]==NodeType.PDF.ToString()))&& packetType == PacketType.TCP && _clientHandler.IsHost())
                {
                    await _clientHandler.SendMassTCPMessage(message);
                }
            }
            else
            {
                throw new InvalidIDException("there was no id, that's why this error is occurring...");
            }
        }

        /*
        * parses message to dictionary of properties
        */
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>(message);
        }

        /*
        * makes a message from a dictionary of properties.  dict must have an ID
        */
        public string MakeSubMessageFromDict(Dictionary<string, string> dict)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
        }

        #region publicRequests
        /*
        * PUBLIC request for deleting a nod 
        */
        public async Task RequestDeleteSendable(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (HasLock(id))
                {
                    await _clientHandler.SendDeleteMessage(id); //tells host to delete the node
                }
            });
        }

        /*
        * PUBLIC general method to update everyone from an Atom update.  sends mass udp packet
        */
        public async Task QuickUpdateAtom(Dictionary<string, string> properties, PacketType packetType = PacketType.UDP)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (properties.ContainsKey("id"))
                {
                    if (HasSendableID(properties["id"]))
                    {
                        string message = MakeSubMessageFromDict(properties);
                        await _clientHandler.SendMassMessage(message, packetType);
                        if (packetType == PacketType.TCP)
                        {
                            await HandleRegularMessage(_clientHandler.LocalIP(), message, packetType);
                        }
                    }
                    else
                    {
                        throw new InvalidIDException(properties["id"]);
                        return;
                    }
                }
                else
                {
                    throw new NoIDException();
                    return;
                }
            });
        }

        /*
        * PUBLIC general method to create Node
        */
        public async Task RequestMakeNode(string x, string y, string nodeType, string data = null, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (x != "" && y != "" && nodeType != "")
                {
                    Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                    string id = oldID == null ? _clientHandler.GetID() : oldID;

                    if (props.ContainsKey("x"))
                    {
                        props.Remove("x");
                    }
                    if (props.ContainsKey("y"))
                    {
                        props.Remove("y");
                    }
                    if (props.ContainsKey("id"))
                    {
                        props.Remove("id");
                    }
                    if (props.ContainsKey("nodeType"))
                    {
                        props.Remove("nodeType");
                    }
                    if (props.ContainsKey("type"))
                    {
                        props.Remove("type");
                    }
                    props.Add("x", x);
                    props.Add("y", y);
                    props.Add("nodeType", nodeType);
                    props.Add("id", id);
                    props.Add("type", "node");
                    if (data != null && data != "null" && data != "")
                    {
                        props.Add("data", data);
                    }

                    if (callback != null)
                    {
                        AddCreationCallback(id, callback);
                    }

                    string message = MakeSubMessageFromDict(props);

                    await _clientHandler.SendMessageToHost(message);
                }
                else
                {
                    throw new InvalidCreationArgumentsException();
                    return;
                }
            });
        }

        /*
        * PUBLIC general method to create Pin
        */
        public async Task RequestMakePin(string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
 
                var props = properties == null ? new Dictionary<string, string>() : properties;
            string id = oldID == null ? _clientHandler.GetID() : oldID;
            if (props.ContainsKey("x"))
            {
                props.Remove("x");
            }
            if (props.ContainsKey("y"))
            {
                props.Remove("y");
            }
            if (props.ContainsKey("id"))
            {
                props.Remove("id");
            }
            if (props.ContainsKey("type"))
            {
                props.Remove("type");
            }
            props.Add("x", x);
            props.Add("y", y);
            props.Add("id", id);
            props.Add("type", "pin");
            string message = MakeSubMessageFromDict(props);
            if (callback != null)
            {
                AddCreationCallback(oldID, callback);
            }
            await _clientHandler.SendMessageToHost(message);
        }

        /*
        * PUBLIC general method to create Group
        * TODO factor this into one method with RequestMakeEmptyGroup that takes in a list os ID's to place in that group
        */
        public async Task RequestMakeGroup(string id1, string id2, string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            if (id1 != "" && id2 != "")
            {
                if (HasSendableID(id1))
                {
                    if (HasSendableID(id2))
                    {
                        Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                        string id = oldID == null ? _clientHandler.GetID() : oldID;
                        props["id1"] = id1;
                        props["id2"] = id2;
                        props["x"] = x;
                        props["y"] = y;
                        props["id"] = id;
                        props["type"] = "group";
                        if (callback != null)
                        {
                            AddCreationCallback(id, callback);
                        }
                        string message = MakeSubMessageFromDict(props);
                        await _clientHandler.SendMessageToHost(message);
                    }
                    else
                    {
                        throw new InvalidIDException(id2);
                    }
                }
                else
                {
                    throw new InvalidIDException(id1);
                }
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }

        /*
       * PUBLIC general method to create Group
       * TODO merge this into one request make group method
       */
        public async Task RequestMakeEmptyGroup( string x, string y, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {

            var props = properties == null ? new Dictionary<string, string>() : properties;
            string id = oldID == null ? _clientHandler.GetID() : oldID;
            props["x"] = x;
            props["y"] = y;
            props["id"] = id;
            props["type"] = "emptygroup";
            if (callback != null)
            {
                AddCreationCallback(id, callback);
            }
            string message = MakeSubMessageFromDict(props);
            await _clientHandler.SendMessageToHost(message);
        }

        /*
        * PUBLIC general method to create Linq
        */
        public async Task RequestMakeLinq(string id1, string id2, string oldID = null, Dictionary<string, string> properties = null, Action<string> callback = null)
        {
            if (id1 == id2)
            {
                throw new InvalidCreationArgumentsException("the two ids for the link were identical");
            }
            if (id1 != "" && id2 != "" && HasSendableID(id1) && HasSendableID(id2))
            {
                Dictionary<string, string> props = properties == null ? new Dictionary<string, string>() : properties;
                string id = oldID == null ? _clientHandler.GetID() : oldID;
                props["id1"] = id1;
                props["id2"] = id2;
                props["type"] = "link";
                props["id"] = id;

                if (callback != null)
                {
                    AddCreationCallback(oldID, callback);
                }

                string message = MakeSubMessageFromDict(props);

                await _clientHandler.SendMessageToHost(message);
            }
            else
            {
                throw new InvalidCreationArgumentsException();
                return;
            }
        }

        public string LocalIP
        {
            get { return _clientHandler.LocalIP(); }
        }
        public async Task RequestSendPartialLine(string id, string canvasNodeID, string x1, string y1, string x2, string y2)
        {
            ThreadPool.RunAsync(async delegate
            {
                Dictionary<string, string> props = new Dictionary<string, string>
            {
                {"x1", x1},
                {"x2", x2},
                {"y1", y1},
                {"y2", y2},
                {"id", id},
                {"canvasNodeID", canvasNodeID},
                {"type", "ink"},
                {"inkType", "partial"}
            };
                string m = MakeSubMessageFromDict(props);
                await _clientHandler.SendMassUDPMessage(m);
            });
        }

        public async Task RequestFinalizeGlobalInk(string previousID, string canvasNodeID,string data)
        {
            ThreadPool.RunAsync(async delegate
            {
                Dictionary<string, string> props = new Dictionary<string, string>
            {
                {"type", "ink"},
                {"inkType", "full"},
                {"canvasNodeID", canvasNodeID},
                {"id", _clientHandler.GetID()},
                {"data", data},
                {"previousID", previousID}
            };
                string m = MakeSubMessageFromDict(props);

                await _clientHandler.SendMessageToHost(m);
            });
        }
        public async Task RequestLock(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (HasSendableID(id))
                {
                    Debug.WriteLine("Requesting lock for ID: " + id);
                }
                else
                {
                    Debug.WriteLine("Requesting lock for ID: " + id + " although it doesn't exist yet");
                }
                await _clientHandler.SendMessageToHost("SPECIAL5:" + id, PacketType.TCP);
            });
        }

        public async Task RequestReturnLock(string id)
        {
            ThreadPool.RunAsync(async delegate
            {
                if (HasSendableID(id))
                {
                    Debug.WriteLine("Returning lock for ID: " + id);
                }
                else
                {
                    Debug.WriteLine("Attempted to return lock with ID: " + id + " When no such ID exists");
                    //throw new InvalidIDException(id);
                }
                await _clientHandler.SendMessageToHost("SPECIAL7:" + id);
                await _clientHandler.SendMassTCPMessage(MakeSubMessageFromDict(await GetNodeState(id)));
            });
        }
        #endregion publicRequests

        #region oldModelIntermediate
            public WorkSpaceModel WorkSpaceModel { get; set; }
            private async Task HandleMessage(Dictionary<string, string> props)
            {
                if (props.ContainsKey("id"))
                {
                    string id = props["id"];//get id from dictionary
                    _sendablesBeingUpdated.TryAdd(id, true);
                    if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                    {
                        Sendable n = WorkSpaceModel.IDToSendableDict[id];//if the id exists, get the sendable

                        await UITask.Run(async () => { await n.UnPack(props); });//update the sendable with the dictionary info
                    }
                    else//if the sendable doesn't yet exist
                    {

                        if (!_deletedIDs.ContainsKey(id))
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

                                Action<string> action;
                                _creationCallbacks.TryRemove(id, out action);
                            }
                        }
                    }
                    bool r;
                    _sendablesBeingUpdated.TryRemove(id, out r);
                }
                else
                {
                    Debug.WriteLine("ID was not found in property list of message: ");
                }
            }

            public bool IsSendableBeingUpdated(string id)
            {
                return _sendablesBeingUpdated.ContainsKey(id);
            }
            private async Task HandleCreateNewSendable(string id, Dictionary<string, string> props)
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

            private async Task HandleCreateNewPin(string id, Dictionary<string, string> props)
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
                    await UITask.Run(async () =>
                    {
                        await WorkSpaceModel.CreateNewPin(id, x, y);
                    });
                }
            }
            private async Task HandleCreateNewLink(string id, Dictionary<string, string> props)
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
                    await UITask.Run(async () => { WorkSpaceModel.CreateLink((AtomModel)WorkSpaceModel.IDToSendableDict[id1], (AtomModel)WorkSpaceModel.IDToSendableDict[id2], id); });

                }
            }
            private async Task HandleCreateNewNode(string id, Dictionary<string, string> props)
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
                        case NodeType.Text:
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
                        case NodeType.Audio:
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
                await UITask.Run(async () => { await WorkSpaceModel.CreateNewNode(props["id"], type, x, y, data); });
                if (props.ContainsKey("data"))
                {
                    string s;
                    props.Remove("data");
                }
            }

            private async Task HandleCreateNewEmptyGroup(string id, Dictionary<string, string> props)
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

                await UITask.Run(async () => { await WorkSpaceModel.CreateEmptyGroup(id, x, y); });
            }

            private async Task HandleCreateNewGroup(string id, Dictionary<string, string> props)
            {
                NodeModel node1 = null;
                NodeModel node2 = null;
                double x = 0;
                double y = 0;
                if (props.ContainsKey("id1") && props.ContainsKey("id2") && WorkSpaceModel.IDToSendableDict.ContainsKey(props["id1"]) && WorkSpaceModel.IDToSendableDict.ContainsKey(props["id2"]))
                {
                    node1 = (NodeModel)WorkSpaceModel.IDToSendableDict[props["id1"]];
                    node2 = (NodeModel)WorkSpaceModel.IDToSendableDict[props["id2"]];
                }
                if (props.ContainsKey("x"))
                {
                    double.TryParse(props["x"], out x);
                }
                if (props.ContainsKey("y"))
                {
                    double.TryParse(props["y"], out y);
                }
                await UITask.Run(async () => { await WorkSpaceModel.CreateGroup(id, node1, node2, x, y); });
            }
            private async Task HandleCreateNewInk(string id, Dictionary<string, string> props)
            {
                if (props.ContainsKey("canvasNodeID") && (HasSendableID(props["canvasNodeID"]) || props["canvasNodeID"] == "WORKSPACE_ID"))
                {
                    InqCanvasModel canvas = null;
                    if (props["canvasNodeID"] != "WORKSPACE_ID")
                    {
                        await UITask.Run(async delegate { canvas = ((NodeModel)WorkSpaceModel.IDToSendableDict[props["canvasNodeID"]]).InqCanvas; });
                    }
                    else
                    {
                        canvas = WorkSpaceModel.InqModel;
                    }
                    if (props.ContainsKey("inkType") && props["inkType"] == "partial")
                    {
                        Point one;
                        Point two;
                        ParseToLineSegment(props, out one, out two);

                        await UITask.Run(() =>
                        {
                            var lineModel = new InqLineModel(props["canvasNodeID"]);
                            var line = new InqLineView(new InqLineViewModel(lineModel), 2, new SolidColorBrush(Colors.Black));
                            PointCollection pc = new PointCollection();
                            pc.Add(one);
                            pc.Add(two);
                            lineModel.Points = pc;
                            canvas.AddTemporaryInqline(lineModel, id);
                        });
                    }
                    else if (props.ContainsKey("inkType") && props["inkType"] == "full")
                    {
                        await UITask.Run(async delegate {

                            PointCollection points;
                            double thickness;
                            SolidColorBrush stroke;

                            if (props.ContainsKey("data"))
                            {
                                InqLineModel.ParseToLineData(props["data"], out points, out thickness, out stroke);

                                if (props.ContainsKey("previousID") && WorkSpaceModel.InqModel.PartialLines.ContainsKey(props["previousID"]))
                                {
                                    canvas.OnFinalizedLine += async delegate
                                    {
                                        await UITask.Run(() => { canvas.RemovePartialLines(props["previousID"]); });
                                    };
                                }

                                var lineModel = new InqLineModel(id);
                                if (props.ContainsKey("canvasNodeID"))
                                {
                                    lineModel.ParentID = props["canvasNodeID"];
                                }
                                var line = new InqLineView(new InqLineViewModel(lineModel), thickness, stroke);
                                lineModel.Points = points;
                                lineModel.Stroke = stroke;
                                canvas.FinalizeLine(lineModel);
                                WorkSpaceModel.IDToSendableDict.Add(id, lineModel);

                            }
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("Ink creation failed because no canvas ID was given or the ID wasn't valid");
                }
            }
            private async Task RemoveSendable(string id)
            {
                await UITask.Run(async () => {
                    if (WorkSpaceModel.IDToSendableDict.ContainsKey(id))
                    {
                        WorkSpaceModel.RemoveSendable(id);
                    }
                    _deletedIDs.TryAdd(id, true);
                });
            }

            public bool HasSendableID(string id)
            {
                return WorkSpaceModel.IDToSendableDict.ContainsKey(id);
            }
            private async Task SetAtomLock(string id, string ip)
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
                Dictionary<string, Sendable> set = new Dictionary<string, Sendable>();

                foreach (KeyValuePair<string, Sendable> kvp in WorkSpaceModel.IDToSendableDict)
                {
                    set.Add(kvp.Key, kvp.Value);
                }

                while (set.Count > 0)
                {
                    Sendable s = set[set.Keys.First()];
                    if (s.GetType() != typeof(LinkModel) || (!set.ContainsKey(((LinkModel)s).InAtomID) &&
                        !set.ContainsKey(((LinkModel)s).OutAtomID)))
                    {
                        list.AddLast(s);
                        set.Remove(s.ID);
                    }
                }
                if (WorkSpaceModel.IDToSendableDict.Count > 0)
                {
                    string ret = "";
                    while (list.Count > 0)
                    {
                        Sendable atom = list.First.Value;
                        list.RemoveFirst();
                        string s = String.Empty;
                        if (atom is InqLineModel)
                        {
                            await UITask.Run(async delegate
                            {
                                s = await atom.Stringify();
                            });
                        }
                        else
                        {
                            s = await atom.Stringify();
                        }
                        ret += s;
                    }
                    return ret;
                }
                return "";
            }

            public async Task ReturnAllLocks()
            {
                List<string> locks = new List<string>();
                locks.AddRange(WorkSpaceModel.Locks.LocalLocks);
                while (locks.Count > 0)
                {
                    string l = locks.First();
                    locks.Remove(l);
                    await NetworkConnector.Instance.RequestReturnLock(l);
                }
            }

            private void AddCreationCallback(string id, Action<string> d)
            {
                _creationCallbacks.TryAdd(id, d);
            }
            public bool HasLock(string id)
            {
                if (!WorkSpaceModel.IDToSendableDict.ContainsKey(id)) return false;
                var sendable = WorkSpaceModel.IDToSendableDict[id];
                bool isLine = sendable is InqLineModel || sendable is PinModel; // TODO there should be no special casing for inks
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
                    await NetworkConnector.Instance.RequestReturnLock(l);
                }
            }

            private void RemoveIPFromLocks(string ip)
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
        
            private async Task ForceSetLocks(string message)
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
                if (HasSendableID(id))
                {
                    return await WorkSpaceModel.IDToSendableDict[id].Pack();
                }
                else
                {
                    return new Dictionary<string, string>();
                }
            }

            private string DictToString(IEnumerable<KeyValuePair<string, string>> dict)
            {
                string s = "";
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    s += kvp.Key + ":" + kvp.Value + "&";
                }
                s = s.Substring(0, Math.Max(s.Length - 1, 0));
                return s;
            }

            private Dictionary<string, string> StringToDict(string s)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                string[] strings = s.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string kvpString in strings)
                {
                    string[] kvpparts = kvpString.Split(new string[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (kvpparts.Length == 2)
                    {
                        dict.Add(kvpparts[0], kvpparts[1]);
                    }
                }
                return dict;
            }

            private void ParseToLineSegment(Dictionary<string, string> props, out Point one, out Point two)
            {
                one = new Point(Double.Parse(props["x1"]), Double.Parse(props["y1"]));
                two = new Point(Double.Parse(props["x2"]), Double.Parse(props["y2"]));
            }

    #endregion oldModelIntermediate

    #region customExceptions
    public class InvalidIDException : Exception
        {
            public InvalidIDException(string id) : base(String.Format("The ID {0}  was used but is invalid", id)) { }
        }
        public class IncorrectFormatException : Exception
        {
            public IncorrectFormatException(string message) : base(String.Format("The message '{0}' is incorrectly formatted or unrecognized", message)) { }
        }

        public class NotHostException : Exception
        {
            public NotHostException(string message, string remoteIP)
                : base(String.Format("The message {0} was sent to a non-host from IP: {1} when it is a host-only message", message, remoteIP))
            { }
        }

        public class HostException : Exception
        {
            public HostException(string message, string remoteIP) : base(String.Format("The message {0} was sent to this machine, THE HOST, from IP: {1} when it is meant for only non-hosts", message, remoteIP)) { }
        }
        public class UnknownIPException : Exception
        {
            public UnknownIPException(string ip) : base(String.Format("The IP {0} was used when it is not recgonized", ip)) { }
        }

        public class NoIDException : Exception
        {
            public NoIDException(string message) : base(message) { }
            public NoIDException() { }
        }

        public class InvalidCreationArgumentsException : Exception
        {
            public InvalidCreationArgumentsException(string message) : base(message) { }
            public InvalidCreationArgumentsException() { }
        }
        #endregion customExceptions
    }
}
