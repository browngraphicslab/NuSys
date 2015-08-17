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
using Windows.Storage.Streams;
using System.Text;
using System.Xml;
using SQLite.Net.Async;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using System.Xml;
using System.IO;
using Windows.UI.Xaml.Shapes;
using SQLite.Net;

namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class WorkspaceViewModel : BaseINPC
    {
        #region Private Members

        private readonly Factory _factory;
        private SQLiteDatabase myDB;
        private int idCounter;

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
            this.CurrentLinkMode = LinkMode.Bezierlink;

            myDB = new SQLiteDatabase("NuSysTest.sqlite");
            idCounter = 0;

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
            await SetupDirectories();
            SetupChromeIntermediate();
            SetupOfficeTransfer();       
        }

        private async void SetupOfficeTransfer()
        {
            var fw = new FolderWatcher(NuSysStorages.PowerPointTransferFolder);
            fw.FilesChanged += async delegate
            {            
                var foundUpdate = await NuSysStorages.PowerPointTransferFolder.TryGetItemAsync("update.nusys").AsTask();
                if (foundUpdate == null)
                {
                    Debug.WriteLine("no update yet!");
                    return;
                }
                await foundUpdate.DeleteAsync();

                var transferFiles = await NuSysStorages.PowerPointTransferFolder.GetFilesAsync().AsTask();
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                foreach (var file in transferFiles) { 

                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var lines = await FileIO.ReadLinesAsync(file);
                        if (lines[0].EndsWith(".png"))
                        {
                            var str = lines[0];
                            var imageFile = await NuSysStorages.Media.GetFileAsync(lines[0]).AsTask();
                            var nodeVm = await Factory.CreateNewImage(this, imageFile, idCounter);
                            idCounter++;
                            var p = CompositeTransform.Inverse.TransformPoint(new Point(250, 200));
                            PositionNode(nodeVm, p.X, p.Y);
                            NodeViewModelList.Add(nodeVm);
                            AtomViewList.Add(nodeVm.View);

                        } else {
                            var readFile = await FileIO.ReadTextAsync(file);
                            var nodeVm = Factory.CreateNewRichText(this, readFile, idCounter);
                            idCounter++;
                            var p = CompositeTransform.Inverse.TransformPoint(new Point(250, 200));
                            PositionNode(nodeVm, p.X, p.Y);
                            NodeViewModelList.Add(nodeVm);
                            AtomViewList.Add(nodeVm.View);
                        }
                    });
                }

                foreach (var file in transferFiles)
                {
                    await file.DeleteAsync();
                }
            };
        }

        private void SetupChromeIntermediate()
        {
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.ChromeTransferFolder.GetFilesAsync().AsTask();

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                var count = 0;
                foreach (var file in transferFiles)
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        DataReader reader = DataReader.FromBuffer(buffer);
                        byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                        reader.ReadBytes(fileContent);
                        string text = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

                        var nodeVm = Factory.CreateNewRichText(this, text, idCounter);
                        idCounter++;
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

        private static async Task<bool> SetupDirectories()
        {
            NuSysStorages.NuSysTempFolder = await StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FolderNusysTemp);
            NuSysStorages.ChromeTransferFolder = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderChromeTransferName);
           
            NuSysStorages.NuSysTempFolder =
                await StorageUtil.CreateFolderIfNotExists(KnownFolders.DocumentsLibrary, Constants.FolderNusysTemp);
            NuSysStorages.ChromeTransferFolder =
                await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderChromeTransferName);
            NuSysStorages.WordTransferFolder = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderWordTransferName);
            NuSysStorages.PowerPointTransferFolder = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderPowerpointTransferName);
            NuSysStorages.Media = await StorageUtil.CreateFolderIfNotExists(NuSysStorages.NuSysTempFolder, Constants.FolderMediaName);
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
                    if (link.IsVisible && Geometry.LinesIntersect(line1, line2) && link.Atom1 != node && link.Atom2 != node)
                    {
                        node.ClippedParent = link;
                        link.Annotation = node;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckForNodeNodeIntersection(NodeViewModel node)
        {
            if (node.ParentGroup != null)
            {
                var x = node.Transform.Matrix.OffsetX * node.ParentGroup.LocalTransform.ScaleX;
                var y = node.Transform.Matrix.OffsetY * node.ParentGroup.LocalTransform.ScaleY;
                if (x > node.ParentGroup.Width || x < 0 || y > node.ParentGroup.Height || y < 0) 
                {
                    node.ParentGroup.RemoveNode(node);
                    NodeViewModelList.Add(node);
                    AtomViewList.Add(node.View);
                    PositionNode(node, node.ParentGroup.Transform.Matrix.OffsetX + x, node.ParentGroup.Transform.Matrix.OffsetY + y);
                    node.ParentGroup = null;
                    node.UpdateAnchor();
                    return false;
                }
                node.ParentGroup.CheckNodeIntersection(node);
            }
            foreach (var node2 in NodeViewModelList)
            {
                var rect1 = Geometry.NodeToBoudingRect(node);
                var rect2 = Geometry.NodeToBoudingRect(node2);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (node != node2 && !rect1.IsEmpty)
                {
                    CreateNewGroup(node, node2);
                    return true;
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
            //Remove all the node's links
            var toDelete = new List<LinkViewModel>();
            foreach (var linkVm in nodeVM.LinkList)
            {
                AtomViewList.Remove(linkVm.View);
                toDelete.Add(linkVm);
            }

            foreach (var linkVm in toDelete) //second loop avoids concurrent modification error
            {
                linkVm.Remove();
                nodeVM.LinkList.Remove(linkVm);
            }

            if (nodeVM.ParentGroup == null)
            {
                AtomViewList.Remove(nodeVM.View);
                NodeViewModelList.Remove(nodeVM);
            }
            else
            {
                nodeVM.ParentGroup.RemoveNode(nodeVM);
            }
           
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
            var vm1 = atomVm1 as NodeViewModel;
            if (vm1 != null && ((NodeViewModel)vm1).IsAnnotation)
            {
                return;
            }
            var vm2 = atomVm2 as NodeViewModel;
            if (vm2 != null && ((NodeViewModel)vm2).IsAnnotation)
            {
                return;
            }
            if (atomVm1 == atomVm2) return;
            var vm = new LinkViewModel(atomVm1, atomVm2, this, idCounter);
            idCounter++;

            if (vm1?.ParentGroup != null || vm2?.ParentGroup != null)
            {
                vm.IsVisible = false;
            }
            LinkViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            atomVm1.AddLink(vm);
            atomVm2.AddLink(vm);
        }

        public async Task<NodeViewModel> CreateNewNode(NodeType type, double xCoordinate, double yCoordinate, object data = null)
        {
            NodeViewModel vm = null;
            switch (type)
            {
                case NodeType.Text:
                    vm = new TextNodeViewModel(this, (string)data, idCounter);
                    idCounter++;
                    break;
                case NodeType.Ink:
                    vm = new InkNodeViewModel(this, idCounter);
                    idCounter++;
                    break;
                case NodeType.Document:
                    var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
                    if (storageFile == null) return null;
                    
                    if (Constants.ImageFileTypes.Contains(storageFile.FileType))
                    {
                        var imgVM = new ImageNodeViewModel(this, idCounter);
                        await imgVM.InitializeImageNodeAsync(storageFile);
                        vm = imgVM;
                        idCounter++;
                    }

                    if (Constants.PdfFileTypes.Contains(storageFile.FileType))
                    {
                        var pdfVM = new PdfNodeViewModel(this, idCounter);
                        await pdfVM.InitializePdfNodeAsync(storageFile);
                        vm = pdfVM;
                        idCounter++;
                    }
                    break;
                //   case Mode.InkSelect:
                //      vm = Factory.CreateNewPromotedInk(this);
                //      break;
                default:
                    return null;
            }
            NodeViewModelList.Add(vm);

            if (vm != null)
            {
                AtomViewList.Add(vm.View);

                if (data is Polyline[])
                {
                    Polyline p = (data as Polyline[]).First();
                    var minX = p.Points.Min(em => em.X);
                    var minY = p.Points.Min(em => em.Y);
                    (vm.View as InkNodeView2).PromoteStrokes(data as Polyline[]);
                    PositionNode(vm, minX, minY);
                }
                else
                {
                    PositionNode(vm, xCoordinate, yCoordinate);
                }
            }
            return vm;
        }

        public void CreateNewGroup(NodeViewModel node1, NodeViewModel node2)
        {
            if (node1 is GroupViewModel)
            {
                return; //TODO this is temporary until we fix everything else
            }
            //Check if group already exists
            var groupVm = node2 as GroupViewModel;
            if (groupVm != null)
            {
                var group = groupVm;
                this.AtomViewList.Remove(node1.View);
                this.NodeViewModelList.Remove(node1); 
                groupVm.AddNode(node1);
                node1.ParentGroup = groupVm;
                return;
            }

            //Create new group, because no group exists
            groupVm = new GroupViewModel(this, idCounter);
            idCounter++;

            //Set location to node2's location
            var xCoordinate = node2.Transform.Matrix.OffsetX;
            var yCoordinate = node2.Transform.Matrix.OffsetY;
          
            //Add group to workspace
            NodeViewModelList.Add(groupVm);
            AtomViewList.Add(groupVm.View);
            PositionNode(groupVm, xCoordinate, yCoordinate);

            //Add the first node
            groupVm.AddNode(node1);
            this.AtomViewList.Remove(node1.View);
            this.NodeViewModelList.Remove(node1);

            //Add the second node
            groupVm.AddNode(node2);
            this.AtomViewList.Remove(node2.View);
            this.NodeViewModelList.Remove(node2);

            node1.ParentGroup = groupVm;
            node2.ParentGroup = groupVm;
        }

        public async Task SaveWorkspace()
        {
            // clear the existing table so that there is always only one workspace to load, just for testing purposes
            SQLiteAsyncConnection dbConnection = myDB.DBConnection;
            dbConnection.DropTableAsync<XmlFileHelper>();

            // recreate the table to store the xml file of the current workspace
            dbConnection.CreateTableAsync<XmlFileHelper>();
            XmlFileHelper currWorkspaceXml = new XmlFileHelper();
            currWorkspaceXml.toXml = currWorkspaceXml.XmlToString(this.getXml());
            //Debug.WriteLine(currWorkspaceXml.XmlToString(this.getXml()));
            dbConnection.InsertAsync(currWorkspaceXml);
        }

        public async Task LoadWorkspace()
        {
            SQLiteAsyncConnection dbConnection = myDB.DBConnection;
            var query = dbConnection.Table<XmlFileHelper>().Where(v => v.ID == 1);
            query.FirstOrDefaultAsync().ContinueWith((t) => 
            t.Result.ParseXml(this, t.Result.StringToXml(t.Result.toXml)));
        }

        public XmlDocument getXml()
        {
            //Document declaration
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement parent = doc.CreateElement(string.Empty, "Parent", string.Empty);
            doc.AppendChild(parent);

            for (int i = 0; i < NodeViewModelList.Count; i++)
            {
                XmlElement ele = NodeViewModelList[i].WriteXML(doc);
                parent.AppendChild(ele);
            }

            for(int i=0; i<LinkViewModelList.Count; i++)
            {
                XmlElement ele = LinkViewModelList[i].WriteXML(doc);
                parent.AppendChild(ele);
            }
            return doc;
        }

        public void PositionNode(NodeViewModel vm, double xCoordinate, double yCoordinate)
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

        //public Mode CurrentMode { get; set; }

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