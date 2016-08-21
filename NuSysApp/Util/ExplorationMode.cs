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
using NusysIntermediate;
using NuSysApp.Util;

namespace NuSysApp
{
    /// <summary>
    /// Implements a prezi-like exploration mode. -Zack. K.
    /// </summary>
    class ExplorationMode : IDisposable, IModable
    {
      
        /// <summary>
        /// The ElementViewModel we are currently exploring
        /// </summary>
        private ElementViewModel _currentNode;

        /// <summary>
        /// Box containing a list of related elements
        /// </summary>
        private RelatedListBox _relatedListBox;

        /// <summary>
        /// A stack of ElementViewModels we have already explored, for the back button.
        /// </summary>
        private Stack<ElementViewModel> _explorationHistory;

        /// <summary>
        /// A stack of ElementViewModels which have been popped off the _explorationHistory, for the forward button
        /// </summary>
        private Stack<ElementViewModel> _explorationFuture;

        // animation variables
        private CompositeTransform _originalTransform;
        private DispatcherTimer _timer;
        private Storyboard _flyOutStoryBoard;
        private Storyboard _flyInStoryBoard;
        private CompositeTransform _endTransform;

        // Required by IModeable
        public ModeType Mode { get { return ModeType.EXPLORATION;} }

        public ExplorationMode(ElementViewModel start=null)
        {

            _currentNode = start;
            _explorationHistory = new Stack<ElementViewModel>();

            // Clear the current selection in the session controller, and add the current node to it
            SessionController.Instance.ActiveFreeFormViewer.Selections.Clear();
            
            // instantiate a timer that redraws the inqCanvas
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            _timer.Tick += OnTick;

            // instantiate the storyboards so they don't have null exceptions on the first run
            _flyInStoryBoard = new Storyboard();
            _flyOutStoryBoard = new Storyboard();

            // copy active free from viewer transform to return back to original view upon exit
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);

            if (start != null)
            {
                SelectElement(_currentNode);
                FullScreen(_currentNode);
            }
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

            // don't do anything if we are already a the node
            if (_currentNode == evm)
            {
                return;
            }

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
            _timer.Tick -= OnTick;
            HideRelatedListBox();
            SessionController.Instance.SwitchMode(Options.SelectNode);
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
            var translateX = widthAdjustment - x;
            var translateY = heightAdjustment - y;
            double scale;


            // Scale based on the width and height proportions of the current node
            if (nodeWidth > nodeHeight)
            {
                scale = sv.ActualWidth / nodeWidth;
                if (nodeWidth - nodeHeight <= Constants.MinNodeSize)
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
        /// <param name="x">The workspace coordinate of the center of the element view model we are moving to</param>
        /// <param name="y">The workspace coordinate of the center of the element view model we are moving to</param>
        /// <param name="translateX">The translation from the upper left corner of the workspace to the upper left corner of the camera</param>
        /// <param name="translateY">The translation from the upper left corner of the workspace to the upper left corner of the camera</param>
        private void AnimatePresentation(double scale, double x, double y, double translateX, double translateY)
        {
            // stop any current animations
            _flyOutStoryBoard.Stop();
            _flyInStoryBoard.Stop();

            // get the current position and scale values from the ActiveFreeFormViewerTransform
            var currentTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            var currentX = currentTransform.CenterX;
            var currentY = currentTransform.CenterY;
            var currentScale = currentTransform.ScaleX;
            var currentTranslateX = currentTransform.TranslateX;
            var currentTranslateY = currentTransform.TranslateY;

            // Get the distance we are going to travel
            var dist = Math.Sqrt(Math.Pow(Math.Abs(currentX - x), 2) + Math.Pow(Math.Abs(currentY - y), 2));

            // set the scale of the animation based on the distance
            var halfScale = Math.Pow(dist, 1.0 / 3.0) / Math.Pow(dist, 1.0 / 2.0);

            // set the duration of the outAnimation based on the distance
            var outDuration = new Duration(TimeSpan.FromSeconds(Math.Log(Math.Abs(dist) <= 0 ? dist + .0001 : dist, 5 ) / 5)); // we check that distance isn't exactly zero so log doesn't fail

            // get the center point of the halfway location we are going to
            var halfCenterX = (currentX + x )/2.0;
            var halfCenterY = (currentY + y) / 2.0;

            // get the translation point of the halfway location we are going to
            var halfTransX = (translateX + currentTranslateX) /2.0;
            var halfTransY = (translateY + currentTranslateY) / 2.0;
            
            // create the flyout animation elements
            var outScaleAnimationX = MakeAnimationElement(halfScale, "ScaleX", outDuration, easeMode: EasingMode.EaseOut);
            var outScaleAnimationY = MakeAnimationElement(halfScale, "ScaleY", outDuration, easeMode: EasingMode.EaseOut);
            var outCenterAnimationX = MakeAnimationElement(halfCenterX, "CenterX", outDuration, easeMode: EasingMode.EaseOut);
            var outCenterAnimationY = MakeAnimationElement(halfCenterY, "CenterY", outDuration, easeMode: EasingMode.EaseOut);
            var outTranslateAnimationX = MakeAnimationElement(halfTransX, "TranslateX", outDuration, easeMode: EasingMode.EaseOut);
            var outTranslateAnimationY = MakeAnimationElement(halfTransY, "TranslateY", outDuration, easeMode: EasingMode.EaseOut);
            var outAnimationList = new List<DoubleAnimation>
            {
                outScaleAnimationX,
                outScaleAnimationY,
                outCenterAnimationX,
                outCenterAnimationY,
                outTranslateAnimationX,
                outTranslateAnimationY
            };

            // create the flyout storyboard
            _flyOutStoryBoard = new Storyboard();
            // set the duration of the flyout storyboard to duration
            _flyOutStoryBoard.Duration = outDuration;

            // Add each animation to the storyboard
            foreach (var animation in outAnimationList)
            {
                _flyOutStoryBoard.Children.Add(animation);
            }

            // set the duration of the inAnimation
            var inDuration = new Duration(TimeSpan.FromSeconds(Math.Log(dist, 5) / 5));

            // Create the flyin animation elements
            var inScaleAnimationX = MakeAnimationElement(scale, "ScaleX", inDuration, easeMode:EasingMode.EaseInOut);
            var inScaleAnimationY = MakeAnimationElement(scale, "ScaleY", inDuration, easeMode: EasingMode.EaseInOut);
            var inCenterAnimationX = MakeAnimationElement(x, "CenterX", inDuration, easeMode: EasingMode.EaseInOut);
            var inCenterAnimationY = MakeAnimationElement(y, "CenterY", inDuration, easeMode: EasingMode.EaseInOut);
            var inTranslateAnimationX = MakeAnimationElement(translateX, "TranslateX", inDuration, easeMode: EasingMode.EaseInOut);
            var inTranslateAnimationY = MakeAnimationElement(translateY, "TranslateY", inDuration, easeMode: EasingMode.EaseInOut);
            var inAnimationList = new List<DoubleAnimation>{ inScaleAnimationX, inScaleAnimationY, inCenterAnimationX, inCenterAnimationY, inTranslateAnimationX, inTranslateAnimationY };

            // create the flyin storyboard
            _flyInStoryBoard = new Storyboard();
            // set the duration of the flyin storyboard to duration
            _flyInStoryBoard.Duration = inDuration;

            // Add each animation to the storyboard
            foreach (var animation in inAnimationList)
            {
                _flyInStoryBoard.Children.Add(animation);
            }

            // Save the transform information for the midpoint and update the panzoom transform, for culling composition rendering or something
            var midTransform = new CompositeTransform
            {
                TranslateX = halfTransX,
                TranslateY = halfTransY,
                ScaleX = halfScale,
                ScaleY = halfScale,
                CenterX = halfCenterX,
                CenterY = halfCenterY
            };
            SessionController.Instance.SessionView.FreeFormViewer.PanZoom.UpdateTempTransform(midTransform);
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Transform = midTransform;

            // Save the transform information for the end point we will update the pan zoom transform when the flyOutStoryBoard is completed
            _endTransform = new CompositeTransform
            {
                TranslateX = translateX,
                TranslateY = translateY,
                ScaleX = scale,
                ScaleY = scale,
                CenterX = x,
                CenterY = y
            };

            // Begin the animation.
            _flyOutStoryBoard.Begin();
            // start the second animation when the first one completes
            _flyOutStoryBoard.Completed += _flyOutStoryBoard_Completed;
        }

        private void _flyOutStoryBoard_Completed(object sender, object e)
        {
            _flyOutStoryBoard.Completed -= _flyOutStoryBoard_Completed;
            SessionController.Instance.SessionView.FreeFormViewer.PanZoom.UpdateTempTransform(_endTransform);
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Transform = _endTransform;
            _flyInStoryBoard.Begin();
        }


        /// <summary>
        /// Produces an animation element to animate a certain property transition using a storyboard
        /// </summary>
        /// <param name="to"></param>
        /// <param name="transformPath">The path on the transform to the property we are animating</param>
        /// <param name="duration">Gets or sets the length of time for which this timeline plays, not counting repetitions.</param>
        /// <param name="transform"></param>
        /// <param name="dependent">If an animation will cause the ui thread to slow down, this has to be set to true so the animation works</param>
        /// <param name="easeMode">Sets the easing mode of the animation</param>
        /// <returns></returns>
        private DoubleAnimation MakeAnimationElement(double to, String transformPath, Duration duration,
            CompositeTransform transform = null, bool dependent = false, EasingMode easeMode = EasingMode.EaseInOut)
        {
            // allows us to avoid passing in the transform on each call, the default transform is the ActiveFreeFormViewer
            if (transform == null)
            {
                transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            }

            // create a newAnimation and set properties on it
            var newAnimation = new DoubleAnimation();

            // this allows the animation to consist of things that might slow down ui thread
            newAnimation.EnableDependentAnimation = dependent;

            // Gets or sets the length of time for which this timeline plays, not counting repetitions
            newAnimation.Duration = duration;

            // Set the target of the animation to the transform
            Storyboard.SetTarget(newAnimation, transform);

            // Set the target property of the animation to the path of what's being animated
            Storyboard.SetTargetProperty(newAnimation, transformPath);

            // set the value we are animating the double ot
            newAnimation.To = to;

            // set the easing mode of the animation
            newAnimation.EasingFunction = new SineEase { EasingMode = easeMode};
            return newAnimation;
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
            _flyOutStoryBoard = null;
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
