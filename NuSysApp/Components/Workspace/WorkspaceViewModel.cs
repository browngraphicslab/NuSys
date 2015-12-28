using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SQLite.Net.Async;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class WorkspaceViewModel : NodeContainerViewModel
    {
        #region Private Members

        private CompositeTransform _compositeTransform, _fMTransform;
        private AtomViewModel _preparedAtomVm;
  
        #endregion Private Members

        public WorkspaceViewModel(WorkSpaceModel model) : base(model)
        {
            Model = model;
            
            GroupDict = new Dictionary<string, NodeContainerViewModel>();
            MultiSelectedAtomViewModels = new List<AtomViewModel>();
            SelectedAtomViewModel = null;
            
            var c = new CompositeTransform
            {
                TranslateX = (-1) * (Constants.MaxCanvasSize),
                TranslateY = (-1) * (Constants.MaxCanvasSize)
            };

            CompositeTransform = c;
            FMTransform = new CompositeTransform();
        }

        #region Node Interaction

        public void CheckForInkNodeIntersection(InqLineModel inq)
        {
            var nodes = new List<NodeViewModel>();
            foreach (var node2 in AtomViewList.Where(a => a.DataContext is NodeViewModel))
            {
                var rect1 = Geometry.InqToBoudingRect(inq);
                var rect2 = Geometry.NodeToBoudingRect(node2.DataContext as NodeViewModel);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (!rect1.IsEmpty)
                {
                    nodes.Add(node2.DataContext as NodeViewModel);
                }
            }
            removeNodes(nodes);
        }

        private void removeNodes(List<NodeViewModel> nodes)
        {
            foreach (NodeViewModel node in nodes)
            {
                DeleteNode(node);
            }
        }


        public void DeleteNode(NodeViewModel node)
        {
            Debug.WriteLine("deleting node");
            var uc = AtomViewList.Where(a => a.DataContext == node).First();
            AtomViewList.Remove(uc);

        }


        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(AtomViewModel selected)
        {
            List<string> locks = new List<string>();
            locks.Add(selected.Model.Id);
            NetworkConnector.Instance.CheckLocks(locks);
            if (selected.Model.CanEdit == AtomModel.EditStatus.Maybe)
            {
                NetworkConnector.Instance.RequestLock(selected.Model.Id);
            }
            if (SelectedAtomViewModel == null)
            {
                SelectedAtomViewModel = selected;
                return;
            }
           // NetworkConnector.Instance.RequestMakeLinq(SelectedAtomViewModel.ID, selected.ID);
            selected.SetSelected(false);
            SelectedAtomViewModel.SetSelected(false);
            SelectedAtomViewModel = null;
            ClearSelection();
        }

        public void SetMultiSelection(AtomViewModel selected)
        {
            if (!selected.IsMultiSelected)
            {
                selected.IsMultiSelected = true;
                NetworkConnector.Instance.RequestLock(selected.Id);
                this.MultiSelectedAtomViewModels.Add(selected);
            }
        }

        public void MoveMultiSelection(AtomViewModel sender, double x, double y)
        {
            foreach (var atom in MultiSelectedAtomViewModels)
            {
                var node = atom as NodeViewModel;
                if (node != null && atom != sender)
                {
                    node.IsMultiSelected = false;
                    node.Translate(x, y);
                    node.IsMultiSelected = true;
                }
            }
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            NetworkConnector.Instance.ReturnAllLocks();
            if (SelectedAtomViewModel == null) return;
            SelectedAtomViewModel.SetSelected(false);
            SelectedAtomViewModel = null;
        }


        private delegate void AddInk(string s);

        public async void PromoteInk(Rect nodeBounds, List<InqLineModel> linesToPromote)
        {
            var dict = new Dictionary<string, string>();
            dict["width"] = nodeBounds.Width.ToString();
            dict["height"] = nodeBounds.Height.ToString();
            AddInk add = delegate (string s)
            {
                var v = SessionController.Instance.IdToSendables[s] as TextNodeModel;
                if (v != null)
                {
                    Debug.Assert(linesToPromote.Count > 0);
                    foreach (var model in linesToPromote)
                    {
                        UITask.Run(async () =>
                        {
                            //NetworkConnector.Instance.RequestLock(v.ID);
                            NetworkConnector.Instance.RequestFinalizeGlobalInk(model.Id, v.InqCanvas.ID, model.GetString());
                            //is the model being deleted and then trying to be added? is the canvas fully there when we try to add?
                        });
                    }
                }
            };
            Action<string> a = new Action<string>(add);
            await NetworkConnector.Instance.RequestMakeNode(nodeBounds.X.ToString(), nodeBounds.Y.ToString(), NodeType.Text.ToString(), null, null, dict, a);
        }
           
        public void ClearMultiSelection()
        {
            NetworkConnector.Instance.ReturnAllLocks();
            foreach (var avm in MultiSelectedAtomViewModels)
            {
                NetworkConnector.Instance.RequestReturnLock(avm.Id);
                avm.IsMultiSelected = false;
            }
            MultiSelectedAtomViewModels.Clear();
        }

        public void DeleteMultiSelecttion()
        {
            foreach (var avm in MultiSelectedAtomViewModels)
            {
                avm.Remove();
            }
            MultiSelectedAtomViewModels.Clear();
        }
        delegate void Del(string s);

        public async void GroupFromMultiSelection()
        {
            if (MultiSelectedAtomViewModels.Count < 2)
            {
                return;
            }

            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;

            foreach (var t in MultiSelectedAtomViewModels)
            {
                var nodeModel = (NodeModel)t.Model;
                minX = nodeModel.X < minX ? nodeModel.X : minX;
                minY = nodeModel.Y < minY ? nodeModel.Y : minY;
                maxX = nodeModel.X + nodeModel.Width > maxX ? nodeModel.X + nodeModel.Width : maxX;
                maxY = nodeModel.Y + nodeModel.Height > maxY ? nodeModel.Y + nodeModel.Height : maxY;
            }


            /*
            Del del = delegate (string s)
            {
                var groupmodel = (GroupNodeModel)SessionController.Instance.IdToSendables[s];

  
                foreach (var t in MultiSelectedAtomViewModels)
                {
                    var nodeModel = (NodeModel) t.Model;
                    nodeModel.X -= minX;
                    nodeModel.Y -= minY;
                    AtomViewList.Remove(t.View);
                    ((NodeModel)t.Model).MoveToGroup(groupmodel);
                }

                ClearMultiSelection();
            };
            */

            var props = new Dictionary<string, string>();
            props["width"] = (maxX - minX).ToString();
            props["height"] = (maxY - minY).ToString();

            var node1 = (NodeModel)MultiSelectedAtomViewModels[0].Model;
          //  await NetworkConnector.Instance.RequestMakeEmptyGroup(minX.ToString(), minY.ToString(),null, props, new Action<string>(del));
        }


        #endregion Node Interaction

        #region Event Handlers

        private void PartialLineAdditionHandler(object source, AddLineEventArgs e)
        {
            LastPartialLineModel = e.AddedLineModel;
            RaisePropertyChanged("PartialLineAdded");
        }

        #endregion Event Handlers
        #region Public Members

      
        public AtomViewModel SelectedAtomViewModel { get; private set; }

        public List<AtomViewModel> MultiSelectedAtomViewModels { get; private set; }

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

        public Dictionary<string, NodeContainerViewModel> GroupDict { get; private set; }

        public InqLineModel LastPartialLineModel { get; set; }
        #endregion Public Members

        
    }
}