﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.MISC;
using System.IO;
using Windows.Storage.Search;

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
            Textnode,
            Globalink,
            Ink,
            Erase,
            Image,
            Pdf,
            InkSelect
        }; //enum created to switch between multiple modes in the appbar

        public enum LinkMode
        {
            Linelink,
            Bezierlink
        }

        private CompositeTransform _compositeTransform, _fMTransform;
        

        #endregion Private Members

        public WorkspaceViewModel()
        {
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            SelectedAtomViewModel = null;
            this.CurrentMode = Mode.Textnode;
            this.CurrentLinkMode = LinkMode.Bezierlink;
            //_factory = new Factory(this);


            Init();
            var c = new CompositeTransform
            {
                TranslateX = (-1)*(Constants.MaxCanvasSize),
                TranslateY = (-1)*(Constants.MaxCanvasSize)
            };
            CompositeTransform = c;
            FMTransform = new CompositeTransform();
        }


        private async void Init()
        {
            /*var result = */await SetupDirectories();
            SetupChromeIntermediate();
            SetUpOfficeToPdfWatcher();
        }
        private void SetupChromeIntermediate()
        {
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                //Debug.WriteLine("CONTENTS CHANGED! ");
                var transferFiles = await NuSysStorages.ChromeTransferFolder.GetFilesAsync().AsTask();

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                var count = 0;
                foreach (var file in transferFiles)
                {
                    Debug.WriteLine(file.Path);

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var readFile = await FileIO.ReadTextAsync(file);
                        //var nodeVm = _factory.CreateNewRichText(readFile);
                        var nodeVm = Factory.CreateNewRichText(this, readFile);
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

        private static void SetUpOfficeToPdfWatcher()
        {
            var folderWatcher = new FolderWatcher(NuSysStorages.OfficeToPdfFolder);
            // runs when folder contents change
            folderWatcher.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.OfficeToPdfFolder.GetFilesAsync();
                Debug.WriteLine("Number of files in OfficeToPdf: {0}", transferFiles.Count());
                foreach (var file in transferFiles)
                {
                    Debug.WriteLine("File name: " + file.Name);
                    Debug.WriteLine("File path: " + file.Path);
                    var fileContents = await FileIO.ReadTextAsync(file);
                    Debug.WriteLine("File contents: " + fileContents);
                }
            };
        }

        private static async Task<bool> SetupDirectories()
        {
            NuSysStorages.NuSysTempFolder =
                await StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FolderNusysTemp);
            NuSysStorages.ChromeTransferFolder =
                await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderChromeTransferName);
            NuSysStorages.OfficeToPdfFolder =
                await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderOfficeToPdf);
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
            if (CurrentMode != Mode.Textnode && CurrentMode != Mode.Ink) return;
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
                case Mode.Textnode:
                    vm = Factory.CreateNewText(this, "Enter text here");
                    break;
                case Mode.Ink:
                    vm = Factory.CreateNewInk(this);
                    break;
                case Mode.Image:
                    vm = await Factory.CreateNewImage(this, (StorageFile)data);
                    break;
                case Mode.Pdf:
                    vm = await Factory.CreateNewPdfNodeViewModel(this, (StorageFile)data);
                    break;
                case Mode.InkSelect:
                    vm = Factory.CreateNewPromotedInk(this);
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