using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class SessionController
    {
        private static readonly object _syncRoot = new Object();
        private static SessionController _instance = new SessionController();
        private WorkspaceViewModel _activeWorkspace;

        public delegate void WorkspaceChangedHandler(object source, WorkspaceViewModel workspace);
        public event WorkspaceChangedHandler WorkspaceChanged;

        public NuSysNetworkSession NuSysNetworkSession
        {
            get { return _nuSysNetworkSession; }
        }

        private LockDictionary _locks;

        private ContentController _contentController = new ContentController();
        public ObservableDictionary<string, Sendable> IdToSendables { set; get; }

        public SessionView SessionView { get; set; }
        public ContentController ContentController { get { return _contentController; } }

        public Dictionary<string, ImageSource> Thumbnails = new Dictionary<string, ImageSource>(); 

        private NuSysNetworkSession _nuSysNetworkSession;

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

        private SessionController()
        {
            _locks = new LockDictionary(this);
            IdToSendables = new ObservableDictionary<string, Sendable>();
            _nuSysNetworkSession = new NuSysNetworkSession();
        }

        public FrameworkElement GetUserControlById(string id)
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
            IdToSendables.Add(id, group);

            node1.Creators.Add(group.Id);
            var prevGroups1 = (List<string>)node1.GetMetaData("groups");
            prevGroups1.Add(group.Id);
            node1.SetMetaData("groups", prevGroups1);

            
            node2.Creators.Add(group.Id);
            var prevGroups2 = (List<string>)node2.GetMetaData("groups");
            prevGroups2.Add(group.Id);
            node2.SetMetaData("groups", prevGroups2);
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
                //NetworkConnector.Instance.RequestMakeNode(group.X.ToString(), group.Y.ToString(), ((NodeModel)searchResult).NodeType.ToString(), null, null, props, callback);
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

        public async Task<NodeModel> CreateNewNode(string id, NodeType type)
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
                    node = new NodeContainerModel(id);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
                    return null;
            }
            if (node == null)
                return null;

            // TODO: bullshit fix
            if (!IdToSendables.ContainsKey(id))
                IdToSendables.Add(id, node);
            return node;
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

        private async Task LoadThumbs()
        {

            var thumbs = await NuSysStorages.Thumbs.GetFilesAsync();
            foreach (var thumbFile in thumbs)
            {
                var buffer = await FileIO.ReadBufferAsync(thumbFile);
                var id = Path.GetFileNameWithoutExtension(thumbFile.Path);
                var img = await ImageUtil.ByteArrayToBitmapImage(buffer.ToArray());
                Thumbnails[id] = img;
            }           
        }

        public async Task SaveThumb(string id, RenderTargetBitmap image)
        {
            Thumbnails[id] = image;
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.Thumbs, id + ".png");
            var img = await ImageUtil.RenderTargetBitmapToByteArray(image);
            FileIO.WriteBytesAsync(file, img);
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
            await LoadThumbs();
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
