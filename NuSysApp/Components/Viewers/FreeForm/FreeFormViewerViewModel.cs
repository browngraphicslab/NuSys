﻿using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class FreeFormViewerViewModel : ElementCollectionViewModel
    {
        #region Private Members

        private CompositeTransform _compositeTransform, _fMTransform;
        private ElementViewModel _preparedElementVm;

        #endregion Private Members

        public FreeFormViewerViewModel(ElementCollectionController controller) : base(controller)
        {
            MultiSelectedAtomViewModels = new List<ElementViewModel>();
            SelectedElementViewModel = null;
            FMTransform = new CompositeTransform();

            var model = controller.Model;

            if (model.GetMetaData("locationX") == null)
            {
                CompositeTransform = new CompositeTransform
                {
                    ScaleX = 1,
                    ScaleY = 1,
                    TranslateX = -Constants.MaxCanvasSize / 2.0,
                    TranslateY = -Constants.MaxCanvasSize / 2.0,
                    CenterX = -Constants.MaxCanvasSize / 2.0,
                    CenterY = -Constants.MaxCanvasSize / 2.0
                };

            }
            else {
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
        }

        
        
        public void MoveToNode(string id)
        {
            // TODO: refactor
            /*
            var node = (ElementModel)SessionController.Instance.IdToSendables[id];
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
            return false;
            // TODO: refactor
            /*
            var nodes = new List<ElementViewModel>();
            var links = new List<LinkViewModel>();
            foreach (var node2 in AtomViewList.Where(a => a.DataContext is ElementViewModel))
            {
                var rect1 = Geometry.InqToBoudingRect(inq);
                var rect2 = Geometry.NodeToBoudingRect(node2.DataContext as ElementViewModel);
                rect1.Intersect(rect2);//stores intersection rectangle in rect1
                if (!rect1.IsEmpty)
                {
                    nodes.Add(node2.DataContext as ElementViewModel);
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
            */

        }


        public void DeleteLink(LinkViewModel link)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(link.Id));
        }

        public void DeleteNode(ElementViewModel node)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(node.Id));
        }


        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelection(ElementViewModel selected)
        {   
            ClearSelection();
            selected.SetSelected(true);
            SelectedElementViewModel = selected;
            var f = AtomViewList.Where(a => a.DataContext == selected).First();
            Canvas.SetZIndex(f, NodeManipulationMode._zIndexCounter++);
        }





        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            if (SelectedElementViewModel == null) return;
            SelectedElementViewModel.SetSelected(false);
            SelectedElementViewModel = null;
        }

        


        #endregion Node Interaction

        #region Event Handlers



        #endregion Event Handlers
        #region Public Members


        public ElementViewModel SelectedElementViewModel { get; private set; }

        public List<ElementViewModel> MultiSelectedAtomViewModels { get; private set; }

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