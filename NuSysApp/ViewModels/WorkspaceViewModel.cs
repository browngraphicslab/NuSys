using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.MISC;
using SQLite.Net.Async;

namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class WorkspaceViewModel : BaseINPC
    {
        #region Private Members
        //private readonly Factory _factory;
        public enum LinkMode
        {
            Linelink,
            Bezierlink
        }
        private CompositeTransform _compositeTransform, _fMTransform;
        #endregion Private Members

        public WorkspaceViewModel(WorkSpaceModel model)
        {
            Model = model;
            AtomViewList = new ObservableCollection<UserControl>();
            NodeViewModelList = new ObservableCollection<NodeViewModel>();
            GroupDict = new Dictionary<string, GroupViewModel>();
            LinkViewModelList = new ObservableCollection<LinkViewModel>();
            PinViewModelList = new ObservableCollection<PinViewModel>();
            SelectedAtomViewModel = null;
            this.CurrentLinkMode = LinkMode.Bezierlink;

            myDB = new SQLiteDatabase("NuSysTest.sqlite");

            Init();
            var c = new CompositeTransform
            {
                TranslateX = (-1)*(Constants.MaxCanvasSize),
                TranslateY = (-1)*(Constants.MaxCanvasSize)
            };
            CompositeTransform = c;
            FMTransform = new CompositeTransform();
            this.Model.OnCreation += CreatedHandler;
            this.Model.OnPartialLineAddition += PartialLineAdditionHandler;
            this.Model.OnGroupCreation += CreateNewGroupHandler;
        }

        private void CreateNewGroupHandler(object source, CreateGroupEventArgs e)
        {
            this.CreateNewGroup(e.CreatedGroup.ID, e.CreatedGroup);
        }

        private async void Init()
        {

            Debug.WriteLine("Setting up Network Connector at IP: "+NetworkConnector.Instance.LocalIP);

        }

        

        /// <summary>
        /// Returns true if the given node intersects with any link on the workspace, 
        /// using a simple line approximation for Bezier curves.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
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
                    if (node is GroupViewModel)
                    {
                        return true;
                    }
                    if (node2 is GroupViewModel)
                    {
                        var group = (Group)(((GroupViewModel)node2).Model);
                        var nodeModel = (Node)node.Model;
                        nodeModel.MoveToGroup(group);
                        return true;
                    }
                    NetworkConnector.Instance.RequestMakeGroup(node.ID, node2.ID, ((Node)node.Model).X.ToString(), ((Node)node.Model).Y.ToString());
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
            NetworkConnector.Instance.ModelIntermediate.CheckLocks(selected.Model.ID);
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
            /*
            foreach (ISelectable select in SelectedComponents)
            {
                select.ToggleSelection();
            }
            SelectedComponents.Clear();*/
        }

        /// <summary>
        /// Creates a link between two nodes. 
        /// </summary>
        /// <param name="atomVM1"></param>
        /// <param name="atomVM2"></param>
        private LinkViewModel CreateNewLink(string id,AtomViewModel atomVm1, AtomViewModel atomVm2, Link link)
        {
            var vm1 = atomVm1 as NodeViewModel;
            if (vm1 != null && ((NodeViewModel)vm1).IsAnnotation)
            {
                return null;
            }
            var vm2 = atomVm2 as NodeViewModel;
            if (vm2 != null && ((NodeViewModel)vm2).IsAnnotation)
            {
                return null;
            }
            if (atomVm1 == atomVm2)
            {
                return null;
            }
            if (atomVm1 == atomVm2) return null;
            var vm = new LinkViewModel(link, atomVm1, atomVm2, this, id);//TODO fix this
            Model.AtomDict.Add(id, vm);

            if (vm1?.ParentGroup != null || vm2?.ParentGroup != null)
            {
                vm.IsVisible = false;
            }

            LinkViewModelList.Add(vm);
            AtomViewList.Add(vm.View);
            atomVm1.AddLink(vm);
            atomVm2.AddLink(vm);
            return vm;
        }

        private AtomViewModel _preparedAtomVm;
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

        public InqLine LastPartialLine { get; set; }
        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            LastPartialLine = e.AddedLine;
            RaisePropertyChanged("PartialLineAdded");
        }

        public async void CreatedHandler(object source, CreateEventArgs e)
        {
            NodeViewModel vm = null;
            var model = e.CreatedNode;
            var type = model.NodeType;
            var id = model.ID;
            //var data = model.Data;
            var x = model.X;
            var y = model.Y;
            switch (type)
            {
                case NodeType.Text:
                    vm = new TextNodeViewModel((TextNode)model, this, "Enter Text Here", id);
                    break;
                case NodeType.Richtext:
                    vm = new TextNodeViewModel((TextNode)model, this, "Enter Text Here", id);
                    break;
                case NodeType.Ink:
                    vm = new InkNodeViewModel((InkModel)model, this, id);
                    break;
                case NodeType.Image:
                    vm = new ImageNodeViewModel((ImageModel)model,this,id);
                    if (((ImageModel)vm.Model).Image != null){
                        vm.Width = ((ImageModel)vm.Model).Image.PixelWidth;//TODO remove this line and the next
                        vm.Height = ((ImageModel)vm.Model).Image.PixelHeight;
                    }
                    break;
                case NodeType.PDF:
                    vm = new PdfNodeViewModel((PdfNodeModel)model,this,id);
                    if (((PdfNodeModel)vm.Model).RenderedPage != null)
                    {
                        vm.Width = ((PdfNodeModel)vm.Model).RenderedPage.PixelWidth;//TODO remove this line and the next
                        vm.Height = ((PdfNodeModel)vm.Model).RenderedPage.PixelHeight;
                    }
                    break;
                case NodeType.Audio:
                    vm = new AudioNodeViewModel((AudioModel)model, this, id);
                    break;
                default:
                    return;
                    break;
            }
            AtomViewList.Add(vm.View);
            NodeViewModelList.Add(vm);
            PositionNode(vm, x, y);
        }

        public async Task<PinViewModel> AddNewPin(double x, double y)
        {
            PinViewModel vm = new PinViewModel();
            PinViewModelList.Add(vm);
            if (vm != null)
            {
                AtomViewList.Add(vm.View);
                PositionPin(vm, x, y);
            }
            return vm;
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
        
        public void CreateNewGroup(string id,Group groupModel) 
        {          
            //Create new group, because no group exists
            var groupVm = new GroupViewModel(groupModel,this, id);

            //Set location to node2's location
            var xCoordinate = groupModel.X;
            var yCoordinate = groupModel.Y;

            //Add group to workspace
            NodeViewModelList.Add(groupVm);
            AtomViewList.Add(groupVm.View);
            GroupDict.Add(groupModel.ID, groupVm);
            PositionNode(groupVm, xCoordinate, yCoordinate);            
        }

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

            foreach (NodeViewModel nodeVm in NodeViewModelList)
            {
                if (!nodeVm.IsAnnotation)
                {
                    XmlElement ele = nodeVm.WriteXML(doc);
                    parent.AppendChild(ele);
                }
            }

            foreach (LinkViewModel linkVm in LinkViewModelList)
            {
                XmlElement ele = linkVm.WriteXML(doc);
                parent.AppendChild(ele);
            }
            Debug.WriteLine(doc.OuterXml);
            return doc;
        }

        public void PositionNode(NodeViewModel vm, double xCoordinate, double yCoordinate)
        {
            var transMat = ((MatrixTransform)vm.View.RenderTransform).Matrix;
            transMat.OffsetX = xCoordinate;
            transMat.OffsetY = yCoordinate;
            vm.Transform = new MatrixTransform { Matrix = transMat };
        }

        #region Public Members

        public ObservableCollection<NodeViewModel> NodeViewModelList { get; }

        public ObservableCollection<PinViewModel> PinViewModelList { get; }

        public ObservableCollection<LinkViewModel> LinkViewModelList { get; }

        public ObservableCollection<UserControl> AtomViewList { get; }

        public AtomViewModel SelectedAtomViewModel { get; private set; }

        public SQLiteDatabase myDB { get; set; }

        public WorkSpaceModel Model { get; set; }

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

        public Dictionary<string, GroupViewModel> GroupDict { get; private set; }
        #endregion Public Members
    }
}