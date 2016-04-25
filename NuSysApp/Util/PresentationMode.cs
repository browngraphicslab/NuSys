using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using MyToolkit.Utilities;
using Windows.UI;

namespace NuSysApp.Util
{
    /// <summary>
    /// Implements PresentationMode for nodes.
    /// </summary>
    class PresentationMode
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
        public PresentationMode(ElementViewModel start)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _timer.Tick += OnTick;

            _storyboard = new Storyboard();
            _storyboard.Completed += OnAnimationCompleted;

            _currentNode = start;
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);
            Load();
            FullScreen();
            UITask.Run(async delegate {
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
                    _linksUsed.Add(linkController);
                    linkController.SetColor(_backwardColor);
                }
            });
        }


        private void OnAnimationCompleted(object sender, object e)
        {
            _timer.Stop();
        }

        private void OnTick(object sender, object e)
        {
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
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
            } else
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
        /*
         * sets the color of the passing links
         */
        private void SetColor(LinkElementController controller, bool reverse)
        {
            if(controller == null)
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

        public void ExitMode()
        {
            foreach(var link in _linksUsed)
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
            if(model == null)
            {
                return null;
            }
            var links = _currentNode.LinkList;
            foreach(var link in links)
            {
                //var list = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(item => ((ElementViewModel)item.DataContext).Model.Id == link.InElement.Model.Id || ((ElementViewModel)item.DataContext).Model.Id == link.OutElement.Model.Id);
                //var l = list.First()?.DataContext as LinkViewModel;
                //if (l != null)
                //{
                //    return l;
                //}
                if(link.InElement.Model.Id == model.Model.Id || link.OutElement.Model.Id == model.Model.Id)
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
            if (vm?.LinkList == null) {
                return null;
            }
            foreach (LinkElementController link in vm.LinkList)
            {
                var linkModel = (LinkModel) link.Model;
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

            if(_currentNode == null)
            {
                return;
            }
            var view = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                    item => ((ElementViewModel)item.DataContext).Model.Id == _currentNode.Id);

            if(view.Count() == 0)
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

        private void AnimatePresentation(double scale, double x , double y, double translateX, double translateY)
        {           
            // Create a duration of 2 seconds.
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
            _timer.Start();
       
            _storyboard.Stop();
            _storyboard.Children.Clear();

            _storyboard.Duration = duration;
            DoubleAnimation scaleAnimationX = MakeAnimationElement(scale, "ScaleX", duration);
            DoubleAnimation scaleAnimationY = MakeAnimationElement(scale, "ScaleY", duration);
            DoubleAnimation centerAnimationX = MakeAnimationElement(x, "CenterX", duration);
            DoubleAnimation centerAnimationY = MakeAnimationElement(y, "CenterY", duration);
            DoubleAnimation translateAnimationX = MakeAnimationElement(translateX, "TranslateX", duration);
            DoubleAnimation translateAnimationY = MakeAnimationElement(translateY, "TranslateY", duration);

            _storyboard.Children.Add(scaleAnimationX);
            _storyboard.Children.Add(scaleAnimationY);
            _storyboard.Children.Add(centerAnimationX);
            _storyboard.Children.Add(centerAnimationY);
            _storyboard.Children.Add(translateAnimationX);
            _storyboard.Children.Add(translateAnimationY);

            // Make the Storyboard a resource.
            SessionController.Instance.SessionView.Resources.Add("PresentationStoryboard", _storyboard);

            // Begin the animation.
            _storyboard.Begin();
            SessionController.Instance.SessionView.Resources.Remove("PresentationStoryboard");

            PanZoomMode.UpdateTempTransform(new CompositeTransform
            {
                TranslateX = translateX,
                TranslateY = translateY,
                ScaleX = scale,
                ScaleY = scale,
                CenterX = x,
                CenterY = y
            });

        }

        private DoubleAnimation MakeAnimationElement(double to, String name, Duration duration)
        {
            var transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            DoubleAnimation toReturn = new DoubleAnimation();
            toReturn.EnableDependentAnimation = true;
            toReturn.Duration = duration;
            Storyboard.SetTarget(toReturn, transform);
            Storyboard.SetTargetProperty(toReturn, name);
            toReturn.To = to;
            toReturn.EasingFunction = new QuadraticEase();
            return toReturn;
        }

        private CompositeTransform MakeShallowCopy(CompositeTransform transform)
        {
            CompositeTransform newTransform = new CompositeTransform();
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
    }
}
