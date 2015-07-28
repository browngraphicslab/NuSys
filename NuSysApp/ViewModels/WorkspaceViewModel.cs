using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
            ERASE,
            IMAGE,
            PDF
        }; //enum created to switch between multiple modes in the appbar

        public enum LinkMode
        {
            LINELINK,
            BEZIERLINK
        }

        private CompositeTransform _compositeTransform, _fMTransform;
        

        #endregion Private Members

        public WorkspaceViewModel()
        {
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            SelectedAtomViewModel = null;
            this.CurrentMode = Mode.TEXTNODE;
            this.CurrentLinkMode = LinkMode.BEZIERLINK;
            //_factory = new Factory(this);


            Init();
            var c = new CompositeTransform();
            c.TranslateX = (-1)* (Constants.MAX_CANVAS_SIZE);
            c.TranslateY = (-1) * (Constants.MAX_CANVAS_SIZE);
            CompositeTransform = c;
            FMTransform = new CompositeTransform();
        }


        private async void Init()
        {
            var result = await SetupDirectories();
            SetupChromeIntermediate();
        }

        private async void SetupChromeIntermediate()
        {
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                Debug.WriteLine("CONTENTS CHANGED! ");
                var transferFiles = await NuSysStorages.ChromeTransferFolder.GetFilesAsync().AsTask();

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                int count = 0;
                foreach (var file in transferFiles)
                {
                    Debug.WriteLine(file.Path);

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var readFile = await FileIO.ReadTextAsync(file);
                        //var nodeVm = _factory.CreateNewRichText(readFile);
                        var nodeVm = Factory.CreateNewRichText(readFile);
                        var p = CompositeTransform.Inverse.TransformPoint(new Point((count++) * 250, 200));
                        PositionNode(nodeVm, p.X, p.Y);
                        NodeViewModelList.Add(nodeVm);
                        AtomViewList.Add(nodeVm.View);
                    });
                }


                foreach (var file in transferFiles)
                {
                    await file.DeleteAsync();
                }
            };
        }

        private async Task<bool> SetupDirectories()
        {
            NuSysStorages.NuSysTempFolder = await StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FOLDER_NUSYS_TEMP);
            NuSysStorages.ChromeTransferFolder = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FOLDER_CHROME_TRANSFER_NAME);
            return true;
        }

        /// <summary>
        /// Returns true if the given node intersects with any link on the workspace, 
        /// using a simple line approximation for Bezier curves.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CheckForNodeLinkIntersections(NodeViewModel node)
        {
            var lines = Geometry.NodeToLineSegment(node);
            foreach (var link in LinkViewModelList)
            {
                var line1 = link.LineRepresentation;
                foreach (var line2 in lines)
                {
                    if (Geometry.LinesIntersect(line1, line2) && link.Atom1 != node && link.Atom2 != node)
                    {
                        node.ClippedParent = link;
                        link.Annotation = node;
                        return true;
                    }
                }
            }
            return false;
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
                linkVm.Remove();
                nodeVM.LinkList.Remove(linkVm);
            }

            //2. Remove the node itself 
            AtomViewList.Remove(nodeVM.View);
            NodeViewModelList.Remove(nodeVM);
        }

        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(AtomViewModel selected)
        {
            if (SelectedAtomViewModel == null)
            {
                SelectedAtomViewModel = selected;
                return;
            }
            this.CreateNewLink(SelectedAtomViewModel, selected);
            selected.IsSelected = false;
            SelectedAtomViewModel.IsSelected = false;
            SelectedAtomViewModel = null;
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            if (SelectedAtomViewModel == null) return;
            SelectedAtomViewModel.IsSelected = false;
            SelectedAtomViewModel = null;
        }

        /// <summary>
        /// Creates a link between two nodes. 
        /// </summary>
        /// <param name="atomVM1"></param>
        /// <param name="atomVM2"></param>
        public void CreateNewLink(AtomViewModel atomVm1, AtomViewModel atomVm2)
        {
            if (CurrentMode != Mode.TEXTNODE && CurrentMode != Mode.INK) return;
            if (atomVm1.IsAnnotation || atomVm2.IsAnnotation) return;

            var vm = new LinkViewModel(atomVm1, atomVm2, this);

            LinkViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            atomVm1.AddLink(vm);
            atomVm2.AddLink(vm);
        }

        public async Task CreateNewNode(double xCoordinate, double yCoordinate, object data = null)
        {
            NodeViewModel vm;
            switch (this.CurrentMode)
            {
                case Mode.TEXTNODE:
                    vm = Factory.CreateNewText("Enter text here");
                    break;
                case Mode.INK:
                    vm = Factory.CreateNewInk();
                    break;
                case Mode.IMAGE:
                    vm = await Factory.CreateNewImage((StorageFile)data);
                    break;
                case Mode.PDF:
                    vm = await Factory.CreateNewPdfNodeViewModel((StorageFile)data);
                    break;
                default:
                    return;
            }
            NodeViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            PositionNode(vm, xCoordinate, yCoordinate);
        }

        private static void PositionNode(NodeViewModel vm, double xCoordinate, double yCoordinate)
        {
            vm.X = 0;
            vm.Y = 0;

            var transMat = ((MatrixTransform)vm.View.RenderTransform).Matrix;
            transMat.OffsetX = xCoordinate;
            transMat.OffsetY = yCoordinate;
            vm.Transform = new MatrixTransform { Matrix = transMat };
        }

        #region Public Members

        public ObservableCollection<NodeViewModel> NodeViewModelList { get; }

        public ObservableCollection<LinkViewModel> LinkViewModelList { get; }

        public ObservableCollection<UserControl> AtomViewList { get; }

        public AtomViewModel SelectedAtomViewModel { get; private set; }

        public Mode CurrentMode { get; set; }

        public LinkMode CurrentLinkMode { get; set; }


        public CompositeTransform CompositeTransform
        {
            get { return _compositeTransform; }
            set
            {
                if (_compositeTransform == value)
                {
                    return;
                }
                _compositeTransform = value;
                RaisePropertyChanged("CompositeTransform");
            }
        }

        public CompositeTransform FMTransform
            {
            get { return _fMTransform; }
            set
            {
                if (_fMTransform == value)
                {
                    return;
                }
                _fMTransform = value;
                RaisePropertyChanged("FMTransform");
            }
        }
        #endregion Public Members

    }
}