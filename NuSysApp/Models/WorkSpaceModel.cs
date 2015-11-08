using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class WorkSpaceModel : AtomModel
    {
        #region Events and Delegates
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public delegate void CreateEventHandler(object source, CreateEventArgs e);
        public delegate void CreateGroupEventHandler(object source, CreateGroupEventArgs e);
        public delegate void CreatePinEventHandler(object source, CreatePinEventArgs e);
        public delegate void AddPartialLineEventHandler(object source, AddLineEventArgs e);
        public event DeleteEventHandler OnDeletion;
        public event CreateEventHandler OnCreation;
        public event CreatePinEventHandler OnPinCreation;
        public event CreateGroupEventHandler OnGroupCreation;
        //public event AddPartialLineEventHandler OnPartialLineAddition;
        
        #endregion Events and Delegates

        #region Private Members

        //private ObservableDictionary<string,ObservableCollection<InqLineView>> _partialLines;
        private LockDictionary _locks;
        private InqCanvasModel _inqModel;
        #endregion Private members
       

        public WorkSpaceModel(InqCanvasModel inqModel) : base("WORKSPACE_ID")
        {
            this._inqModel = inqModel;
            AtomDict = new Dictionary<string, AtomViewModel>();
            _locks = new LockDictionary(this);
            NetworkConnector.Instance.WorkSpaceModel = this;
        }

        public Dictionary<string, AtomViewModel> AtomDict { set; get; }

        #region Public Members
        public LockDictionary Locks
        {
            get { return _locks; }
            set { _locks = value;}
        }

        #endregion

        public void CreateLink(AtomModel atom1, AtomModel atom2, string id)
        {
            var link = new LinkModel(atom1, atom2, id);
            atom1.AddToLink(link);
            atom2.AddToLink(link);
            Children.Add(id,link);
        }

        public async Task CreateGroup(string id, NodeModel node1, NodeModel node2, double xCooordinate, double yCoordinate)     
        {
            var group = new GroupNodeModel(id)
            {
                X = xCooordinate,
                Y= yCoordinate,
                NodeType = NodeType.Group
            };
            OnGroupCreation?.Invoke(this, new CreateGroupEventArgs("Created new group", group));
            node1.MoveToGroup(group);
            node2.MoveToGroup(group);
            Children.Add(id, group);  
        }

        public async Task CreateEmptyGroup(string id, double xCooordinate, double yCoordinate)
        {
            var group = new GroupNodeModel(id)
            {
                X = xCooordinate,
                Y = yCoordinate,
                NodeType = NodeType.Group
            };
            Children.Add(id, group);
            OnGroupCreation?.Invoke(this, new CreateGroupEventArgs("Created new group", group));
 
        }

        public void AddGlobalInq(InqLineModel lineView)
        {
            //OnPartialLineAddition?.Invoke(this, new AddLineEventArgs("Added Lines", lineView));
            this._inqModel.FinalizeLine(lineView);
        }

        public async Task CreateNewPin(string id, double x, double y)
        {
            var pinModel = new PinModel(id);
            pinModel.X = x;
            pinModel.Y = y;

            Children.Add(id, pinModel);
            OnPinCreation?.Invoke(Children[id], new CreatePinEventArgs("Created", pinModel));

        }
        public async Task CreateNewNode(string id, NodeType type, double xCoordinate, double yCoordinate, object data = null)
        {
            NodeModel node;
            switch (type)
            {
                case NodeType.Text:
                    node = new TextNodeModel((string)data ?? "", id);
                    break;
                case NodeType.Image:
                    node = new ImageNodeModel((byte[])data,id);
                    break;
                case NodeType.PDF:
                    node = new PdfNodeModel((byte[])data, id);
                    await ((PdfNodeModel) node).SaveFile();
                    break;
                case NodeType.Audio:
                    node = new AudioNodeModel((byte[])data, id);
                    break;
                case NodeType.Video:
                    node = new VideoNodeModel((byte[])data, id);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
                    return;
            }
            node.X = xCoordinate;
            node.Y = yCoordinate;
            node.NodeType = type;

            Children.Add(id, node);
            OnCreation?.Invoke(Children[id], new CreateEventArgs("Created", node));
        }

        public async Task RemoveSendable(string id)
        {
            if (Children.ContainsKey(id))
            {
                Children[id].Delete();
                Children.Remove(id);
            }
            else
            {
                throw new InvalidOperationException("Sendable no longer exists");
            }
        }

        public async Task<Dictionary<string, string>> Pack(){return new Dictionary<string, string>();}
        public async Task UnPack(Dictionary<string, string> props){}
        public override void Delete(){}
        public string ID{get { return "WORKSPACE_ID"; }}
        public AtomModel.EditStatus CanEdit {get; set; }

        public InqCanvasModel InqModel
        {
            get { return this._inqModel; }
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
                if (_workSpaceModel.Children.ContainsKey(id))
                {
                    await UITask.Run(() => {
                        if (_workSpaceModel.Children.ContainsKey(id))
                        {
                            if (lockHolder == "")
                            {
                                _workSpaceModel.Children[id].CanEdit = AtomModel.EditStatus.Maybe;
                            }
                            else if (lockHolder == NetworkConnector.Instance.LocalIP)
                            {
                                _workSpaceModel.Children[id].CanEdit = AtomModel.EditStatus.Yes;
                            }
                            else
                            {
                                _workSpaceModel.Children[id].CanEdit = AtomModel.EditStatus.No;
                            }
                        }
                    });
                }
            }

            public void Clear()
            {
                _dict.Clear();
                _locals.Clear();
                foreach (KeyValuePair<string, Sendable> kvp in _workSpaceModel.Children)
                {
                    kvp.Value.CanEdit = AtomModel.EditStatus.Maybe;
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
