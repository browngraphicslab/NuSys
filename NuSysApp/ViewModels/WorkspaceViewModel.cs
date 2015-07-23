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
using NuSysApp.MISC;

namespace NuSysApp
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
            AtomViewList = new ObservableCollection<UserControl>();
            LinkViewList = new ObservableCollection<UserControl>();
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

        }

        public ObservableCollection<UserControl> LinkViewList { get; set; }


        private async void SetupChromeIntermediate()
        {
            var transferFile = await StorageUtil.CreateFileIfNotExists(NuSysStorages.ChromeTransferFolder, Constants.FILE_CHROME_TRANSFER_NAME);
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                var readFile = await FileIO.ReadTextAsync(transferFile);
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var nodeVm = _factory.CreateNewRichText(readFile);
                    this.PositionNode(nodeVm, 100, 100);
                    NodeViewModelList.Add(nodeVm);
                    NodeViewList.Add(nodeVm.View);
                });
            };            
        }

        private async void SetupDirectories()
        {
            NuSysStorages.NuSysTempFolder = await StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FOLDER_NUSYS_TEMP);
            NuSysStorages.ChromeTransferFolder = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FOLDER_CHROME_TRANSFER_NAME);
        }


        private async void OnTransferFolderChange(IStorageQueryResultBase sender, object args)
        {
            Debug.WriteLine("CONTENTS CHANGED! " + args);
            const string transferFolderName = "NuSysTransfer";
            const string transferFileName = "chromeSelections.nusys";
            var docFolder = KnownFolders.DocumentsLibrary;
            var transferFolder = await docFolder.GetFolderAsync(transferFolderName).AsTask();
            var transferFile = await transferFolder.GetFileAsync(transferFileName).AsTask();
            
            var readFile = await FileIO.ReadTextAsync(transferFile);
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;


            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var nodeVm = _factory.CreateNewRichText(readFile);
                this.PositionNode(nodeVm, 100, 100);
                NodeViewModelList.Add(nodeVm);
                AtomViewList.Add(nodeVm.View);

            });

            var options = new QueryOptions { FileTypeFilter = { ".nusys" } };
            var query = transferFolder.CreateFileQueryWithOptions(options);
            query.ContentsChanged += OnTransferFolderChange;
            var files = query.GetFilesAsync();
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
                var line1 = link.LineRepresentation;
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
            //1. Remove all the node's links
            var toDelete = new List<LinkViewModel>();
            foreach (var linkVm in nodeVM.LinkList)
            {
                AtomViewList.Remove(linkVm.View);
                toDelete.Add(linkVm);
            }
          
            foreach (var linkVm in toDelete)  //second loop avoids concurrent modification error
            {
                linkVm.DeleteLink();
            }
            
            //2. Remove the node itself 
            AtomViewList.Remove(nodeVM.View);
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
            if (CurrentMode != Mode.TEXTNODE && CurrentMode != Mode.INK) return;
            var vm = new LinkViewModel(nodeVM1, nodeVM2, this);

            AtomViewList.Add(vm.View);
            nodeVM1.AddLink(vm);
            nodeVM2.AddLink(vm);
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
            AtomViewList.Add(vm.View);
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

        public ObservableCollection<UserControl> AtomViewList { get; }

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