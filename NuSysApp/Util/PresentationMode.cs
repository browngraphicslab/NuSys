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

namespace NuSysApp
{
    /// <summary>
    /// Implements a prezi-like presentation mode.
    /// </summary>
    class PresentationMode : IDisposable, IModable
    {
        private ElementViewModel _previousNode = null;
        private ElementViewModel _nextNode = null;
        private ElementViewModel _currentNode;
        private CompositeTransform _originalTransform;
        private DispatcherTimer _timer;
        private Storyboard _storyboard;
        private SolidColorBrush _backwardColor = Application.Current.Resources["lighterredcolor"] as SolidColorBrush;
        private SolidColorBrush _forwardColor = Application.Current.Resources["color4"] as SolidColorBrush;
        private HashSet<LinkElementController> _linksUsed = new HashSet<LinkElementController>();
        public ModeType Mode { get { return ModeType.PRESENTATION;} }

        public PresentationMode(ElementViewModel start)
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
            Load();
            FullScreen();

            
            UITask.Run(async delegate
            {
                var curr = start;
                var previous = curr;
                LinkElementController linkController = null;
                while (previous != null)
                {
                    var list = new List<LinkElementController>(previous.LinkList);
                    var model = previous.Model;
                    previous = null;
                    foreach (LinkElementController link in list)
                    {
                        var linkModel = (LinkModel)link.Model;
                        if (!linkModel.IsPresentationLink)
                            continue;

                        if (link.OutElement.Model.Equals(model))
                        {
                            var l = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(item => ((ElementViewModel)item.DataContext).Model.Id == link.InElement.Model.Id);
                            previous = l?.First()?.DataContext as ElementViewModel;
                            linkController = link;
                            break;
                        }
                    }
                    if (linkController == null)
                        continue;
                    if (linkController != null && _linksUsed.Contains(linkController))
                    {
                        break;
                    }
                    _linksUsed.Add(linkController);
                    linkController.SetColor(_backwardColor);
                }
            });
        }



        private void OnTick(object sender, object e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Redraw();

        }

        /// <summary>
        /// Checks if there is a valid next node and stores it
        /// </summary>
        /// <returns></returns>
        private void Load()
        {
            var next = GetNextOrPrevNode(_currentNode, false);
            if (next == null)
            {
                _nextNode = null;
            }
            else
            {
                var nextVMList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                    item => ((ElementViewModel)item.DataContext).Model.Id == next.Id);

                _nextNode = (ElementViewModel)nextVMList.Single().DataContext;
            }

            var prev = GetNextOrPrevNode(_currentNode, true);
            if (prev == null)
            {
                _previousNode = null;
            }
            else
            {
                var prevVMList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                item => ((ElementViewModel)item.DataContext).Model.Id == prev.Id);

                _previousNode = (ElementViewModel)prevVMList.Single().DataContext;
            }
        }

        public void GoToCurrent()
        {
            FullScreen();
        }

        public bool Next()
        {
            return (_nextNode != null);
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            SetColor(GetLinkBetweenNode(_nextNode), false);
            _currentNode = _nextNode;
            Load();
            FullScreen();
        }
     
        /// <summary>
        /// Sets the color of the passing links
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="reverse"></param>
        private void SetColor(LinkElementController controller, bool reverse)
        {
            if (controller == null)
            {
                return;
            }
            if (reverse)
            {
                controller.SetColor(_forwardColor);
            }
            else
            {
                controller.SetColor(_backwardColor);
            }
            _linksUsed.Add(controller);
        }

        /// <summary>
        /// Checks if there are any previous nodes
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (_previousNode != null);
        }

        /// <summary>
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            SetColor(GetLinkBetweenNode(_previousNode), true);
            _currentNode = _previousNode;
            Load();
            FullScreen();
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
        }

        private LinkElementController GetLinkBetweenNode(ElementViewModel model)
        {
            if (model == null)
            {
                return null;
            }
            var links = _currentNode.LinkList;
            foreach (var link in links)
            {

                if (link.InElement.Model.Id == model.Model.Id || link.OutElement.Model.Id == model.Model.Id)
                {
                    return link;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds previous node for presentation if reverse is true, next node otherwise.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ElementModel GetNextOrPrevNode(ElementViewModel vm, bool reverse)
        {
            if (vm?.LinkList == null)
            {
                return null;
            }
            foreach (LinkElementController link in vm.LinkList)
            {
                var linkModel = (LinkModel)link.Model;
                if (!linkModel.IsPresentationLink)
                    continue;

                if (link.OutElement.Model.Equals(vm.Model) && reverse)
                {
                    return link.InElement.Model;
                }

                if (link.InElement.Model.Equals(vm.Model) && !reverse)
                {
                    return link.OutElement.Model;
                }

            }
            return null;
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
            _previousNode = null;
            _nextNode = null;
            _currentNode = null;
            _originalTransform = null;
            _timer = null;
            _storyboard = null;
            _backwardColor = null;
            _forwardColor = null;
            _linksUsed = null;
        }
    }
}
