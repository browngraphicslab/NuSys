using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Network;
using System.Collections;
using System.Linq;

namespace NuSysApp
{
    public class WorkSpaceModel
    {
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);

        public event DeleteEventHandler OnDeletion;
        //Node _selectedNode;
        private Dictionary<string, Atom> _idDict;
        private LockDictionary _locks;
        private WorkspaceViewModel _workspaceViewModel;
        private ModelIntermediate _modelIntermediate;
        private int _currentId;
        //private Factory _factory;
        public WorkSpaceModel(WorkspaceViewModel vm)
        {
            _idDict = new Dictionary<string, Atom>();
            _workspaceViewModel = vm;
            AtomDict = new Dictionary<string, AtomViewModel>();
            _currentId = 0;
            _locks = new LockDictionary(this);
            _modelIntermediate = new ModelIntermediate(this);
            NetworkConnector.Instance.ModelIntermediate = _modelIntermediate;
            // _factory = new Factory(this);
        }

        public Dictionary<string, AtomViewModel> AtomDict { set; get; }

        public void CreateNewTextNode(string data)
        {
            //_nodeDict.Add(CurrentID, _factory.createNewTextNode(data));
            //CurrentID++;
        }

        public Dictionary<string, Atom> IDToAtomDict
        {
            get { return _idDict; }
        } 
        public LockDictionary Locks
        {
            get { return _locks; }
            set { _locks = value;}
        }

        public async Task<Atom> CreateNewNode(string id, NodeType type, double xCoordinate, double yCoordinate, object data = null)
        {
            Atom atom = await _workspaceViewModel.CreateNewNode(id, type, xCoordinate, yCoordinate, data); 
            _idDict.Add(id,atom);
            return atom;
        }

        public async Task CreateGroup(string id, Node node1, Node node2)
        {
            //TODO make groups work here
        }

        public async Task RemoveNode(string id)
        {
            if (_idDict.ContainsKey(id))
            {
                OnDeletion?.Invoke(_idDict[id], new DeleteEventArgs("Deleted"));
                _idDict.Remove(id);
            }
        }

        public class DeleteEventArgs : EventArgs
        {
            private string EventInfo;

            public DeleteEventArgs(string text)
            {
                EventInfo = text;
            }

            public string GetInfo()
            {
                return EventInfo;
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
            public void Set(string k, string v)
            {
                if (v == NetworkConnector.Instance.LocalIP)
                {
                    _locals.Add(k);
                }
                if (!_dict.ContainsKey(k))
                {
                    _dict.Add(k, v);
                }
                else
                {
                    _dict[k] = v;
                }
                UpdateAtomLock(k,v);
            }

            private void UpdateAtomLock(string id, string lockHolder)
            {
                if (_workSpaceModel.IDToAtomDict.ContainsKey(id))
                {
                    if(lockHolder == "")
                    {
                        _workSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.Maybe;
                    }
                    else if (lockHolder == NetworkConnector.Instance.LocalIP)
                    {
                        _workSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.Yes;
                    }
                    else
                    {
                        _workSpaceModel.IDToAtomDict[id].CanEdit = Atom.EditStatus.No;
                    }
                }
            }

            public void Clear()
            {
                _dict.Clear();
                _locals.Clear();
                foreach (KeyValuePair<string, Atom> kvp in _workSpaceModel.IDToAtomDict)
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
