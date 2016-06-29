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

namespace NuSysApp.Util
{
    /// <summary>
    /// Implements a prezi-like exploration mode.
    /// </summary>
    class ExplorationMode : IDisposable, IModable
    {
      
        private ElementViewModel _currentNode;
        private CompositeTransform _originalTransform;
        private DispatcherTimer _timer;
        private Storyboard _storyboard;
        private SolidColorBrush _backwardColor = Application.Current.Resources["lighterredcolor"] as SolidColorBrush;
        private SolidColorBrush _forwardColor = Application.Current.Resources["color4"] as SolidColorBrush;
        private HashSet<LinkElementController> _linksUsed = new HashSet<LinkElementController>();
        public ElementViewModel CurrentNode { get { return _currentNode; } }
        private RelatedListBox _relatedListBox;
        private Stack<ElementViewModel> _explorationHistory;
        private Stack<ElementViewModel> _explorationFuture;

        public ModeType Mode { get { return ModeType.EXPLORATION;} }

        public ExplorationMode(ElementViewModel start)
        {

            if (start == null)
            {
                return;
            }
            
          
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1);
            _timer.Tick += OnTick;
            _storyboard = new Storyboard();
            _currentNode = start;
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);
            _explorationHistory = new Stack<ElementViewModel>();
            _explorationHistory.Push(start);
            FullScreen();    
        }



        private void OnTick(object sender, object e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Redraw();

        }  

        public void GoToCurrent()
        {
            FullScreen();
        }

        public bool Next()
        {
            if (_explorationFuture == null || _explorationFuture.Count == 0)
            { 
                return false;
            }
            return true;
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            if (Next())
            {
                _explorationHistory.Push(_currentNode);
                _currentNode = _explorationFuture.Pop();   
                FullScreen();
            }
        }

        public void MoveTo(ElementViewModel evm)
        {
            // push the current node if it isn't null
            if (_currentNode != null)
            {
                _explorationHistory.Push(CurrentNode);
                _currentNode.IsSelected = false;
            }
            // reset the exploration future
            _explorationFuture = null;
            _currentNode = evm;
            _currentNode.IsSelected = true;
            FullScreen();
        }

        /// <summary>
        /// Checks if there are any previous nodes
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return _explorationHistory.Count > 1;
        }

        /// <summary>
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            if (Previous())
            {

                if (_explorationFuture == null)
                {
                    _explorationFuture = new Stack<ElementViewModel>();
                }

                _explorationFuture.Push(_currentNode);

                _currentNode = _explorationHistory.Pop();
                FullScreen();

                

                
            }
        }

        /// <summary>
        /// Exits presentation mode by resetting the original composite transform properties
        /// </summary>
        public void ExitMode()
        {
            foreach (var link in _linksUsed)
            {
                if (link != null)
                {
                    link.SetColor(Application.Current.Resources["color4"] as SolidColorBrush);
                }
            }
            AnimatePresentation(_originalTransform.ScaleX, _originalTransform.CenterX, _originalTransform.CenterY, _originalTransform.TranslateX, _originalTransform.TranslateY);
           this.HideRelatedListBox();
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
        /// Will make a full screen appeareance for the passed in element view model. Use this for presentation view.
        /// </summary>
        /// <param name="e"></param>

        private void FullScreen()
        {
            if (_currentNode == null)
                return;

            // Determines tag adjustment by getting the height of the tag container from the view
            double tagAdjustment = 0;

            if (_currentNode == null)
            {
                return;
            }
            var view = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                    item => ((ElementViewModel)item.DataContext).Model.Id == _currentNode.Id);

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
            var nodeWidth = _currentNode.Width;
            var nodeHeight = _currentNode.Height + 40 + tagAdjustment; // 40 for title adjustment
            var sv = SessionController.Instance.SessionView;
            var x = _currentNode.Model.X + nodeWidth / 2;
            var y = _currentNode.Model.Y - 40 + nodeHeight / 2;
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
        /// Explore a link by going to the other side. Using the current node being explored (which is stored as CurrentNode), we can find the node opposite the link.
        /// </summary>
        /// <param name="vm"></param>
        internal void ExploreLink(LinkViewModel vm)
        {
            // We will use the in atom or the out atom ID to decide which side of the link to go to
            string id = vm.LinkModel.InAtomId;
            if (vm.LinkModel.InAtomId.Equals(_currentNode.Id))
            {
                id = vm.LinkModel.OutAtomId;
            }

            // Find a list of element view models that have that id
            var vms =
                    SessionController.Instance.ActiveFreeFormViewer.AllContent.Where(
                        item => (item.Id == id));

            // Use a helper method to the view model that is connected to this link
            var opposite = vms.Single(item => this.EvmInLink(item, vm.LinkModel.Id));

            // Move to the found element view model 
            this.MoveTo(opposite);

            // Make sure the link is deselected
            vm.IsSelected = false;
        
            vm.Color = Application.Current.Resources["color2"] as SolidColorBrush;
            if (vm.LinkModel.IsPresentationLink)
            {
                vm.Color = Application.Current.Resources["color4"] as SolidColorBrush;
            }
        }

        /// <summary>
        /// Returns if the passed in element view model is connected to the link, using the passed in ID
        /// </summary>
        /// <param name="evm"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        private bool EvmInLink(ElementViewModel evm, string linkId)
        {
            // Look thru the evm's links
            foreach (var link in evm.LinkList)
            {
                // Return true if there is a link id match
                if (link.Model.Id == linkId)
                {
                    return true;
                }
            }
            return false;
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
            _backwardColor = null;
            _forwardColor = null;
            _linksUsed = null;
        }
    }

    public enum ModeType
    {
        EXPLORATION,PRESENTATION
    }

  

}
