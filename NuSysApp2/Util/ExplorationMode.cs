using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using MyToolkit.Utilities;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using NuSysApp.Util;

namespace NuSysApp2
{
    /// <summary>
    /// Implements a prezi-like exploration mode. -Zack. K.
    /// </summary>
    class ExplorationMode : IDisposable, IModable
    {
      

        private ElementViewModel _currentNode;          // current node we're on
        private RelatedListBox _relatedListBox;         // box of related elements, (i.e. click on tag)
        // stacks for moving forward and backward in exploration mode
        private Stack<ElementViewModel> _explorationHistory;
        private Stack<ElementViewModel> _explorationFuture;

        // animation variables
        private CompositeTransform _originalTransform;
        private DispatcherTimer _timer;
        private Storyboard _storyboard;

        // Required by IModeable
        public ModeType Mode { get { return ModeType.EXPLORATION;} }

        public ExplorationMode(ElementViewModel start)
        {

            Debug.Assert(start != null);

            _currentNode = start;
            _explorationHistory = new Stack<ElementViewModel>();

            // Clear the current selection in the session controller, and add the current node to it
            SessionController.Instance.ActiveFreeFormViewer.Selections.Clear();
            SelectElement(_currentNode);

            // instantiate animation variables
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            _timer.Tick += OnTick;
            _storyboard = new Storyboard();

            // copy active free from viewer transform to return back to original view upon exit
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);

            FullScreen(_currentNode);    
        }

        /// <summary>
        /// Redraws the ink on the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, object e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Redraw();

        }

        /// <summary>
        /// From the IModeable Interface
        /// Pans and zooms the screen to the Current Node
        /// </summary>
        public void GoToCurrent()
        {
            Debug.Assert(_currentNode != null, "the current node should always be set if we are in exploration mode");
            FullScreen(_currentNode);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Used to check if the exploration future stack contains any elements
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            if (_explorationFuture == null || _explorationFuture.Count == 0)
            { 
                return false;
            }
            return true;
        }

        /// <summary>
        /// From the IModeable Interface
        /// Full screen zooms into the next element from the future stack if any exists
        /// </summary>
        public void MoveToNext()
        {
            if (Next())
            {
                _explorationHistory.Push(_currentNode);
                DeselectElement(_currentNode);
                _currentNode = _explorationFuture.Pop();
                SelectElement(_currentNode);
                FullScreen(_currentNode);
            }
        }

        /// <summary>
        /// Helper method for moving from the element stored in _currentNode to the element passed in as evm
        /// </summary>
        /// <param name="evm"></param>
        public void MoveTo(ElementViewModel evm)
        {
            Debug.Assert(evm != null);

            // push the current node if it isn't null
            if (_currentNode != null)
            {
                // reset the exploration future and add to exploration history if the node we are on has changed
                if (string.Compare(_currentNode.Id, evm.Id) != 0)
                {
                    _explorationHistory.Push(_currentNode);
                    _explorationFuture = null;
                }
                DeselectElement(_currentNode);
            }
            _currentNode = evm;
            SelectElement(_currentNode);
            FullScreen(_currentNode);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Used to check if the exploration history stack contains any elements
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (_explorationHistory != null && _explorationHistory.Count > 0);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Full screen zooms into the next element from the previous stack if any exists
        /// </summary>
        public void MoveToPrevious()
        {
            // If the previous stack is empty return
            if (!Previous())
            {
                return;
            }

            // push the current Node to _exploration future
            if (_explorationFuture == null)
            {
                _explorationFuture = new Stack<ElementViewModel>();
            }
            _explorationFuture.Push(_currentNode);
            DeselectElement(_currentNode);
            // set the current node to the next element from the previous stack and zoom to it
            _currentNode = _explorationHistory.Pop();
            SelectElement(_currentNode);
            FullScreen(_currentNode);
        }


        /// <summary>
        /// From the IModeable Interface
        /// Exits presentation mode by resetting the original composite transform properties
        /// </summary>
        public void ExitMode()
        {
            AnimatePresentation(_originalTransform.ScaleX, _originalTransform.CenterX, _originalTransform.CenterY, _originalTransform.TranslateX, _originalTransform.TranslateY);
           HideRelatedListBox();
        }

        /// <summary>
        /// Hides the related list box by removing it from the sessionview
        /// </summary>
        public void HideRelatedListBox()
        {
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(_relatedListBox);
            _relatedListBox = null;
        }

        /// <summary>
        /// Shows the box with elements related--that is, elements with the same tag
        /// </summary>
        /// <param name="tag"></param>
        internal void ShowRelatedElements(string tag)
        {
            // Box not on session view, so instatiate a new one and add it
            if (_relatedListBox == null)
            {
                _relatedListBox = new RelatedListBox(tag);
                SessionController.Instance.SessionView.MainCanvas.Children.Add(_relatedListBox);
                Canvas.SetTop(_relatedListBox, 300);
                Canvas.SetLeft(_relatedListBox,200);
            }
            // Box already on session view, so update the contents
            else
            {
                _relatedListBox.UpdateTag(tag);
            }
        }

        /// <summary>
        /// Will make a full screen appeareance for the passed in element view model. Use this for presentation view.
        /// </summary>
        /// <param name="e"></param>
        private void FullScreen(ElementViewModel elementToBeFullScreened)
        {
            Debug.Assert(elementToBeFullScreened != null);

            // Determines tag adjustment by getting the height of the tag container from the view
            double tagAdjustment = 0;

            var view = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                    item => (item.DataContext as ElementViewModel)?.Model.Id == elementToBeFullScreened.Id);

            if (view.Count() == 0)
            {
                return;
            }
            var found = view.Single().FindName("nodeTpl");
            if (found != null)
            {
                var ss = (NodeTemplate)found;
                tagAdjustment = ss.tags.ActualHeight;
            }


            // Define some variables that will be used in future translation/scaling
            var nodeWidth = elementToBeFullScreened.Width;
            var nodeHeight = elementToBeFullScreened.Height + 40 + tagAdjustment; // 40 for title adjustment
            var sv = SessionController.Instance.SessionView;
            var x = elementToBeFullScreened.Model.X + nodeWidth / 2;
            var y = elementToBeFullScreened.Model.Y - 40 + nodeHeight / 2;
            var widthAdjustment = sv.ActualWidth / 2;
            var heightAdjustment = sv.ActualHeight / 2;

            // Reset the scaling and translate the free form viewer so that the passed in element is at the center
            var scaleX = 1;
            var scaleY = 1;
            var translateX = widthAdjustment - x;
            var translateY = heightAdjustment - y;
            double scale;


            // Scale based on the width and height proportions of the current node
            if (nodeWidth > nodeHeight)
            {
                scale = sv.ActualWidth / nodeWidth;
                if (nodeWidth - nodeHeight <= 20)
                    scale = scale * .50;
                else
                    scale = scale * .55;
            }


            else
            {
                scale = sv.ActualHeight / nodeHeight;
                scale = scale * .7;
            }


            // Call a helper method to set up the animation
            AnimatePresentation(scale, x, y, translateX, translateY);

        }


        /// <summary>
        /// Animates the presentation by creating various DoubleAnimations, adding then to the storyboard,
        /// and finally starting the story board
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="translateX"></param>
        /// <param name="translateY"></param>
        private void AnimatePresentation(double scale, double x, double y, double translateX, double translateY)
        {

            var duration = new Duration(TimeSpan.FromSeconds(1));
            _storyboard.Stop();
            _storyboard = new Storyboard();


            _storyboard.Duration = duration;

            // Create a DoubleAnimation for each property to animate
            var scaleAnimationX = MakeAnimationElement(scale, "ScaleX", duration);
            var scaleAnimationY = MakeAnimationElement(scale, "ScaleY", duration);
            var centerAnimationX = MakeAnimationElement(x, "CenterX", duration);
            var centerAnimationY = MakeAnimationElement(y, "CenterY", duration);
            var translateAnimationX = MakeAnimationElement(translateX, "TranslateX", duration);
            var translateAnimationY = MakeAnimationElement(translateY, "TranslateY", duration);
            var animationList = new List<DoubleAnimation>(new DoubleAnimation[] { scaleAnimationX, scaleAnimationY, centerAnimationX, centerAnimationY, translateAnimationX, translateAnimationY });

            // Add each animation to the storyboard
            foreach (var anim in animationList)
            {
                _storyboard.Children.Add(anim);
            }

            // Saves the final product as a composite transform and updates other transforms based on this
            var tt = new CompositeTransform
            {
                TranslateX = translateX,
                TranslateY = translateY,
                ScaleX = scale,
                ScaleY = scale,
                CenterX = x,
                CenterY = y
            };
            SessionController.Instance.SessionView.FreeFormViewer.PanZoom.UpdateTempTransform(tt);
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Transform = tt;
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Redraw();

            // Begin the animation.
            _storyboard.Begin();

        }


        /// <summary>
        /// Produces an animation element to animate a certain property transition using a storyboard
        /// </summary>
        /// <param name="to"></param>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="transform"></param>
        /// <param name="dependent"></param>
        /// <returns></returns>
        private DoubleAnimation MakeAnimationElement(double to, String name, Duration duration,
            CompositeTransform transform = null, bool dependent = false)
        {

            if (transform == null)
            {
                transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            }

            var toReturn = new DoubleAnimation();
            toReturn.EnableDependentAnimation = true;
            toReturn.Duration = duration;
            Storyboard.SetTarget(toReturn, transform);
            Storyboard.SetTargetProperty(toReturn, name);
            toReturn.To = to;
            toReturn.EasingFunction = new QuadraticEase();
            return toReturn;
        }

        /// <summary>
        /// Produces and returns a copy of the passed in composite transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private CompositeTransform MakeShallowCopy(CompositeTransform transform)
        {
            var newTransform = new CompositeTransform();
            newTransform.CenterX = transform.CenterX;
            newTransform.CenterY = transform.CenterY;
            newTransform.ScaleX = transform.ScaleX;
            newTransform.ScaleY = transform.ScaleY;
            newTransform.TranslateX = transform.TranslateX;
            newTransform.TranslateY = transform.TranslateY;
            newTransform.Rotation = transform.Rotation;
            newTransform.SkewX = transform.SkewX;
            newTransform.SkewY = transform.SkewY;
            return newTransform;
        }

        /// <summary>
        /// Removes all previously attached event listeners and object references, frees previously allocated memory
        /// </summary>
        public void Dispose()
        {
           
            _currentNode = null;
            _originalTransform = null;
            _timer = null;
            _storyboard = null;
        }


        /// <summary>
        /// Helper for highlighting an element view model and adding it to the active free form view selections
        /// </summary>
        /// <param name="toBeSelected"></param>
        private void SelectElement(ElementViewModel toBeSelected)
        {
            Debug.Assert(toBeSelected != null);

            // if the session controller currently doesn't have the current element selected, add it
            if (!SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(toBeSelected))
            {
                SessionController.Instance.ActiveFreeFormViewer.Selections.Add(toBeSelected);
            }
            
            // highlight the selection
            toBeSelected.IsSelected = true;
        }

        /// <summary>
        /// Helper for unhighlighting an element view model and removing it from the active free form view selections
        /// </summary>
        /// <param name="toBeDeselected"></param>
        private void DeselectElement(ElementViewModel toBeDeselected)
        {
            Debug.Assert(toBeDeselected != null);

            // if the session controller currently has the element selected, remove it
            if (SessionController.Instance.ActiveFreeFormViewer.Selections.Contains(toBeDeselected))
            {
                SessionController.Instance.ActiveFreeFormViewer.Selections.Remove(toBeDeselected);
            }

            // unhighlight the selection
            toBeDeselected.IsSelected = false;
        }
    }

    public enum ModeType
    {
        EXPLORATION,PRESENTATION
    }

  

}
