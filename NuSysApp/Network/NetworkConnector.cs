////using System;
////using System.Collections.Concurrent;
////using System.Collections.Generic;
////using System.Collections.ObjectModel;
////using System.Diagnostics;
////using System.Linq;
////using System.Text.RegularExpressions;
////using System.Threading.Tasks;
////using Windows.Foundation;
////using Windows.System.Threading;
////using Windows.UI;
////using Windows.UI.Xaml.Media;
////using NuSysApp;
////using NuSysApp.Network;

////namespace NuSysApp
////{
////    public class NetworkConnector
////    {
////        #region Private Members

////        private static volatile NetworkConnector _instance;
////        private static readonly object _syncRoot = new Object();
////        #endregion Private Members

////        #region Public Members
////        private ClientHandler _clientHandler;
////        public LockDictionary Locks { get { return SessionController.Instance.Locks; } }
////        private ConcurrentDictionary<string, bool> _deletedIDs;
////        private ConcurrentDictionary<string, Action<string>> _creationCallbacks;
////        private ConcurrentDictionary<string, bool> _sendablesBeingUpdated;
////        public enum PacketType
////        {
////            UDP,
////            TCP,
////            Both
////        }
////        public static NetworkConnector Instance
////        {
////            get
////            {
////                if (_instance == null)
////                {
////                    lock (_syncRoot)
////                    {
////                        if (_instance == null)
////                        {
////                            _instance = new NetworkConnector();
////                        }
////                    }
////                }
////                return _instance;
////            }
////        }
////        #endregion
////        private NetworkConnector()//pls keep this private or shit won't work anymore
////        {
////            _creationCallbacks = new ConcurrentDictionary<string, Action<string>>();
////            _sendablesBeingUpdated = new ConcurrentDictionary<string, bool>();
////            _deletedIDs = new ConcurrentDictionary<string, bool>();
////            _clientHandler = new ClientHandler(this);
////            _clientHandler.OnNewMessage += MessageRecieved;
////            _clientHandler.OnAllLocksSet += async delegate (string s) { await ForceSetLocks(s); };
////            _clientHandler.OnLockUpdateRecieved += async delegate (string id, string holder) { await SetAtomLock(id, holder); };
////            _clientHandler.OnRemoveIP += async delegate(string ip) { RemoveIPFromLocks(ip); };
////            _clientHandler.OnDeleteRequestRecieved += async delegate(string id) { RemoveSendable(id); };
////        }

////        private async void MessageRecieved(string ip, string message, PacketType packetType)
////        {

////            Message props = new Message(message);
////            //await HandleMessage(props); //handle each submessage
////            await HandleMessage(props);
////            if ((HasSendableID(props.GetString("id")) || (props.ContainsKey("nodeType") && props["nodeType"] == NodeType.PDF.ToString())) && packetType == PacketType.TCP && _clientHandler.IsHost())
////            {
////                await _clientHandler.SendMassTCPMessage(message);
////            }

////            /*
////            var matches = Regex.Match(message, "(?:({[^}]+}) *)*");
////            string[] miniStrings = matches.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();

////            //var miniStrings = message.Split(new string[] { Constants.AndReplacement }, StringSplitOptions.RemoveEmptyEntries); //break up message into subparts
////            foreach (var subMessage in miniStrings)
////            {
////                if (subMessage.Length > 0)
////                {
               

//                }
//            }*/
//        }

//        /*
//        * removes this local ip from the php script that keeps track of all the members on the server
//        */
//        public async Task Disconnect(string ip = null) //called by the closing of the application
//{
//    _clientHandler.Disconnect(ip);
//}

///*
//* handles and proccesses a regular sub-message
//*/
//private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
//{
//    if (_clientHandler.IsHost())//if host, add a new packet and store it in every joining member's stack of updates
//    {
//        /*
//        foreach (var kvp in _joiningMembers)
//        // keeps track of messages sent during initial loading into workspace
//        {
//            kvp.Value.Item2.Add(new Packet(message, packetType));
//            if (packetType == PacketType.TCP && !kvp.Value.Item1)
//            {
//                var tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
//                _joiningMembers[kvp.Key] = tup;
//            }
//        }*/
//    }

//    //Dictionary<string, string> props = ParseOutProperties(message);
//    Message props = new Message();
//    await props.Init(message);
//    if (props.ContainsKey("id"))
//    {
//        await HandleMessage(props);
//        if ((HasSendableID(props["id"]) || (props.ContainsKey("nodeType") && props["nodeType"] == NodeType.PDF.ToString())) && packetType == PacketType.TCP && _clientHandler.IsHost())
//        {
//            await _clientHandler.SendMassTCPMessage(message);
//        }
//    }
//    else
//    {
//        throw new InvalidIDException("there was no id, that's why this error is occurring...");
//    }
//}

///*
//* parses message to dictionary of properties
//*/
//private Dictionary<string, string> ParseOutProperties(string message)
//{
//    return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
//}

///*
//* makes a message from a dictionary of properties.  dict must have an ID
//*/
//public string MakeSubMessageFromDict(Dictionary<string, object> dict)
//{
//    return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
//}

//#region publicRequests
///*
//* PUBLIC request for deleting a nod 
//*/
//public async Task RequestDeleteSendable(string id)
//{
//    ThreadPool.RunAsync(async delegate
//    {
//        if (HasLock(id))
//        {
//            await _clientHandler.SendDeleteMessage(id); //tells host to delete the node
//                }
//    });
//}

///*
//* PUBLIC general method to update everyone from an Atom update.  sends mass udp packet
//*/
//public async Task QuickUpdateAtom(Dictionary<string, object> properties, PacketType packetType = PacketType.UDP)
//{
//    ThreadPool.RunAsync(async delegate
//    {
//        if (properties.ContainsKey("id"))
//        {
//            if (HasSendableID(properties["id"].ToString()))
//            {
//                string message = MakeSubMessageFromDict(properties);
//                await _clientHandler.SendMassMessage(message, packetType);
//                if (packetType == PacketType.TCP)
//                {
//                    await HandleRegularMessage(_clientHandler.LocalIP(), message, packetType);
//                }
//            }
//            else
//            {
//                throw new InvalidIDException(properties["id"].ToString());
//                return;
//            }
//        }
//        else
//        {
//                    //TODO: is sometimes thrown!
//                    //throw new NoIDException();
//                    return;
//        }
//    });
//}


//public async Task<string> RequestDuplicateNode(Dictionary<string, object> packed, Action<string> callback = null)
//{
//    var id = _clientHandler.GetID();
//    packed["id"] = id;
//    if (callback != null)
//    {
//        AddCreationCallback(id, callback);
//    }

//    packed["type"] = "duplicate";

//    string message = MakeSubMessageFromDict(packed);
//    await _clientHandler.SendMessageToHost(message);
//    return id;
//}

///*
//* PUBLIC general method to create Node
//*/
//public async Task<string> RequestMakeNode(string x, string y, string nodeType, string contentId = null, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
//{
//    string id = null;
//    await ThreadPool.RunAsync(async delegate
//    {
//        if (x != "" && y != "" && nodeType != "")
//        {
//            var props = properties == null ? new Dictionary<string, object>() : properties;
//            id = oldID == null ? _clientHandler.GetID() : oldID;
//            Debug.WriteLine("========== " + id + "============");
//            props["x"] = x;
//            props["y"] = y;
//            props["nodeType"] = nodeType;
//            props["type"] = "node";
//            props["id"] = id;

//            props["contentId"] = contentId;

//            if (callback != null)
//            {
//                AddCreationCallback(id, callback);
//            }

//            string message = MakeSubMessageFromDict(props);

//            await _clientHandler.SendMessageToHost(message);
//        }
//        else
//        {
//            throw new InvalidCreationArgumentsException();
//            return;
//        }
//    });
//    return id;
//}

///*
//* PUBLIC general method to create Pin
//*/
//public async Task RequestMakePin(string x, string y, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
//{
//=======
////                }
////            }*/
////        }

////        /*
////        * removes this local ip from the php script that keeps track of all the members on the server
////        */
////        public async Task Disconnect(string ip = null) //called by the closing of the application
////        {
////            _clientHandler.Disconnect(ip);
////        }

////        /*
////        * handles and proccesses a regular sub-message
////        */
////        private async Task HandleRegularMessage(string ip, string message, PacketType packetType)
////        {
////            if (_clientHandler.IsHost())//if host, add a new packet and store it in every joining member's stack of updates
////            {
////                /*
////                foreach (var kvp in _joiningMembers)
////                // keeps track of messages sent during initial loading into workspace
////                {
////                    kvp.Value.Item2.Add(new Packet(message, packetType));
////                    if (packetType == PacketType.TCP && !kvp.Value.Item1)
////                    {
////                        var tup = new Tuple<bool, List<Packet>>(true, kvp.Value.Item2);
////                        _joiningMembers[kvp.Key] = tup;
////                    }
////                }*/
////            }

////            //Dictionary<string, string> props = ParseOutProperties(message);
////            Message props = new Message(message);
////            if (props.ContainsKey("id"))
////            {
////                await HandleMessage(props);
////                if ((HasSendableID(props.GetString("id")) || (props.ContainsKey("nodeType") && props["nodeType"]==NodeType.PDF.ToString()))&& packetType == PacketType.TCP && _clientHandler.IsHost())
////                {
////                    await _clientHandler.SendMassTCPMessage(message);
////                }
////            }
////            else
////            {
////                throw new InvalidIDException("there was no id, that's why this error is occurring...");
////            }
////        }

////        /*
////        * parses message to dictionary of properties
////        */
////        private Dictionary<string, string> ParseOutProperties(string message)
////        {
////            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>(message);
////        }

////        /*
////        * makes a message from a dictionary of properties.  dict must have an ID
////        */
////        public string MakeSubMessageFromDict(Dictionary<string, object> dict)
////        {
////            return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
////        }

////        #region publicRequests
////        /*
////        * PUBLIC request for deleting a nod 
////        */
////        public async Task RequestDeleteSendable(string id)
////        {
////            ThreadPool.RunAsync(async delegate
////            {
////                if (HasLock(id))
////                {
////                    await _clientHandler.SendDeleteMessage(id); //tells host to delete the node
////                }
////            });
////        }

////        /*
////        * PUBLIC general method to update everyone from an Atom update.  sends mass udp packet
////        */
////        public async Task QuickUpdateAtom(Dictionary<string, object> properties, PacketType packetType = PacketType.UDP)
////        {
////            ThreadPool.RunAsync(async delegate
////            {
////                if (properties.ContainsKey("id"))
////                {
////                    if (HasSendableID(properties["id"].ToString()))
////                    {
////                        string message = MakeSubMessageFromDict(properties);
////                        await _clientHandler.SendMassMessage(message, packetType);
////                        if (packetType == PacketType.TCP)
////                        {
////                            await HandleRegularMessage(_clientHandler.LocalIP(), message, packetType);
////                        }
////                    }
////                    else
////                    {
////                        throw new InvalidIDException(properties["id"].ToString());
////                        return;
////                    }
////                }
////                else
////                {
////                    //TODO: is sometimes thrown!
////                    //throw new NoIDException();
////                    return;
////                }
////            });
////        }


////        public async Task<string> RequestDuplicateNode(Dictionary<string, object> packed, Action<string> callback = null)
////        {
////            var id = _clientHandler.GetID();
////            packed["id"] = id;
////            if (callback != null)
////            {
////                AddCreationCallback(id, callback);
////            }

////            packed["type"] = "duplicate";


////            string message = MakeSubMessageFromDict(packed);

////            await _clientHandler.SendMessageToHost(message);

////            return id;
////        }

////        /*
////        * PUBLIC general method to create Node
////        */
////        public async Task<string> RequestMakeNode(string x, string y, string nodeType, string contentId = null, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
////        {
////            string id = null;
////            await ThreadPool.RunAsync(async delegate
////            {
////                if (x != "" && y != "" && nodeType != "")
////                {
////                    var props = properties == null ? new Dictionary<string, object>() : properties;
////                    id = oldID == null ? _clientHandler.GetID() : oldID;
////                    Debug.WriteLine("========== " + id + "============");
////                    props["x"] = x;
////                    props["y"] = y;
////                    props["nodeType"] = nodeType;
////                    props["type"] = "node";
////                    props["id"] = id;
////                    if (!props.ContainsKey("creator"))
////                        props["creator"] = SessionController.Instance.ActiveWorkspace.Id;

////                    props["contentId"] = contentId;

////                    if (callback != null)
////                    {
////                        AddCreationCallback(id, callback);
////                    }

////                    string message = MakeSubMessageFromDict(props);

////                    await _clientHandler.SendMessageToHost(message);
////                }
////                else
////                {
////                    throw new InvalidCreationArgumentsException();
////                    return;
////                }
////            });
////            return id;
////        }

////        /*
////        * PUBLIC general method to create Pin
////        */
////        public async Task RequestMakePin(string x, string y, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
////        {
//>>>>>>> origin / phil_groups_new_network

////            var props = properties == null ? new Dictionary<string, object>() : properties;
////            string id = oldID == null ? _clientHandler.GetID() : oldID;
////            props["x"] = x;
////            props["y"] = y;
////            props["type"] = "pin";
////            props["id"] = id;
////            string message = MakeSubMessageFromDict(props);
////            if (callback != null)
////            {
////                AddCreationCallback(oldID, callback);
////            }
////            await _clientHandler.SendMessageToHost(message);
////        }

////        /*
////        * PUBLIC general method to create Collection
////        * TODO factor this into one method with RequestMakeEmptyGroup that takes in a list os ID's to place in that group
////        */
////        public async Task RequestMakeGroup(string id1, string id2, string x, string y, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
////        {
////            if (id1 != "" && id2 != "")
////            {

////                        var props = properties == null ? new Dictionary<string, object>() : properties;
////                        string id = oldID == null ? _clientHandler.GetID() : oldID;
////                        props["id1"] = id1;
////                        props["id2"] = id2;
////                        props["x"] = x;
////                        props["y"] = y;
////                        props["id"] = id;
////                        props["type"] = "group";
////                        if (callback != null)
////                        {
////                            AddCreationCallback(id, callback);
////                        }
////                        string message = MakeSubMessageFromDict(props);
////                        await _clientHandler.SendMessageToHost(message);
////                    }

////            else
////            {
////                throw new InvalidCreationArgumentsException();
////                return;
////            }
////        }

////        /*
////       * PUBLIC general method to create Collection
////       * TODO merge this into one request make group method
////       */


////        public async Task RequestNewGroupTag(string x, string y, string title, Dictionary<string, object> properties = null, Action<string> callback = null)
////        {

////            var props = properties == null ? new Dictionary<string, object>() : properties;
////            string id = _clientHandler.GetID();
////            props["x"] = x;
////            props["y"] = y;
////            props["id"] = id;
////            props["type"] = "grouptag";
////            props["title"] = title;
////            if (callback != null)
////            {
////                AddCreationCallback(id, callback);
////            }
////            string message = MakeSubMessageFromDict(props);
////            await _clientHandler.SendMessageToHost(message);
////        }

////        /*
////        * PUBLIC general method to create Linq
////        */
////        public async Task RequestMakeLinq(string id1, string id2, string oldID = null, Dictionary<string, object> properties = null, Action<string> callback = null)
////        {
////            if (id1 == id2)
////            {
////                throw new InvalidCreationArgumentsException("the two ids for the link were identical");
////            }
////            if (id1 != "" && id2 != "" && HasSendableID(id1) && HasSendableID(id2))
////            {
////                var props = properties == null ? new Dictionary<string, object>() : properties;
////                string id = oldID == null ? _clientHandler.GetID() : oldID;
////                props["id1"] = id1;
////                props["id2"] = id2;
////                props["type"] = "link";
////                props["id"] = id;
////                props["creator"] = SessionController.Instance.ActiveWorkspace.Id;

////                if (callback != null)
////                {
////                    AddCreationCallback(oldID, callback);
////                }

////                string message = MakeSubMessageFromDict(props);

////                await _clientHandler.SendMessageToHost(message);
////            }
////            else
////            {
////                throw new InvalidCreationArgumentsException();
////                return;
////            }
////        }

////        public string LocalIP
////        {
////            get { return _clientHandler.LocalIP(); }
////        }
////        public async Task RequestSendPartialLine(string id, string canvasNodeID, string x1, string y1, string x2, string y2, string color = "black")
////        {
////            ThreadPool.RunAsync(async delegate
////            {
////                var props = new Dictionary<string, object>
////            {
////                {"x1", x1},
////                {"x2", x2},
////                {"stroke",color },
////                {"y1", y1},
////                {"y2", y2},
////                {"id", id},
////                {"canvasNodeID", canvasNodeID},
////                {"type", "ink"},
////                {"inkType", "partial"}
////            };
////                string m = MakeSubMessageFromDict(props);
////                await _clientHandler.SendMassUDPMessage(m);
////            });
////        }

////        public async Task RequestFinalizeGlobalInk(Dictionary<string, object> props)
////        {
////            ThreadPool.RunAsync(async delegate
////            {

////                string m = MakeSubMessageFromDict(props);

//                await _clientHandler.SendMessageToHost(m);
//});
//        }
//        public async Task RequestLock(string id)
//{
//    ThreadPool.RunAsync(async delegate
//    {
//        if (HasSendableID(id))
//        {
//            Debug.WriteLine("Requesting lock for ID: " + id);
//        }
//        else
//        {
//            Debug.WriteLine("Requesting lock for ID: " + id + " although it doesn't exist yet");
//        }
//        await _clientHandler.RequestLock(id);
//    });
//}

//public async Task RequestReturnLock(string id)
//{
//    ThreadPool.RunAsync(async delegate
//    {
//        if (HasSendableID(id))
//        {
//            Debug.WriteLine("Returning lock for ID: " + id);
//        }
//        else
//        {
//            Debug.WriteLine("Attempted to return lock with ID: " + id + " When no such ID exists");
//                    //throw new InvalidIDException(id);
//                }
//        await _clientHandler.ReturnLock(id);
//        await _clientHandler.SendMassTCPMessage(MakeSubMessageFromDict(await GetNodeState(id)));
//    });
//}
//#endregion publicRequests

//#region oldModelIntermediate

//private async Task HandleMessage(Message props, bool justCreated = false)
//{
//    if (props.ContainsKey("id"))
//    {
//        string id = props["id"];//get id from dictionary
//        _sendablesBeingUpdated.TryAdd(id, true);
//        if (SessionController.Instance.IdToSendables.ContainsKey(id))
//        {
//            Sendable n = SessionController.Instance.IdToSendables[id];//if the id exists, get the sendable

//            await UITask.Run(async () =>
//            {
//                await n.UnPack(props);
//                if (justCreated && n is AtomModel)
//                {
    //                    var creators = (n as AtomModel).Creator;
    //                    if (creators.Count > 0)
    //                    {
    //                        foreach (var creator in creators)
    //                        {
    //                            await (SessionController.Instance.IdToSendables[creator] as NodeContainerModel).AddChild(n);
    //                        }
    //                    }
    //                    else
    //                        await (SessionController.Instance.ActiveWorkspace.Model as WorkspaceModel).AddChild(n);
//                }
//            });//update the sendable with the dictionary info
//        }
//        else//if the sendable doesn't yet exist
//        {

//            if (!_deletedIDs.ContainsKey(id))
//            {
//                await HandleCreateNewSendable(id, props); //create a new sendable
//                if (SessionController.Instance.IdToSendables.ContainsKey(id))
//                {
//                    await HandleMessage(props, true);
//                }
//                if (_creationCallbacks.ContainsKey(id))
//                //check if a callback is waiting for that sendable to be created
//                {
//                    _creationCallbacks[id].DynamicInvoke(id);

//                    Action<string> action;
//                    _creationCallbacks.TryRemove(id, out action);
//                }
//            }
//        }
//        bool r;
//        _sendablesBeingUpdated.TryRemove(id, out r);
//    }
//    else
//    {
//        Debug.WriteLine("ID was not found in property list of message: ");
//    }
//}

//public bool IsSendableBeingUpdated(string id)
//{
//    return _sendablesBeingUpdated.ContainsKey(id);
//}
//private async Task HandleCreateNewSendable(string id, Message props)
//{
//    if (props.ContainsKey("type") && props["type"] == "duplicate")
//    {
//        await HandleCreateDuplicate(id, props);
//    }

//    if (props.ContainsKey("type") && props["type"] == "ink")
//    {
//        await HandleCreateNewInk(id, props);
//    }
//    else if (props.ContainsKey("type") && props["type"] == "group")
//    {
//        await HandleCreateNewGroup(id, props);
//    }
//    else if (props.ContainsKey("type") && props["type"] == "grouptag")
//    {
//        await HandleCreateNewGroupTag(id, props);
//    }
//    else if (props.ContainsKey("type") && props["type"] == "node")
//    {
//        await HandleCreateNewNode(id, props);
//    }
//    else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
//    {
//        await HandleCreateNewLink(id, props);
//    }
//    else if (props.ContainsKey("type") && (props["type"] == "pin"))
//    {
//        await HandleCreateNewPin(id, props);
//    }
//}

//private async Task HandleCreateNewPin(string id, Message props)
//{
//    double x = 0;
//    double y = 0;
//    if (props.ContainsKey("x") && props.ContainsKey("y"))
//    {
//        try
//        {
//            x = double.Parse(props["x"]);
//            y = double.Parse(props["y"]);
//        }
//        catch (Exception e)
//        {
//            Debug.WriteLine("Pin creation failed because coordinates could not be parsed to doubles");
//        }
//        await UITask.Run(async () =>
//        {
//            await SessionController.Instance.CreateNewPin(id, x, y);
//        });
//    }
//}
//private async Task HandleCreateNewLink(string id, Message props)
//{
//    string id1 = "null";
//    string id2 = "null";
//    if (props.ContainsKey("id1"))
//    {
//        id1 = props["id1"];
//    }
//    else
//    {
//        Debug.WriteLine("Could not create link");
//        return;
//    }
//    if (props.ContainsKey("id2"))
//    {
//        id2 = props["id2"];
//    }
//    else
//    {
//        Debug.WriteLine("Could not create link");
//        return;
//    }

//    if (SessionController.Instance.IdToSendables.ContainsKey(id1) && (SessionController.Instance.IdToSendables.ContainsKey(id2)))
//    {
//        await UITask.Run(async () => { SessionController.Instance.CreateLink((AtomModel)SessionController.Instance.IdToSendables[id1], (AtomModel)SessionController.Instance.IdToSendables[id2], id); });

//    }
//}

//private async Task HandleCreateDuplicate(string id, Message props)
//{
//    NodeType type = NodeType.Text;
//    if (props.ContainsKey("nodeType"))
//    {
//        string t = props["nodeType"];
//        type = (NodeType)Enum.Parse(typeof(NodeType), t);
//    }

//    if (type == NodeType.Collection)
//    {

//        var childList = props.GetList<string>("groupChildren");
//        foreach (var childId in childList)
//        {

//            var childModel = (AtomModel)SessionController.Instance.IdToSendables[childId];
//            var groups = (List<string>)childModel.GetMetaData("groups");
//            childModel.Creator.Add(id);
//            groups.Add(id);
//        }
//    }


//    await UITask.Run(async () =>
//    {
//    await SessionController.Instance.CreateNewNode(id, type);
//=======
////                await _clientHandler.SendMessageToHost(m);
////            });
////        }
////        public async Task RequestLock(string id)
////        {
////            ThreadPool.RunAsync(async delegate
////            {
////                if (HasSendableID(id))
////                {
////                    Debug.WriteLine("Requesting lock for ID: " + id);
////                }
////                else
////                {
////                    Debug.WriteLine("Requesting lock for ID: " + id + " although it doesn't exist yet");
////                }
////                await _clientHandler.RequestLock(id);
////            });
////        }

////        public async Task RequestReturnLock(string id)
////        {
////            ThreadPool.RunAsync(async delegate
////            {
////                if (HasSendableID(id))
////                {
////                    Debug.WriteLine("Returning lock for ID: " + id);
////                }
////                else
////                {
////                    Debug.WriteLine("Attempted to return lock with ID: " + id + " When no such ID exists");
////                    //throw new InvalidIDException(id);
////                }
////                await _clientHandler.ReturnLock(id);
////                await _clientHandler.SendMassTCPMessage(MakeSubMessageFromDict(await GetNodeState(id)));
////            });
////        }
////        #endregion publicRequests

////        #region oldModelIntermediate

////        private async Task HandleMessage(Message props, bool justCreated = false)
////            {
////                if (props.ContainsKey("id"))
////                {
////                    string id = props.GetString("id");//get id from dictionary
////                    _sendablesBeingUpdated.TryAdd(id, true);
////                    if (SessionController.Instance.IdToSendables.ContainsKey(id))
////                    {
////                        Sendable n = SessionController.Instance.IdToSendables[id];//if the id exists, get the sendable

////                        await UITask.Run(async () =>
////                        {
////                            await n.UnPack(props);
////                            if (justCreated && n is AtomModel)
////                            {
////                                var creator = (n as AtomModel).Creator;
////                                if (creator != null)
////                                    await (SessionController.Instance.IdToSendables[creator] as NodeContainerModel).AddChild(n);
////                                else
////                                    await (SessionController.Instance.ActiveWorkspace.Model as WorkspaceModel).AddChild(n);
////                            }
////                        });//update the sendable with the dictionary info
////                    }
////                    else//if the sendable doesn't yet exist
////                    {

////                        if (!_deletedIDs.ContainsKey(id))
////                        {
////                            await HandleCreateNewSendable(id, props); //create a new sendable
////                            if (SessionController.Instance.IdToSendables.ContainsKey(id))
////                            {
////                                await HandleMessage(props, true);
////                            }
////                            if (_creationCallbacks.ContainsKey(id))
////                            //check if a callback is waiting for that sendable to be created
////                            {
////                                _creationCallbacks[id].DynamicInvoke(id);

////                                Action<string> action;
////                                _creationCallbacks.TryRemove(id, out action);
////                            }
////                        }
////                    }
////                    bool r;
////                    _sendablesBeingUpdated.TryRemove(id, out r);
////                }
////                else
////                {
////                    Debug.WriteLine("ID was not found in property list of message: ");
////                }
////            }

////            public bool IsSendableBeingUpdated(string id)
////            {
////                return _sendablesBeingUpdated.ContainsKey(id);
////            }
////            private async Task HandleCreateNewSendable(string id, Message props)
////            {
////                if (props.ContainsKey("type") && props["type"] == "duplicate")
////                {
////                    await HandleCreateDuplicate(id, props);
////                }

////                if (props.ContainsKey("type") && props["type"] == "ink")
////                {
////                    await HandleCreateNewInk(id, props);
////                }
////                else if (props.ContainsKey("type") && props["type"] == "group")
////                {
////                    await HandleCreateNewGroup(id, props);
////                }
////                else if (props.ContainsKey("type") && props["type"] == "grouptag")
////                {
////                    await HandleCreateNewGroupTag(id, props);
////                }
////                else if (props.ContainsKey("type") && props["type"] == "node")
////                {
////                    await HandleCreateNewNode(id, props);
////                }
////                else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
////                {
////                    await HandleCreateNewLink(id, props);
////                }
////                else if (props.ContainsKey("type") && (props["type"] == "pin"))
////                {
////                    await HandleCreateNewPin(id, props);
////                }
////            }

////            private async Task HandleCreateNewPin(string id, Message props)
////            {
////                double x = 0;
////                double y = 0;
////                if (props.ContainsKey("x") && props.ContainsKey("y"))
////                {
////                    try
////                    {
////                        x = double.Parse(props.GetString("x"));
////                        y = double.Parse(props.GetString("y"));
////                    }
////                    catch (Exception e)
////                    {
////                        Debug.WriteLine("Pin creation failed because coordinates could not be parsed to doubles");
////                    }
////                    await UITask.Run(async () =>
////                    {
////                        await SessionController.Instance.CreateNewPin(id, x, y);
////                    });
////                }
////            }
////            private async Task HandleCreateNewLink(string id,Message props)
////            {
////                string id1 = "null";
////                string id2 = "null";
////                if (props.ContainsKey("id1"))
////                {
////                    id1 = props.GetString("id1");
////                }
////                else
////                {
////                    Debug.WriteLine("Could not create link");
////                    return;
////                }
////                if (props.ContainsKey("id2"))
////                {
////                    id2 = props.GetString("id2");
////                }
////                else
////                {
////                    Debug.WriteLine("Could not create link");
////                    return;
////                }

////                if (SessionController.Instance.IdToSendables.ContainsKey(id1) && (SessionController.Instance.IdToSendables.ContainsKey(id2)))
////                {
////                    await UITask.Run(async () => { SessionController.Instance.CreateLink((AtomModel)SessionController.Instance.IdToSendables[id1], (AtomModel)SessionController.Instance.IdToSendables[id2], id); });

////                }
////            }

////        private async Task HandleCreateDuplicate(string id, Message props)
////        {
////            NodeType type = NodeType.Text;
////            if (props.ContainsKey("nodeType"))
////            {
////                string t = props.GetString("nodeType");
////                type = (NodeType)Enum.Parse(typeof(NodeType), t);
////            }

////            if (type == NodeType.Collection) { 

////                var childList = props.GetList<string>("groupChildren");
////                foreach (var childId in childList)
////                {

////                    var childModel = (AtomModel)SessionController.Instance.IdToSendables[childId];
////                    var groups = (List<string>)childModel.GetMetaData("groups");
////                    groups.Add(id);
////                }
////            }


////            await UITask.Run(async () =>
////            {
////                await SessionController.Instance.CreateNewNode(id, type);
//>>>>>>> origin / phil_groups_new_network

////            });
////        }

////        private async Task HandleCreateNewNode(string id, Message props)
////            {
////                NodeType type = NodeType.Text;
////                if (props.ContainsKey("nodeType"))
////                {
////                    string t = props.GetString("nodeType");
////                    type = (NodeType)Enum.Parse(typeof(NodeType), t);
////                }

////                await UITask.Run(async () => { await SessionController.Instance.CreateNewNode(props.GetString("id"), type); });
////            }




//            private async Task HandleCreateNewGroup(string id, Message props)
//{
//    NodeModel node1 = null;
//    NodeModel node2 = null;
//    double x = 0;
//    double y = 0;
//    if (props.ContainsKey("id1") && props.ContainsKey("id2") && SessionController.Instance.IdToSendables.ContainsKey(props["id1"]) && SessionController.Instance.IdToSendables.ContainsKey(props["id2"]))
//    {
//        node1 = (NodeModel)SessionController.Instance.IdToSendables[props["id1"]];
//        node2 = (NodeModel)SessionController.Instance.IdToSendables[props["id2"]];
//    }
//    if (props.ContainsKey("x"))
//    {
//        double.TryParse(props["x"], out x);
//    }
//    if (props.ContainsKey("y"))
//    {
//        double.TryParse(props["y"], out y);
//    }

//    var workspaceId = SessionController.Instance.ActiveWorkspace.Id;

//    await UITask.Run(async () =>
//    {
//        if (node2 is NodeContainerModel)
//        {
//            node1.Creator.Add(node2.Id);
//            var prevGroups1 = (List<string>)node1.GetMetaData("groups");
//            prevGroups1.Add(node2.Id);
//            node1.SetMetaData("groups", prevGroups1);
//            await (node2 as NodeContainerModel).AddChild(node1);


//        }
//        else
//        {
//            await SessionController.Instance.CreateGroup(id, node1, node2, x, y);
//        }

//    });
//}

//private async Task HandleCreateNewGroupTag(string id, Message props)
//{
//    double x = 0;
//    double y = 0;
//    double w = 0;
//    double h = 0;
//    string title = string.Empty;
//    if (props.ContainsKey("x"))
//    {
//        double.TryParse(props["x"], out x);
//    }
//    if (props.ContainsKey("y"))
//    {
//        double.TryParse(props["y"], out y);
//    }
//    if (props.ContainsKey("width"))
//    {
//        double.TryParse(props["width"], out w);
//    }
//    if (props.ContainsKey("height"))
//    {
//        double.TryParse(props["height"], out h);
//    }
//    if (props.ContainsKey("title"))
//    {
//        title = props["title"];
//    }
//    await UITask.Run(async () => { await SessionController.Instance.CreateGroupTag(id, x, y, w, h, title); });
//}

//private async Task HandleCreateNewInk(string id, Message props)
//{
//=======
////            private async Task HandleCreateNewGroup(string id, Message props)
////            {
////                NodeModel node1 = null;
////                NodeModel node2 = null;
////                double x = 0;
////                double y = 0;
////                if (props.ContainsKey("id1") && props.ContainsKey("id2") && SessionController.Instance.IdToSendables.ContainsKey(props.GetString("id1")) && SessionController.Instance.IdToSendables.ContainsKey(props.GetString("id2")))
////                {
////                    node1 = (NodeModel)SessionController.Instance.IdToSendables[props.GetString("id1")];
////                    node2 = (NodeModel)SessionController.Instance.IdToSendables[props.GetString("id2")];
////                }
////                if (props.ContainsKey("x"))
////                {
////                    double.TryParse(props.GetString("x"), out x);
////                }
////                if (props.ContainsKey("y"))
////                {
////                    double.TryParse(props.GetString("y"), out y);
////                }
////                await UITask.Run(async () => { await SessionController.Instance.CreateGroup(id, node1, node2, x, y); });
////            }

////        private async Task HandleCreateNewGroupTag(string id, Message props)
////        {
////            double x = 0;
////            double y = 0;
////            double w = 0;
////            double h = 0;
////            string title = string.Empty;
////            if (props.ContainsKey("x"))
////            {
////                double.TryParse(props.GetString("x"), out x);
////            }
////            if (props.ContainsKey("y"))
////            {
////                double.TryParse(props.GetString("y"), out y);
////            }
////            if (props.ContainsKey("width"))
////            {
////                double.TryParse(props.GetString("width"), out w);
////            }
////            if (props.ContainsKey("height"))
////            {
////                double.TryParse(props.GetString("height"), out h);
////            }
////            if (props.ContainsKey("title"))
////            {
////                title = props.GetString("title");
////            }
////            await UITask.Run(async () => { await SessionController.Instance.CreateGroupTag(id, x, y, w, h, title); });
////        }

////        private async Task HandleCreateNewInk(string id, Message props) {
//>>>>>>> origin / phil_groups_new_network

////                if (props.ContainsKey("canvasNodeID") && (HasSendableID(props.GetString("canvasNodeID"))))
////                {
////                    var canvas = (InqCanvasModel) (SessionController.Instance.IdToSendables[props.GetString("canvasNodeID")] as NodeModel).InqCanvas;

////                    if (props.ContainsKey("inkType") && props["inkType"] == "partial")
////                    {
////                        Point2d one;
////                        Point2d two;
////                        ParseToLineSegment(props, out one, out two);

////                        await UITask.Run(() =>
////                        {
////                            var lineModel = new InqLineModel(props.GetString("canvasNodeID"));
////                          //  var line = new InqLineView(new InqLineViewModel(lineModel), 2, new SolidColorBrush(Colors.Black));
////                            var pc = new ObservableCollection<Point2d>();
////                            pc.Add(one);
////                            pc.Add(two);
////                            lineModel.Points = pc;
////                            lineModel.Stroke = new SolidColorBrush(Colors.Black);
////                            if (props.ContainsKey("stroke") && props["stroke"] != "black")
////                            {
////                                lineModel.Stroke = new SolidColorBrush(Colors.Yellow);
////                            }
////                            canvas.AddTemporaryInqline(lineModel, id);
////                        });
////                    }
////                    else if (props.ContainsKey("inkType") && props["inkType"] == "full")
////                    {
////                        await UITask.Run(async delegate {

////                            var lineModel = new InqLineModel(id);
////                            await lineModel.UnPack(props);
////                            canvas.FinalizeLine(lineModel);



////                        });
////                    }
////                }
////                else
////                {
////                    Debug.WriteLine("Ink creation failed because no canvas ID was given or the ID wasn't valid");
////                }
////            }
////            private async Task RemoveSendable(string id)
////            {
////                await UITask.Run(async () => {
////                    if (SessionController.Instance.IdToSendables.ContainsKey(id))
////                    {
////                        SessionController.Instance.RemoveSendable(id);
////                    }
////                    _deletedIDs.TryAdd(id, true);
////                });
////            }

////            public bool HasSendableID(string id)
////            {
////                return SessionController.Instance.IdToSendables.ContainsKey(id);
////            }
////            private async Task SetAtomLock(string id, string ip)
////            {
////                if (!HasSendableID(id))
////                {
////                    Debug.WriteLine("got lock update from unknown node");
////                    return;
////                }
////                await SessionController.Instance.Locks.Set(id, ip);
////            }

////            private byte[] ParseToByteArray(string s)
////            {
////                return Convert.FromBase64String(s);
////            }

////            private HashSet<string> LocksNeeded(List<string> ids)
////            {
////                HashSet<string> set = new HashSet<string>();
////                foreach (string id in ids)
////                {
////                    if (HasSendableID(id))
////                    {
////                        set.Add(id);
////                        //TODO make this method return a set of all associated atoms needing to be locked as well.
////                        return set;
////                    }
////                }
////                return new HashSet<string>();
////            }
////            public async Task<string> GetFullWorkspace()
////            {
////                LinkedList<Sendable> list = new LinkedList<Sendable>();
////                Dictionary<string, Sendable> set = new Dictionary<string, Sendable>();

////                foreach (KeyValuePair<string, Sendable> kvp in SessionController.Instance.IdToSendables)
////                {
////                    set.Add(kvp.Key, kvp.Value);
////                }

////                while (set.Count > 0)
////                {
////                    Sendable s = set[set.Keys.First()];
////                    if (s.GetType() != typeof(LinkModel) || (!set.ContainsKey(((LinkModel)s).InAtomID) &&
////                        !set.ContainsKey(((LinkModel)s).OutAtomID)))
////                    {
////                        list.AddLast(s);
////                        set.Remove(s.Id);
////                    }
////                }
////                if (SessionController.Instance.IdToSendables.Count > 0)
////                {
////                    string ret = "";
////                    while (list.Count > 0)
////                    {
////                        Sendable atom = list.First.Value;
////                        list.RemoveFirst();
////                        string s = String.Empty;
////                        if (atom is InqLineModel)
////                        {
////                            await UITask.Run(async delegate
////                            {
////                                s = await atom.Stringify();
////                            });
////                        }
////                        else
////                        {
////                            s = await atom.Stringify();
////                        }
////                        ret += s;
////                    }
////                    return ret;
////                }
////                return "";
////            }

////            public async Task ReturnAllLocks()
////            {
////                List<string> locks = new List<string>();
////                locks.AddRange(SessionController.Instance.Locks.LocalLocks);
////                while (locks.Count > 0)
////                {
////                    string l = locks.First();
////                    locks.Remove(l);
////                    //await NetworkConnector.Instance.RequestReturnLock(l);
////                }
////            }

////            private void AddCreationCallback(string id, Action<string> d)
////            {
////                _creationCallbacks.TryAdd(id, d);
////            }
////            public bool HasLock(string id)
////            {
////                if (!SessionController.Instance.IdToSendables.ContainsKey(id)) return false;
////                var sendable = SessionController.Instance.IdToSendables[id];
////                bool isLine = sendable is InqLineModel || sendable is PinModel; // TODO there should be no special casing for inks
////                return isLine || (SessionController.Instance.Locks.ContainsID(id) && SessionController.Instance.Locks.Value(id) == //NetworkConnector.Instance.LocalIP);
////            }

////            public async Task CheckLocks(List<string> ids)
////            {
////                Debug.WriteLine("Checking locks");
////                HashSet<string> locksNeeded = LocksNeeded(ids);
////                List<string> locksToReturn = new List<string>();
////                foreach (string lockID in SessionController.Instance.Locks.LocalLocks)
////                {
////                    if (!locksNeeded.Contains(lockID))
////                    {
////                        locksToReturn.Add(lockID);
////                    }
////                }
////                while (locksToReturn.Count > 0)
////                {
////                    string l = locksToReturn.First();
////                    locksToReturn.Remove(l);
////                    //await NetworkConnector.Instance.RequestReturnLock(l);
////                }
////            }

////            private void RemoveIPFromLocks(string ip)
////            {
////                if (SessionController.Instance.Locks.ContainsHolder(ip))
////                {
////                    foreach (KeyValuePair<string, string> kvp in SessionController.Instance.Locks)
////                    {
////                        if (kvp.Value == ip)
////                        {
////                            SetAtomLock(kvp.Key, "");
////                            if (!SessionController.Instance.Locks.ContainsHolder(ip))
////                            {
////                                return;
////                            }
////                        }
////                    }
////                }
////            }

////            private async Task ForceSetLocks(string message)
////            {
////                SessionController.Instance.Locks.Clear();
////                foreach (KeyValuePair<string, string> kvp in StringToDict(message))
////                {
////                    await SetAtomLock(kvp.Key, kvp.Value);
////                }
////            }

////            public string GetAllLocksToSend()
////            {
////                return DictToString(SessionController.Instance.Locks);
////            }
////            public async Task<Dictionary<string, object>> GetNodeState(string id)
////            {
////                if (HasSendableID(id))
////                {
////                    return await SessionController.Instance.IdToSendables[id].Pack();
////                }
////                else
////                {
////                    return new Dictionary<string, object>();
////                }
////            }

////            private string DictToString(IEnumerable<KeyValuePair<string, string>> dict)
////            {
////                string s = "";
////                foreach (KeyValuePair<string, string> kvp in dict)
////                {
////                    s += kvp.Key + ":" + kvp.Value + "&";
////                }
////                s = s.Substring(0, Math.Max(s.Length - 1, 0));
////                return s;
////            }

////            private Dictionary<string, string> StringToDict(string s)
////            {
////                Dictionary<string, string> dict = new Dictionary<string, string>();
////                string[] strings = s.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
////                foreach (string kvpString in strings)
////                {
////                    string[] kvpparts = kvpString.Split(new string[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);
////                    if (kvpparts.Length == 2)
////                    {
////                        dict.Add(kvpparts[0], kvpparts[1]);
////                    }
////                }
////                return dict;
////            }

////            private void ParseToLineSegment(Message props, out Point2d one, out Point2d two)
////            {
////                one = new Point2d(Double.Parse(props.GetString("x1")), Double.Parse(props.GetString("y1")));
////                two = new Point2d(Double.Parse(props.GetString("x2")), Double.Parse(props.GetString("y2")));
////            }

////    #endregion oldModelIntermediate

////    #region customExceptions
////    public class InvalidIDException : Exception
////        {
////            public InvalidIDException(string id) : base(String.Format("The ID {0}  was used but is invalid", id)) { }
////        }
////        public class IncorrectFormatException : Exception
////        {
////            public IncorrectFormatException(string message) : base(String.Format("The message '{0}' is incorrectly formatted or unrecognized", message)) { }
////        }

////        public class NotHostException : Exception
////        {
////            public NotHostException(string message, string remoteIP)
////                : base(String.Format("The message {0} was sent to a non-host from IP: {1} when it is a host-only message", message, remoteIP))
////            { }
////        }

////        public class HostException : Exception
////        {
////            public HostException(string message, string remoteIP) : base(String.Format("The message {0} was sent to this machine, THE HOST, from IP: {1} when it is meant for only non-hosts", message, remoteIP)) { }
////        }
////        public class UnknownIPException : Exception
////        {
////            public UnknownIPException(string ip) : base(String.Format("The IP {0} was used when it is not recgonized", ip)) { }
////        }

////        public class NoIDException : Exception
////        {
////            public NoIDException(string message) : base(message) { }
////            public NoIDException() { }
////        }

////        public class InvalidCreationArgumentsException : Exception
////        {
////            public InvalidCreationArgumentsException(string message) : base(message) { }
////            public InvalidCreationArgumentsException() { }
////        }
////        #endregion customExceptions
////    }
////}
