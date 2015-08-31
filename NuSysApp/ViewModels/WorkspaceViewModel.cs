﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SQLite.Net.Async;

namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class WorkspaceViewModel : AtomViewModel
    {
        #region Private Members

        private CompositeTransform _compositeTransform, _fMTransform;
        private AtomViewModel _preparedAtomVm;
        #endregion Private Members

        public WorkspaceViewModel(WorkSpaceModel model) : base(model, null)
        {
            Model = model;
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            GroupDict = new Dictionary<string, GroupViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            PinViewModelList = new ObservableCollection<PinViewModel>();
            SelectedAtomViewModel = null;
            myDB = new SQLiteDatabase("NuSysTest.sqlite");
            this.SetUpTransforms();
            this.SetUpHandlers();
        }

        #region Helper Methods
        private void SetUpTransforms()
        {
            var c = new CompositeTransform
            {
                TranslateX = (-1) * (Constants.MaxCanvasSize),
                TranslateY = (-1) * (Constants.MaxCanvasSize)
            };
            CompositeTransform = c;
            FMTransform = new CompositeTransform();

        }

        private void SetUpHandlers()
        {
            this.Model.OnCreation += CreatedHandler;
            //this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
            this.Model.OnGroupCreation += CreateNewGroupHandler;
            this.Model.OnPinCreation += CreatePinHandler;
        }

        #endregion Helper Methods
        #region Node Interaction


        public void PositionNode(NodeViewModel vm, double xCoordinate, double yCoordinate)
        {
            var transMat = ((MatrixTransform)vm.View.RenderTransform).Matrix;
            transMat.OffsetX = xCoordinate;
            transMat.OffsetY = yCoordinate;
            vm.Transform = new MatrixTransform { Matrix = transMat };
        }
        private void PositionPin(PinViewModel vm, double x, double y)
        {
            var trans = vm.Transform.Matrix;
            trans.OffsetX = x;
            trans.OffsetY = y;
            //    trans.M11 = 1 / CompositeTransform.ScaleX;
            //    trans.M22 = 1 / CompositeTransform.ScaleY;
            vm.Transform = new MatrixTransform { Matrix = trans };
        }
        public bool CheckForNodeLinkIntersections(NodeViewModel node)
        {
            return false;//TODO re-implement annotations
            var lines = Geometry.NodeToLineSegment(node);
            foreach (var link in LinkViewModelList)
            {

                var line1 = link.LineRepresentation;
                foreach (var line2 in lines)
                {
                    if (link.IsVisible && Geometry.LinesIntersect(line1, line2) && link.Atom1 != node && link.Atom2 != node)
                    {
                        node.ClippedParent = link;
                        ((Node)node.Model).ClippedParent = link.Model;
                        ((Link)link.Model).Annotation = (Node)node.Model;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method performs 3 checks: First, it checks whether "node" has been dragged out of the
        /// group. If yes, the node is moved from the group to the workspace. Second, it checks if
        /// "node" has been dragged onto a preexisting group. If yes, "node" is added to the group.
        /// Third, if "node" has neither been dragged out of the group nor added to a pre-existing group,
        /// a new group is created that will contain "node" and its intersecting node.
        /// </summary>
        /// <param name="node"></param>
        public void CheckForNodeNodeIntersection(NodeViewModel node)
        {
            if (node.ParentGroup != null)//Node is in a group (meaning not the workspace)
            {
                var x = node.Transform.Matrix.OffsetX * node.ParentGroup.LocalTransform.ScaleX;
                var y = node.Transform.Matrix.OffsetY * node.ParentGroup.LocalTransform.ScaleY;
                if (x > node.ParentGroup.Width || x < 0 || y > node.ParentGroup.Height || y < 0)//node has been moved out of its group
                {
                    var nodeModel = (Node)node.Model;
                    nodeModel.MoveToGroup(null);//remove from group (meaning move back to workspace)
                    PositionNode(node, node.ParentGroup.Transform.Matrix.OffsetX + x, node.ParentGroup.Transform.Matrix.OffsetY + y);
                    return;
                }
            }
            foreach (var node2 in NodeViewModelList)
            {
                var rect1 = Geometry.NodeToBoudingRect(node);
                var rect2 = Geometry.NodeToBoudingRect(node2);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (node != node2 && !rect1.IsEmpty)
                {
                    if (node is GroupViewModel)//dragging nested group onto node or group
                    {
                        return;//currently, do nothing
                    }
                    if (node2 is GroupViewModel)//dragging nested group onto existing group
                    {
                        var group = (Group)(((GroupViewModel)node2).Model);
                        var nodeModel = (Node)node.Model;
                        nodeModel.MoveToGroup(group);
                        return;
                    }
                    //no group exists, request network to make one
                    NetworkConnector.Instance.RequestMakeGroup(node.ID, node2.ID, ((Node)node.Model).X.ToString(), ((Node)node.Model).Y.ToString());
                    return;
                }
            }
        }
        
        public void DeleteLink(LinkViewModel linkViewModel)
        {
            //Remove all the node's links
            var toDelete = new List<LinkViewModel>();
            foreach (var linkVm in linkViewModel.LinkList)
            {
                AtomViewList.Remove(linkVm.View);
                toDelete.Add(linkVm);
            }

            foreach (var linkVm in toDelete) //second loop avoids concurrent modification error
            {
                linkVm.Remove();
                linkViewModel.LinkList.Remove(linkVm);
            }
            AtomViewList.Remove(linkViewModel.View);
            LinkViewModelList.Remove(linkViewModel);
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
        public void CreateNewGroup(Group groupModel)
        {
            //Create new group, because no group exists
            var groupVm = new GroupViewModel(groupModel, this);

            //Set location to node2's location
            var xCoordinate = groupModel.X;
            var yCoordinate = groupModel.Y;

            //Add group to workspace
            NodeViewModelList.Add(groupVm);
            AtomViewList.Add(groupVm.View);
            GroupDict.Add(groupModel.ID, groupVm);
            PositionNode(groupVm, xCoordinate, yCoordinate);
        }
        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(AtomViewModel selected)
        {
            List<string> locks = new List<string>();
            locks.Add(selected.Model.ID);
            NetworkConnector.Instance.ModelIntermediate.CheckLocks(locks);
            if (selected.Model.CanEdit == Atom.EditStatus.Maybe)
            {
                NetworkConnector.Instance.RequestLock(selected.Model.ID);
            }
            if (SelectedAtomViewModel == null)
            {
                SelectedAtomViewModel = selected;
                return;
            }
            NetworkConnector.Instance.RequestMakeLinq(SelectedAtomViewModel.ID, selected.ID);
            selected.IsSelected = false;
            SelectedAtomViewModel.IsSelected = false;
            SelectedAtomViewModel = null;
            ClearSelection();
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            NetworkConnector.Instance.ModelIntermediate.ClearLocks();
            if (SelectedAtomViewModel == null) return;
            SelectedAtomViewModel.IsSelected = false;
            SelectedAtomViewModel = null;
        }

        /// <summary>
        /// Creates a link between two nodes. 
        /// </summary>
        /// <param name="atomVM1"></param>
        /// <param name="atomVM2"></param>
        private void CreateNewLink(string id, AtomViewModel atomVm1, AtomViewModel atomVm2, Link link)
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
            if (atomVm1 == atomVm2)
            {
                return;
            }
            if (atomVm1 == atomVm2) return;
            var vm = new LinkViewModel(link, atomVm1, atomVm2, this);//TODO fix this
            Model.AtomDict.Add(id, vm);

            if (vm1?.ParentGroup != null || vm2?.ParentGroup != null)
            {
                vm.IsVisible = false;
            }

            LinkViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            atomVm1.AddLink(vm);
            atomVm2.AddLink(vm);
        }

        #endregion Node Interaction
        #region Save/Load
        public async Task SaveWorkspace()
        {
            // clear the existing tables so that there is always only one workspace to load, just for testing purposes
            SQLiteAsyncConnection dbConnection = myDB.DBConnection;
            await dbConnection.DropTableAsync<XmlFileHelper>();
            await dbConnection.DropTableAsync<Content>();

            // recreate the table to store the xml file of the current workspace
            await dbConnection.CreateTableAsync<XmlFileHelper>(); // table to store the xml file of current workspace
            await dbConnection.CreateTableAsync<Content>(); // table to store content of each node

            // generate and save the xml of the current workspace
            XmlFileHelper currWorkspaceXml = new XmlFileHelper();
            XmlDocument doc = this.getXml();
            currWorkspaceXml.toXml = doc.OuterXml;
            await dbConnection.InsertAsync(currWorkspaceXml);

            // save the content of each atom in the current workspace
            foreach (NodeViewModel nodeVm in NodeViewModelList)
            {
                if (((Node)nodeVm.Model).Content != null)
                {
                    Content toInsert = ((Node)nodeVm.Model).Content;
                    await dbConnection.InsertAsync(toInsert);
                }
            }
        }

        public async Task LoadWorkspace()
        {
            SQLiteAsyncConnection dbConnection = myDB.DBConnection;
            var query = dbConnection.Table<XmlFileHelper>().Where(v => v.ID == "1");
            query.FirstOrDefaultAsync().ContinueWith(async (t) =>
                await t.Result.ParseXml(this, t.Result.StringToXml(t.Result.toXml)));
        }

        public XmlDocument getXml()
        {
            // document declaration
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement parent = doc.CreateElement(string.Empty, "Parent", string.Empty);
            doc.AppendChild(parent);

            foreach (var nodeVm in NodeViewModelList)
            {
                if (!nodeVm.IsAnnotation)
                {
                    XmlElement ele = nodeVm.WriteXML(doc);
                    parent.AppendChild(ele);
                }
            }

            foreach (var pinVm in PinViewModelList)
            {
                XmlElement ele = ((PinModel)pinVm.Model).WriteXML(doc);
                parent.AppendChild(ele);
            }

            foreach (var linkVm in LinkViewModelList)
            {
                XmlElement ele = linkVm.WriteXML(doc);
                parent.AppendChild(ele);
            }
            Debug.WriteLine(doc.OuterXml);
            return doc;
        }


        #endregion Save/Load
        #region Event Handlers

        private void CreatePinHandler(object source, CreatePinEventArgs e)
        {
            var vm = new PinViewModel(e.CreatedPin);
            PinViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            PositionPin(vm, e.CreatedPin.X, e.CreatedPin.Y);  
        }

        private void CreateNewGroupHandler(object source, CreateGroupEventArgs e)
        {
            this.CreateNewGroup(e.CreatedGroup);
        }


        public async void CreatedHandler(object source, CreateEventArgs e)
        {
            NodeViewModel vm = null;
            var model = e.CreatedNode;
            var type = model.NodeType;
            var x = model.X;
            var y = model.Y;
            
            switch (type)
            {
                case NodeType.Text:
                    vm = new TextNodeViewModel((TextNode)model, this);
                    break;
                case NodeType.Richtext:
                    vm = new TextNodeViewModel((TextNode)model, this);
                    break;
                case NodeType.Ink:
                    vm = new InkNodeViewModel((InkModel)model, this);
                    break;
                case NodeType.Image:
                    vm = new ImageNodeViewModel((ImageModel)model, this);
                    if (((ImageModel)vm.Model).Image != null)
                    {
                        vm.Width = ((ImageModel)vm.Model).Image.PixelWidth;//TODO remove this line and the next
                        vm.Height = ((ImageModel)vm.Model).Image.PixelHeight;
                    }
                    break;
                case NodeType.PDF:
                    vm = new PdfNodeViewModel((PdfNodeModel)model, this);
                    if (((PdfNodeModel)vm.Model).RenderedPage != null)
                    {
                        vm.Width = ((PdfNodeModel)vm.Model).RenderedPage.PixelWidth;//TODO remove this line and the next
                        vm.Height = ((PdfNodeModel)vm.Model).RenderedPage.PixelHeight;
                    }
                    break;
                case NodeType.Audio:
                    vm = new AudioNodeViewModel((AudioModel)model, this);
                    break;
                default:
                    return;
                    break;
            }
            var view = vm.View;
            var tpl = view.FindName("nodeTpl") as NodeTemplate;
            if (tpl != null)
            {
                tpl.OnTemplateReady += delegate {
                    new InqCanvasViewModel(tpl.inkCanvas, model.InqCanvas);
                };
            }
            AtomViewList.Add(vm.View);
            NodeViewModelList.Add(vm);
            PositionNode(vm, x, y);
        }

        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            LastPartialLine = e.AddedLine;
            RaisePropertyChanged("PartialLineAdded");
        }

        #endregion Event Handlers
        #region Event Helpers
        public void PrepareLink(string id, AtomViewModel atomVm, Link link)
        {
            if (_preparedAtomVm == null)
            {
                _preparedAtomVm = atomVm;
                return;
            }
            else if (atomVm != _preparedAtomVm)
            {
                CreateNewLink(id, _preparedAtomVm, atomVm, link);
            }
            _preparedAtomVm = null;
        }
        #endregion Event Helpers
        #region Public Members

        public ObservableCollection<NodeViewModel> NodeViewModelList { get; }

        public ObservableCollection<PinViewModel> PinViewModelList { get; }

        public ObservableCollection<LinkViewModel> LinkViewModelList { get; }

        public ObservableCollection<UserControl> AtomViewList { get; }

        public AtomViewModel SelectedAtomViewModel { get; private set; }

        public SQLiteDatabase myDB { get; set; }

        public WorkSpaceModel Model { get; set; }
        
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

        public override void Remove(){}
        public override void UpdateAnchor() { }

        public Dictionary<string, GroupViewModel> GroupDict { get; private set; }

        public InqLine LastPartialLine { get; set; }
        #endregion Public Members
    }
}