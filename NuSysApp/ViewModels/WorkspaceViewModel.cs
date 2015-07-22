using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuStarterProject
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class WorkspaceViewModel : BaseINPC
    {
        #region Private Members

        private readonly Factory _factory;

        public enum Mode
        {
            TEXTNODE,
            GLOBALINK,
            INK,
            ERASE
        }; //enum created to switch between multiple modes in the appbar

        public enum LinkMode
        {
            LINELINK,
            BEZIERLINK
        }

        private double _transformX, _transformY, _scaleX, _scaleY;

        #endregion Private Members

        public WorkspaceViewModel()
        {
            NodeViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            SelectedNodeViewModel = null;
            this.CurrentMode = Mode.TEXTNODE;
            this.CurrentLinkMode = LinkMode.BEZIERLINK;
            TransformX = 0;
            TransformY = 0;
            ScaleX = 0;
            ScaleY = 0;
            _factory = new Factory(this);
            SetupChromeIntermediate();

        }

        private async void SetupChromeIntermediate()
        {
            StorageFolder transferFolder = null;
            StorageFile transferFile = null;
            var docFolder = KnownFolders.DocumentsLibrary;
            const string transferFolderName = "NuSysTransfer";
            const string transferFileName = "chromeSelections.nusys";

            // Create transfer folder if not exists.
            try
            {
                transferFolder = await docFolder.GetFolderAsync(transferFolderName).AsTask();
            }
            catch (Exception exception)
            {
                transferFolder = await docFolder.CreateFolderAsync(transferFolderName).AsTask();
            }

            // Create transfer file if not exists.
            try
            {

                transferFile = await transferFolder.GetFileAsync(transferFileName).AsTask();
            }
            catch (Exception exception)
            {
                transferFile = await transferFolder.CreateFileAsync(transferFileName).AsTask();
            }

            // Start watching 
            var options = new QueryOptions {FileTypeFilter = {".nusys"}};
            var query = transferFolder.CreateFileQueryWithOptions(options);
            query.ContentsChanged += delegate(IStorageQueryResultBase sender, object args)
            {
                Debug.WriteLine("CONTENTS CHANGED! " + args);
                //file = transferFolder.GetFileAsync(transferFileName).GetResults();
                //ReadFile(query.Folder.GetFileAsync().GetResults());
                ReadFile(transferFile);
            };

            query.GetFilesAsync();
        }

        private string[] text;
        public async void ReadFile(StorageFile file)
        {
            text = new string[100];
            var readFile = await Windows.Storage.FileIO.ReadLinesAsync(file);
            
            int counter = 0;
            foreach (var line in readFile)
            {
                text[counter] = line;
                counter++;
            }

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                int i = 0;
                while (i < counter)
                {
                    var nodeVm = _factory.CreateNewRichText(text[i]);
                    this.PositionNode(nodeVm, 100 + counter * 100, 100 + counter * 100);
                    i++;
                    NodeViewModelList.Add(nodeVm);
                    NodeViewList.Add(nodeVm.View);
                }

            });
            
            
           
        }

        /// <summary>
        /// Returns true if the given node intersects with any link on the workspace, 
        /// using a simple line approximation for Bezier curves.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CheckForNodeLinkIntersections(NodeViewModel node)
        {
            var lines = NodeToLineSegmentHelper(node);
            foreach (var link in LinkViewModelList)
            {
                var line1 = link.Line;
                foreach (var line2 in lines)
                {
                    if (Geometry.LinesIntersect(line1, line2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Line[] NodeToLineSegmentHelper(NodeViewModel node)
        {
            var lines = new Line[4];
            var x = node.X + node.Transform.Matrix.OffsetX;
            var y = node.Y + node.Transform.Matrix.OffsetY;

            //AB line  
            lines[0] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x + node.Width,
                Y2 = y
            };

            //CD line 
            lines[1] = new Line
            {
                X1 = x,
                Y1 = y + node.Height,
                X2 = x + node.Width,
                Y2 = y + node.Height
            };

            //AC line 
            lines[2] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y + node.Height
            };

            //BC line 
            lines[3] = new Line
            {
                X1 = x + node.Width,
                Y1 = y,
                X2 = x + node.Width,
                Y2 = y + node.Height
            };

            return lines;
        }

        /// <summary>
        /// Deletes a given node from the workspace, and their links.
        /// </summary> 
        /// <param name="nodeVM"></param>
        public void DeleteNode(NodeViewModel nodeVM)
        {
            var linkList = nodeVM.GetLinkList();
            if (linkList != null)
            {
                foreach (var link in linkList)
                {
                    NodeViewList.Remove(link);
                }
            }

            var lineList = nodeVM.GetLineList();
            if (lineList != null)
            {
                foreach (var link in lineList)
                {
                    NodeViewList.Remove(link);
                }
            }

            NodeViewList.Remove(nodeVM.View);
            NodeViewModelList.Remove(nodeVM);
        }

        /// <summary>
        /// Sets the passed in Node as selected. If there atlready is a selected node, the two are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(NodeViewModel selected)
        {
            if (SelectedNodeViewModel == null)
            {
                SelectedNodeViewModel = selected;
                return;
            }
            this.CreateNewLink(SelectedNodeViewModel, selected);
            selected.IsSelected = false;
            SelectedNodeViewModel.IsSelected = false;
            SelectedNodeViewModel = null;
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            if (SelectedNodeViewModel == null) return;
            SelectedNodeViewModel.ToggleSelection();
            SelectedNodeViewModel = null;
            return;
        }

        /// <summary>
        /// Creates a link between two nodes. 
        /// </summary>
        /// <param name="nodeVM1"></param>
        /// <param name="nodeVM2"></param>
        public void CreateNewLink(NodeViewModel nodeVM1, NodeViewModel nodeVM2)
        {
            var x1 = (int) (nodeVM1.X + nodeVM1.Transform.Matrix.OffsetX);
            var y1 = (int) (nodeVM1.Y + nodeVM1.Transform.Matrix.OffsetY);
            var x2 = (int) (nodeVM2.X + nodeVM2.Transform.Matrix.OffsetX);
            var y2 = (int) (nodeVM2.Y + nodeVM2.Transform.Matrix.OffsetY);

            if (CurrentMode != Mode.TEXTNODE && CurrentMode != Mode.INK) return;
            var vm = new LinkViewModel(x1, x2, y1, y2, nodeVM1, nodeVM2, this);

            NodeViewList.Add(vm.View);
            nodeVM1.AddLink(vm.View);
            nodeVM2.AddLink(vm.View);
            LinkViewModelList.Add(vm);
        }

        public void CreateNewNode(double xCoordinate, double yCoordinate)
        {
            NodeViewModel vm;
            switch (this.CurrentMode)
            {
                case Mode.TEXTNODE:
                    vm = _factory.CreateNewText("Enter text here");
                    break;
                case Mode.INK:
                    vm = _factory.CreateNewInk();
                    break;
                default:
                    return;
            }
            NodeViewModelList.Add(vm);
            NodeViewList.Add(vm.View);
            this.PositionNode(vm, xCoordinate, yCoordinate);
        }

        private void PositionNode(NodeViewModel vm, double xCoordinate, double yCoordinate)
        {
            vm.X = 0;
            vm.Y = 0;
            var transMat = ((MatrixTransform) vm.View.RenderTransform).Matrix;
            transMat.OffsetX += xCoordinate + TransformX;
            transMat.OffsetY += yCoordinate + TransformY;
            vm.Transform = new MatrixTransform {Matrix = transMat};
        }

        #region Public Members

        public ObservableCollection<NodeViewModel> NodeViewModelList { get; }

        public ObservableCollection<LinkViewModel> LinkViewModelList { get; }

        public ObservableCollection<UserControl> NodeViewList { get; }

        public NodeViewModel SelectedNodeViewModel { get; private set; }

        public Mode CurrentMode { get; set; }

        public LinkMode CurrentLinkMode { get; set; }

        public double TransformX
        {
            get { return _transformX; }
            set
            {
                if (_transformX == value)
                {
                    return;
                }
                _transformX = value;
                RaisePropertyChanged("TransformX");
            }
        }

        public double TransformY
        {
            get { return _transformY; }

            set
            {
                if (_transformY == value)
                {
                    return;
                }
                _transformY = value;
                RaisePropertyChanged("TransformY");
            }
        }

        public double ScaleX
        {
            get { return _scaleX; }
            set
            {
                if (_scaleX == value)
                {
                    return;
                }
                _scaleX = value;
            }
        }

        public double ScaleY
        {
            get { return _scaleY; }
            set
            {
                if (_scaleY == value)
                {
                    return;
                }
                _scaleY = value;
            }
        }

        #endregion Public Members
    }
}