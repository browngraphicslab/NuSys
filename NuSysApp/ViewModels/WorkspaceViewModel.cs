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
using Windows.UI.Xaml.Input;

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
        private INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        #endregion Private Members

        public WorkspaceViewModel(WorkSpaceModel model) : base(model)
        {
            Model = model;
            AtomViewList = new ObservableCollection<UserControl>();
            GroupDict = new Dictionary<string, GroupViewModel>();
            MultiSelectedAtomViewModels = new List<AtomViewModel>();
            SelectedAtomViewModel = null;
            myDB = new SQLiteDatabase("NuSysTest.sqlite");

            model.ChildAdded += OnChildAdded;
            
            var c = new CompositeTransform
            {
                TranslateX = (-1) * (Constants.MaxCanvasSize),
                TranslateY = (-1) * (Constants.MaxCanvasSize)
            };
            CompositeTransform = c;
            FMTransform = new CompositeTransform();
        }
        
        public async void OnChildAdded(object source, Sendable nodeModel)
        {
            var view = _nodeViewFactory.CreateFromSendable(nodeModel, AtomViewList.ToList());
            AtomViewList.Add(view);
        }
        
        public void OnChildDeleted(object source, Sendable sendable)
        {
            //   var view = _nodeViewFactory.CreateFromSendable()
            //  AtomViewList.Remove()
        }

        #region Node Interaction

        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(AtomViewModel selected)
        {
            List<string> locks = new List<string>();
            locks.Add(selected.Model.ID);
            NetworkConnector.Instance.CheckLocks(locks);
            if (selected.Model.CanEdit == AtomModel.EditStatus.Maybe)
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

        public void SetMultiSelection(AtomViewModel selected)
        {
            if (!selected.IsMultiSelected)
            {
                selected.IsMultiSelected = true;
                NetworkConnector.Instance.RequestLock(selected.ID);
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
            SelectedAtomViewModel.IsSelected = false;
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
                var v = this.Model.Children[s] as TextNodeModel;
                if (v != null)
                {
                    foreach (var model in linesToPromote)
                    {
                        UITask.Run( async() =>
                        {
                            //NetworkConnector.Instance.RequestLock(v.ID);
                            NetworkConnector.Instance.RequestFinalizeGlobalInk(model.ID, v.InqCanvas.ID, model.GetString());
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
                NetworkConnector.Instance.RequestReturnLock(avm.ID);
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
            if (!(MultiSelectedAtomViewModels[0] is NodeViewModel) || !(MultiSelectedAtomViewModels[1] is NodeViewModel))
            {
                return;
            }
            var node1 = (NodeModel)MultiSelectedAtomViewModels[0].Model;
            var node2 = (NodeModel) MultiSelectedAtomViewModels[1].Model;
            Del del = delegate (string s)
            {
                Debug.WriteLine("gid = " + s);
                Debug.WriteLine(MultiSelectedAtomViewModels.ToString());

                var groupmodel = (GroupNodeModel)SessionController.Instance.IdToSendables[s];
                for (int index = 0; index < MultiSelectedAtomViewModels.Count; index++)
                {
                    Debug.WriteLine(NetworkConnector.Instance.HasLock(s));
                    Debug.WriteLine(MultiSelectedAtomViewModels.Count);
                    var avm = MultiSelectedAtomViewModels[index];
        //            if (avm is NodeViewModel)
         //           {
                        ((NodeModel)avm.Model).MoveToGroup(groupmodel);
                    //Debug.WriteLine((avm.Model  as NodeModel).ParentGroup.ID);
                    //Debug.WriteLine(((avm.Model  as NodeModel).ParentGroup as GroupNodeModel).NodeModelList.Count);
         //           }
                }
            ClearMultiSelection();
            };
            
            Action<string> a = new Action<string>(del);
            await NetworkConnector.Instance.RequestMakeEmptyGroup(node1.X.ToString(), node2.Y.ToString(),null,null,a);
            //            await NetworkConnector.Instance.RequestMakeGroup(node1.ID, node2.ID, node1.X.ToString(),node2.Y.ToString());
            //            if (MultiSelectedAtomViewModels.Count > 1)
            //            {
//            }
        }


        #endregion Node Interaction

        #region Event Handlers

        private void PartialLineAdditionHandler(object source, AddPartialLineEventArgs e)
        {
            LastPartialLineModel = e.AddedLineModel;
            RaisePropertyChanged("PartialLineAdded");
        }

        #endregion Event Handlers
        #region Public Members

      

        public ObservableCollection<UserControl> AtomViewList { get; }

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

        public Dictionary<string, GroupViewModel> GroupDict { get; private set; }

        public InqLineModel LastPartialLineModel { get; set; }
        #endregion Public Members

        
    }
}