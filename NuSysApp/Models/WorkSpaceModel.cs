using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class WorkSpaceModel
    {
        #region Events and Delegates
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public delegate void CreateEventHandler(object source, CreateEventArgs e);
        public delegate void CreateGroupEventHandler(object source, CreateGroupEventArgs e);
        public delegate void AddPartialLineEventHandler(object source, AddPartialLineEventArgs e);
        public event DeleteEventHandler OnDeletion;
        public event CreateEventHandler OnCreation;
        public event CreateGroupEventHandler OnGroupCreation;
        public event AddPartialLineEventHandler OnPartialLineAddition;
        
        #endregion Events and Delegates

        #region Private Members
        private Dictionary<string, Sendable> _idDict;

        private ObservableDictionary<string,ObservableCollection<Line>> _partialLines;
        private LockDictionary _locks;
        #endregion Private members
       

        public WorkSpaceModel()
        {
            _idDict = new Dictionary<string, Sendable>();
            AtomDict = new Dictionary<string, AtomViewModel>();
            _locks = new LockDictionary(this);
            _partialLines = new ObservableDictionary<string, ObservableCollection<Line>>();
            _partialLines.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args)
            {
                foreach (ObservableCollection<Line> n in ((object[])args.NewItems.SyncRoot))
                {
                    n.CollectionChanged += delegate(object o, NotifyCollectionChangedEventArgs eventArgs)
                    {
                        OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Partial Lines", ((Line)((object[])eventArgs.NewItems.SyncRoot)[0])));
                    };
                }
            };
            NetworkConnector.Instance.ModelIntermediate = new ModelIntermediate(this);
        }

        public Dictionary<string, AtomViewModel> AtomDict { set; get; }

        #region Public Members
        public Dictionary<string, Sendable> IDToSendableDict
        {
            get { return _idDict; }
        }
        public ObservableDictionary<string, ObservableCollection<Line>> PartialLines 
        {
            get { return _partialLines; }
        }
        public LockDictionary Locks
        {
            get { return _locks; }
            set { _locks = value;}
        }

        #endregion


        public void CreateLink(Atom atom1, Atom atom2, string id)
        {
            var link = new Link(atom1, atom2, id);
            atom1.AddToLink(link);
            atom2.AddToLink(link);
            _idDict.Add(id,link);
        }


        public async Task CreateGroup(string id, Node node1, Node node2, double xCooordinate, double yCoordinate)     
        {
            var group = new Group(id)
            {
                X = xCooordinate,
                Y= yCoordinate,
                NodeType = NodeType.Group
            };
            _idDict.Add(id, group);
             node1.AddToGroup(group);
             node2.AddToGroup(group);
            OnGroupCreation?.Invoke(this, new CreateGroupEventArgs("Created new group", group));
        }

        public async Task CreateNewNode(string id, NodeType type, double xCoordinate, double yCoordinate, object data = null)
        {
            Node node;
            switch (type)
            {
                case NodeType.Text:
                    node = new TextNode((string)data, id);
                    break;
                case NodeType.Richtext:
                    node = new TextNode((string)data, id);
                    break;
                case NodeType.Ink:
                    var lines = data as List<InqLine>;
                    if (lines != null)
                    {
                        node = new InkModel(id, lines);
                    }
                    else
                    {
                        node = new InkModel(id);
                    }
                    break;
                case NodeType.Image:
                    node = new ImageModel((byte[])data,id);
                    break;
                case NodeType.PDF:
                    node = new PdfNodeModel((byte[])data, id);
                    await ((PdfNodeModel) node).SaveFile();
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
                    return;
            }
            node.X = xCoordinate;
            node.Y = yCoordinate;
            node.NodeType = type;

            _idDict.Add(id, node);
            OnCreation?.Invoke(_idDict[id], new CreateEventArgs("Created", node));
        }

        public async Task RemoveNode(string id)
        {
            if (_idDict.ContainsKey(id))
            {
                ((Node) _idDict[id]).Delete();
                _idDict.Remove(id);
            }
            else
            {
                throw new InvalidOperationException("Node no longer exists");
            }
        }


        public class LockDictionary : IEnumerable<KeyValuePair<string,string>>
        {
            private HashSet<string> _locals = new HashSet<string>();
            private Dictionary<string,string> _dict = new Dictionary<string, string>();
            private WorkSpaceModel _workSpaceModel;
            public LockDictionary(WorkSpaceModel wsm)
            {
                _workSpaceModel = wsm;
            }

            public HashSet<string> LocalLocks
            {
                get { return _locals; } 
            } 
            public string Value(string key)
            {
                if (_dict.ContainsKey(key))
                {
                    return _dict[key];
                }
                return null;
            }
            public async Task Set(string k, string v)
            {
                if (v == NetworkConnector.Instance.LocalIP)
                {
                    _locals.Add(k);
                }
                else
                {
                    _locals.Remove(k);
                }
                if (!_dict.ContainsKey(k))
                {
                    _dict.Add(k, v);
                }
                else
                {
                    _dict[k] = v;
                }
                await UpdateAtomLock(k,v);
            }

            private async Task UpdateAtomLock(string id, string lockHolder)
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (_workSpaceModel.IDToSendableDict.ContainsKey(id))
                    {
                        if (lockHolder == "")
                        {
                            _workSpaceModel.IDToSendableDict[id].CanEdit = Atom.EditStatus.Maybe;
                        }
                        else if (lockHolder == NetworkConnector.Instance.LocalIP)
                        {
                            _workSpaceModel.IDToSendableDict[id].CanEdit = Atom.EditStatus.Yes;
                        }
                        else
                        {
                            _workSpaceModel.IDToSendableDict[id].CanEdit = Atom.EditStatus.No;
                        }
                    }
                });
            }

            public void Clear()
            {
                _dict.Clear();
                _locals.Clear();
                foreach (KeyValuePair<string, Sendable> kvp in _workSpaceModel.IDToSendableDict)
                {
                    kvp.Value.CanEdit = Atom.EditStatus.Maybe;
                }
            }

            public bool ContainsID(string id)
            {
                return _dict.ContainsKey(id);
            }

            public bool ContainsHolder(string holder)
            {
                return _dict.ContainsValue(holder);
            }
            public bool RemoveID(string k)
            {
                if (_dict.ContainsKey(k))
                {
                    _dict.Remove(k);
                    if (_locals.Contains(k))
                    {
                        _locals.Remove(k);
                    }
                    return true;
                }
                return false;
            }

            public IEnumerator<KeyValuePair<string,string>> GetEnumerator()
            {
                return _dict.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dict.GetEnumerator();
            }
        }
    }

}
