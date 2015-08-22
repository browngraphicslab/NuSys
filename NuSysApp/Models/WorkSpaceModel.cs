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
        #region Events and Delegates
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public delegate void CreateEventHandler(object source, CreateEventArgs e);
        public event DeleteEventHandler OnDeletion;
        public event CreateEventHandler OnCreation;
        #endregion Events and Delegates

        #region Private Members
        private Dictionary<string, Atom> _idDict;
        private Dictionary<string, string> _locks;
        private HashSet<string> _locksHeld; 
        private int _currentId;
        #endregion Private members

        #region Public Members
        public Dictionary<string, Atom> IDToAtomDict
        {
            get { return _idDict; }
        }
        public Dictionary<string, string> Locks
        {
            get { return _locks; }
        }

        public HashSet<string> LocalLocks
        {
            get
            {
                return _locksHeld;
            }
        }

        public Dictionary<string, AtomViewModel> AtomDict { set; get; }
        #endregion

        public WorkSpaceModel()
        {
            _idDict = new Dictionary<string, Atom>();
            AtomDict = new Dictionary<string, AtomViewModel>();
            _currentId = 0;
            _locks = new Dictionary<string, string>();
            _locksHeld = new HashSet<string>();
            NetworkConnector.Instance.ModelIntermediate = new ModelIntermediate(this);
        }

        public void CreateLink(Atom atom1, Atom atom2, string id)
        {
            var link = new Link(atom1, atom2, id);
            atom1.AddToLink(link);
            atom2.AddToLink(link);
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
                    node = new InkModel(id);
                    break;
                default:
                    Debug.WriteLine("Could not create node");
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
                ((Node)_idDict[id]).Delete();
                _idDict.Remove(id);
            }
        }
  
    }

}
