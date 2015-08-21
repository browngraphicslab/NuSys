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

namespace NuSysApp
{
    public class WorkSpaceModel
    {
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);

        public event DeleteEventHandler OnDeletion;
        //Node _selectedNode;
        private Dictionary<string, Atom> _idDict;
        private Dictionary<string, string> _locks;
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
            _locks = new Dictionary<string, string>();
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
        public Dictionary<string, string> Locks
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
    }

}
