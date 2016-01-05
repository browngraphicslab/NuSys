using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class SessionController
    {
        private static readonly object _syncRoot = new Object();
        private static SessionController _instance = new SessionController();
        private WorkspaceViewModel _activeWorkspace;

        public delegate void WorkspaceChangedHandler(object source, WorkspaceViewModel workspace);
        public event WorkspaceChangedHandler WorkspaceChanged;

        private LockDictionary _locks;

        private ContentController _contentController = new ContentController();
        public ObservableDictionary<string, Sendable> IdToSendables { set; get; }

        public SessionView SessionView { get; set; }
        public ContentController ContentController { get { return _contentController; } }


        public WorkspaceViewModel ActiveWorkspace
        {
            get { return _activeWorkspace; }
            set
            {
                _activeWorkspace = value;
                WorkspaceChanged?.Invoke(this, _activeWorkspace);
            }
        }

        public LockDictionary Locks
        {
            get { return _locks; }
            set { _locks = value; }
        }

        public SessionController()
        {
            _locks = new LockDictionary(this);
            IdToSendables = new ObservableDictionary<string, Sendable>();
        }

        public UserControl GetUserControlById(string id)
        {
            var model = IdToSendables[id];
            foreach (var userControl in ActiveWorkspace.Children.Values)
            {
                var vm = (AtomViewModel) userControl.DataContext;
                if (vm.Model == model)
                    return userControl;
            }
            return null;
        }

        public void CreateLink(AtomModel atom1, AtomModel atom2, string id)
        {
            var link = new LinkModel(atom1, atom2, id);
            IdToSendables.Add(id, link);
        }

        public async Task CreateGroup(string id, NodeModel node1, NodeModel node2, double xCooordinate, double yCoordinate)
        {
            var group = new NodeContainerModel(id)
            {
                X = xCooordinate,
                Y = yCoordinate,
                NodeType = NodeType.Group
            };

            node1.Creator = group.Id;
            node2.Creator = group.Id;

            group.AddChild(node1);
            group.AddChild(node2);

            

           // node1.MoveToGroup(group);
//node2.MoveToGroup(group);
            IdToSendables.Add(id, group);

        }

        public async Task CreateGroupTag(string id, double xCooordinate, double yCoordinate, double width, double height, string title)
        {
            var group = new NodeContainerModel(id)
            {
                X = xCooordinate,
                Y = yCoordinate,
                Width = width,
                Height = height,
                NodeType = NodeType.GroupTag,
                Title = title
            };
            IdToSendables.Add(id, group);

            var searchResults = SessionController.Instance.IdToSendables.Values.Where(m =>
            {
                var mm = m as AtomModel;
                if (mm == null || mm == group)
                    return false;
                var tags = (List<string>) mm.GetMetaData("tags");
                
                return mm.GetMetaData("visualCopyOf") == "" && tags.Contains(group.Title);
            });

            foreach (var searchResult in searchResults.ToList())
            {
                var callback = new Action<string>((s) =>
                {
                    UITask.Run(() =>
                    {
                        var newNodeModel = (NodeModel)SessionController.Instance.IdToSendables[s];
                        newNodeModel.SetMetaData("visualCopyOf", searchResult.Id);
                     //   newNodeModel.MoveToGroup(group, true);
                    });
                });

                var dict = await searchResult.Pack();
                var props = dict;
                props.Remove("id");
                props.Remove("type");
                props.Remove("nodeType");
                props.Remove("x");
                props.Remove("y");
                NetworkConnector.Instance.RequestMakeNode(group.X.ToString(), group.Y.ToString(), ((NodeModel)searchResult).NodeType.ToString(), null, null, props, callback);
            }

            //   ActiveWorkspace.Model.AddChild(group);
        }

        public void AddGlobalInq(InqLineModel lineView)
        {
            //PartialLineAdded?.Invoke(this, new AddPartialLineEventArgs("Added Lines", lineView));
            // TODO: readd line below
            var wvm = (WorkspaceModel)Instance.ActiveWorkspace.Model;
            var cm = (InqCanvasModel) wvm.InqCanvas;
            cm.FinalizeLine(lineView);
        }

        public async Task CreateNewPin(string id, double x, double y)
        {
            var pinModel = new PinModel(id);
            pinModel.X = x;
            pinModel.Y = y;

            IdToSendables.Add(id, pinModel);

            (ActiveWorkspace.Model as WorkspaceModel).AddChild(pinModel);

        }

        public async Task CreateNewNode(string id, NodeType type)
        {
            NodeModel node;
            NodeViewModel nodeViewModel;
            switch (type)
            {
                case NodeType.Text:
                    node = new TextNodeModel(id);
                    break;
                case NodeType.Image:
                    node = new ImageNodeModel(id);
                    break;
                case NodeType.PDF:
                    node = new PdfNodeModel(id);
                    break;
                case NodeType.Audio:
                    node = new AudioNodeModel(id);
                    break;
                case NodeType.Video:
                    node = new VideoNodeModel(id);
                    break;
                case NodeType.GroupTag:
                    node = new NodeContainerModel(id);
                    break;
                case NodeType.Web:
                    node = new WebNodeModel(id);
                    break;
                case NodeType.Workspace:
                    node = new WorkspaceModel(id);
                    break;
                case NodeType.Group:
                    node = null;
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
                    return;
            }
            if (node == null)
                return;

            // TODO: bullshit fix
            if (!IdToSendables.ContainsKey(id))
                IdToSendables.Add(id, node);
        }

        public async Task RemoveSendable(string id)
        {
            if (IdToSendables.ContainsKey(id))
            {
                //NodeDeleted?.Invoke(this, new DeleteEventArgs("node deleted", IdToSendables[id)));
                
                (ActiveWorkspace.Model as WorkspaceModel).RemoveChild(IdToSendables[id]);
                IdToSendables.Remove(id);
            }
            else
            {
                throw new InvalidOperationException("Sendable no longer exists");
            }
        }


        public async Task SaveWorkspace()
        {
            await _contentController.Save();

            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lineTasks = IdToSendables.Values.Select(async s => await s.Stringify());
            var lines = await Task.WhenAll(lineTasks);
            FileIO.WriteLinesAsync(file, lines);
        }

        public async Task LoadWorkspace()
        {
            await _contentController.Load();

            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lines = await FileIO.ReadLinesAsync(file);
;           SessionView.LoadWorksapce(lines);
        }

        public string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
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
