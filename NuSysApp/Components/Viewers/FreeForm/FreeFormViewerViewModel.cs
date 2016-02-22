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
using Windows.Devices.WiFiDirect;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class FreeFormViewerViewModel : ElementInstanceCollectionViewModel
    {
        #region Private Members

        private CompositeTransform _compositeTransform, _fMTransform;
        private ElementInstanceViewModel _preparedElementInstanceVm;

        #endregion Private Members

        public FreeFormViewerViewModel(ElementInstanceController controller) : base(controller)
        {
            GroupDict = new Dictionary<string, ElementInstanceCollectionViewModel>();
            MultiSelectedAtomViewModels = new List<ElementInstanceViewModel>();
            SelectedElementInstanceViewModel = null;

            var model = controller.Model;

            if (model.GetMetaData("locationX") == null)
            {
                CompositeTransform = new CompositeTransform
                {
                    ScaleX = 1,
                    ScaleY = 1,
                    TranslateX = -Constants.MaxCanvasSize/2.0,
                    TranslateY = -Constants.MaxCanvasSize/2.0,
                    CenterX = -Constants.MaxCanvasSize/2.0,
                    CenterY = -Constants.MaxCanvasSize/2.0
                };

            } else { 
                CompositeTransform = new CompositeTransform
                    {
                        TranslateX = (double)model.GetMetaData("locationX"),
                        TranslateY = (double)model.GetMetaData("locationY"),
                        CenterX = (double)model.GetMetaData("centerX"),
                        CenterY = (double)model.GetMetaData("centerY"),
                        ScaleX = (double)model.GetMetaData("zoom"),
                        ScaleY = (double)model.GetMetaData("zoom")
                    };
            }
         
            FMTransform = new CompositeTransform();
        }

        public void MoveToNode(string id)
        {
            // TODO: refactor
            /*
            var node = (ElementInstanceModel)SessionController.Instance.IdToSendables[id];
            var tp = new Point(-node.X, -node.Y);
            var np = CompositeTransform.Inverse.TransformPoint(tp);
            var ns = new Size(node.Width, node.Height);
            CompositeTransform.ScaleX = 1;
            CompositeTransform.ScaleY = 1;
            CompositeTransform.TranslateX = tp.X + (SessionController.Instance.SessionView.ActualWidth - node.Width) / 2;
            CompositeTransform.TranslateY = tp.Y + (SessionController.Instance.SessionView.ActualHeight - node.Height) / 2;
            */
        }

        #region Node Interaction

        public bool CheckForInkNodeIntersection(InqLineModel inq)
        {
            var nodes = new List<ElementInstanceViewModel>();
            var links = new List<LinkViewModel>();
            foreach (var node2 in AtomViewList.Where(a => a.DataContext is ElementInstanceViewModel))
            {
                var rect1 = Geometry.InqToBoudingRect(inq);
                var rect2 = Geometry.NodeToBoudingRect(node2.DataContext as ElementInstanceViewModel);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (!rect1.IsEmpty)
                {
                    nodes.Add(node2.DataContext as ElementInstanceViewModel);
                }
            }
            foreach (var link in AtomViewList.Where(a => a.DataContext is LinkViewModel))
            {
                var rect1 = Geometry.InqToBoudingRect(inq);
                var LinkLine = (link.DataContext as LinkViewModel).LineRepresentation;
                var rectLines = Geometry.RectToLineSegment(rect1);

                foreach (var line in rectLines)
                {
                    if (Geometry.LinesIntersect(LinkLine, line))
                    {
                        links.Add(link.DataContext as LinkViewModel);
                        break;
                    }
                }
            }



            foreach (var link in links)
            {
                var LinkLine = link.LineRepresentation;
                Action checkLines = delegate
                {
                    for (int i = 0; i < inq.Points.Count - 1; i++)
                    {
                        var rect1 = new Rect(new Point(inq.Points[i].X * Constants.MaxCanvasSize, inq.Points[i].Y * Constants.MaxCanvasSize), new Point(inq.Points[i + 1].X * Constants.MaxCanvasSize, inq.Points[i + 1].Y * Constants.MaxCanvasSize));
                        var rectLines = Geometry.RectToLineSegment(rect1);
                        if (rectLines.Any(line => Geometry.LinesIntersect(LinkLine, line)))
                        {
                            DeleteLink(link);
                            return;
                        }
                    }
                };
                checkLines();
            }
            foreach (var node in nodes)
            {
                Action checkLines = delegate
                {

                    var nodeRect = Geometry.NodeToBoudingRect(node);
                    for (int i = 0; i < inq.Points.Count - 1; i++)
                    {
                        var rect1 = new Rect(new Point(inq.Points[i].X * Constants.MaxCanvasSize, inq.Points[i].Y * Constants.MaxCanvasSize), new Point(inq.Points[i + 1].X * Constants.MaxCanvasSize, inq.Points[i + 1].Y * Constants.MaxCanvasSize));
                        rect1.Intersect(nodeRect);
                        if (!rect1.IsEmpty)
                        {
                            foreach(var nodelink in node.LinkList)
                            {
                                DeleteLink(nodelink);
                            }
                            DeleteNode(node);
                            return;
                        }
                    }
                };
                checkLines();

            }
            return true;
        }


        public void DeleteLink(LinkViewModel link)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(link.Id));
        }

        public void DeleteNode(ElementInstanceViewModel node)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(node.Id));
        }


        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(ElementInstanceViewModel selected)
        {
            List<string> locks = new List<string>();
            locks.Add(selected.Model.Id);
            //NetworkConnector.Instance.CheckLocks(locks);

            if (SelectedElementInstanceViewModel == null)
            {
                SelectedElementInstanceViewModel = selected;
                return;
            }
            // //NetworkConnector.Instance.RequestMakeLinq(SelectedAtomViewModel.ID, selected.ID);
            selected.SetSelected(false);
            SelectedElementInstanceViewModel.SetSelected(false);
            SelectedElementInstanceViewModel = null;
            ClearSelection();
        }

        public void SetMultiSelection(ElementInstanceViewModel selected)
        {
            if (!selected.IsMultiSelected)
            {
                selected.IsMultiSelected = true;
                //NetworkConnector.Instance.RequestLock(selected.Id);
                this.MultiSelectedAtomViewModels.Add(selected);
            }
        }

        public void MoveMultiSelection(ElementInstanceViewModel sender, double x, double y)
        {
            foreach (var atom in MultiSelectedAtomViewModels)
            {
                var node = atom as ElementInstanceViewModel;
                if (node != null && atom != sender)
                {
                    node.IsMultiSelected = false;
                    node.Controller.Translate(x, y);
                    node.IsMultiSelected = true;
                }
            }
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            //NetworkConnector.Instance.ReturnAllLocks();
            if (SelectedElementInstanceViewModel == null) return;
            SelectedElementInstanceViewModel.SetSelected(false);
            SelectedElementInstanceViewModel = null;
        }


        private delegate void AddInk(string s);

        public async void PromoteInk(Rect nodeBounds, List<InqLineModel> linesToPromote)
        {
            //TODO: refactor

            /*

            var dict = new Dictionary<string, object>();
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
                            ////NetworkConnector.Instance.RequestLock(v.ID);
                            //  //NetworkConnector.Instance.RequestFinalizeGlobalInk(model.Id, v.InqCanvas.Id, model.GetString());
                            //is the model being deleted and then trying to be added? is the canvas fully there when we try to add?
                        });
                    }
                }
            };
            Action<string> a = new Action<string>(add);
            //await NetworkConnector.Instance.RequestMakeNode(nodeBounds.X.ToString(), nodeBounds.Y.ToString(), NodeType.Text.ToString(), null, null, dict, a);
        
            */
        }

        public void ClearMultiSelection()
        {
            //NetworkConnector.Instance.ReturnAllLocks();
            foreach (var avm in MultiSelectedAtomViewModels)
            {
                //NetworkConnector.Instance.RequestReturnLock(avm.Id);
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
                var nodeModel = (ElementInstanceModel)t.Model;
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

            var node1 = (ElementInstanceModel)MultiSelectedAtomViewModels[0].Model;
            //  //await NetworkConnector.Instance.RequestMakeEmptyGroup(minX.ToString(), minY.ToString(),null, props, new Action<string>(del));
        }


        #endregion Node Interaction

        #region Event Handlers



        #endregion Event Handlers
        #region Public Members


        public ElementInstanceViewModel SelectedElementInstanceViewModel { get; private set; }

        public List<ElementInstanceViewModel> MultiSelectedAtomViewModels { get; private set; }

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

        public override void Remove() { }
        public override void UpdateAnchor() { }

        public Dictionary<string, ElementInstanceCollectionViewModel> GroupDict { get; private set; }

        public InqLineModel LastPartialLineModel { get; set; }
        #endregion Public Members


    }
}