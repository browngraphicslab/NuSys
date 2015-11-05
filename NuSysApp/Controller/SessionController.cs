using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SessionController
    {
        private static readonly object _syncRoot = new Object();
        private static SessionController _instance = new SessionController();
    
        private LockDictionary _locks;
        public Dictionary<string, Sendable> IdToSendables { set; get; }

        public WorkspaceViewModel ActiveWorkspace { get; set; }

        public LockDictionary Locks
        {
            get { return _locks; }
            set { _locks = value; }
        }

        public SessionController()
        {
            _locks = new LockDictionary(this);
            IdToSendables = new Dictionary<string, Sendable>();
        }

        public void CreateLink(AtomModel atom1, AtomModel atom2, string id)
        {
            var link = new LinkModel(atom1, atom2, id);
            IdToSendables.Add(id, link);
            ActiveWorkspace.Model.AddChild(link);
        }

        public async Task CreateGroup(string id, NodeModel node1, NodeModel node2, double xCooordinate, double yCoordinate)
        {
            var group = new GroupNodeModel(id)
            {
                X = xCooordinate,
                Y = yCoordinate,
                NodeType = NodeType.Group
            };

            node1.MoveToGroup(group);
            node2.MoveToGroup(group);
            IdToSendables.Add(id, group);
           // OnGroupCreation?.Invoke(this, new CreateGroupEventArgs("Created new group", group));
        }

        public async Task CreateEmptyGroup(string id, double xCooordinate, double yCoordinate)
        {
            var group = new GroupNodeModel(id)
            {
                X = xCooordinate,
                Y = yCoordinate,
                NodeType = NodeType.Group
            };
            IdToSendables.Add(id, group);
      //      OnGroupCreation?.Invoke(this, new CreateGroupEventArgs("Created new group", group));

        }

        public void AddGlobalInq(InqLineModel lineView)
        {
            //OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Lines", lineView));
            // TODO: readd line below
            //this._inqModel.FinalizeLine(lineView);
        }

        public async Task CreateNewPin(string id, double x, double y)
        {
            var pinModel = new PinModel(id);
            pinModel.X = x;
            pinModel.Y = y;

            IdToSendables.Add(id, pinModel);

            ActiveWorkspace.Model.AddChild(pinModel);

        }

        public async Task CreateNewNode(string id, NodeType type, double xCoordinate, double yCoordinate, object data = null)
        {
            NodeModel node;
            NodeViewModel nodeViewModel;
            switch (type)
            {
                case NodeType.Text:
                    node = new TextNodeModel((string)data ?? "", id);
                    break;
                case NodeType.Image:
                    node = new ImageNodeModel((byte[])data, id);
                    break;
                case NodeType.PDF:
                    node = new PdfNodeModel((byte[])data, id);
                    await ((PdfNodeModel)node).SaveFile();
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
            IdToSendables.Add(id, node);
            
            ActiveWorkspace.Model.AddChild(node);
        }

        public async Task RemoveSendable(string id)
        {
            if (IdToSendables.ContainsKey(id))
            {
                //NodeDeleted?.Invoke(this, new DeleteEventArgs("node deleted", IdToSendables[id)));
                IdToSendables.Remove(id);
            }
            else
            {
                throw new InvalidOperationException("Sendable no longer exists");
            }
        }
        
        public static SessionController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SessionController();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
