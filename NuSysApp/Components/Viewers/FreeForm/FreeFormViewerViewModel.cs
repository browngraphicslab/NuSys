using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using NusysIntermediate;


namespace NuSysApp
{
    /// <summary>
    /// Models the basic Workspace and maintains a list of all atoms. 
    /// </summary>
    public class FreeFormViewerViewModel : ElementCollectionViewModel
    {
        private IEditable _currentlyEditing;
        public delegate void SelectionChangedHandler(object source);
        public event SelectionChangedHandler SelectionChanged;

        public delegate void ModeChangedEventHandler(object source, Options mode);

        public event ModeChangedEventHandler OnModeChanged;

        #region Private Members

        private CompositeTransform _compositeTransform, _fMTransform;
        private ElementViewModel _preparedElementVm;
        private List<ISelectable> _selections = new List<ISelectable>();

        #endregion Private Members

        public FreeFormViewerViewModel(ElementCollectionController controller) : base(controller)
        {
            MultiSelectedAtomViewModels = new List<ISelectable>();

            SelectionChanged += OnSelectionChanged;

            var model = controller.Model as CollectionElementModel;
            CompositeTransform = new CompositeTransform
            {
                TranslateX = -Constants.MaxCanvasSize / 2.0,
                TranslateY = -Constants.MaxCanvasSize / 2.0,
                CenterX = Constants.MaxCanvasSize / 2.0,
                CenterY = Constants.MaxCanvasSize / 2.0,
                ScaleX = 0.85,
                ScaleY = 0.85
            };


        }

        private void OnSelectionChanged(object source)
        {
            if (_currentlyEditing != null)
            {
                _currentlyEditing.IsEditing = false;
                _currentlyEditing = null;
            }
            if (Selections.Count == 1) {
                if (Selections[0] is IEditable)
                {
                    _currentlyEditing = (IEditable) Selections[0];
                    _currentlyEditing.IsEditing = true;
                }
                
                
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


        public List<ElementViewModel> AllContent
        {
            get
            {
                return Elements.Where(item => item != null).ToList();
            }
        }

        public void DeselectAll()
        {
            ClearSelection();
        }


        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void AddSelection(ISelectable selected)
        {
            selected.IsSelected = true;
          
            if (!_selections.Contains(selected))
                _selections.Add(selected);
   
            var selectedElements = AtomViewList.Where(a => a.DataContext == selected);
            if (!selectedElements.Any() || (selectedElements.First().DataContext as ElementViewModel)?.Controller.LibraryElementController == null)
                return;


            // set the z indexing if the libElemModel is a ElementViewModel
            var libElemModel = (selectedElements.First().DataContext as ElementViewModel)?.Controller.LibraryElementController.LibraryElementModel;
            // If we've selected an element view model
            if (libElemModel != null)
            {
                Canvas.SetZIndex(selectedElements.First(), NodeManipulationMode._zIndexCounter++);
            }
            else // we've selected something else
            {
                Canvas.SetZIndex(selectedElements.First(), -2);
            }
            
         

            SelectionChanged?.Invoke(this);
        }

        public void RemoveSelection(ISelectable selected)
        {
            selected.IsSelected = false;
            _selections.Remove(selected);
            SelectionChanged?.Invoke(this);
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            foreach (var selectable in _selections)
            {
                selectable.IsSelected = false;
            }
            _selections.Clear();
            SelectionChanged?.Invoke(this);
            //FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }
        

        #endregion Node Interaction

        #region Event Handlers



        #endregion Event Handlers
        #region Public Members


        public List<ISelectable> MultiSelectedAtomViewModels { get; private set; }

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
        
        /// <summary>
        /// This can be IEditable because IEditable extends ISelectable
        /// </summary>
        public List<ISelectable> Selections { get { return _selections; } } 

        #endregion Public Members


    }
}