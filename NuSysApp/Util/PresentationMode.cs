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
using System.Diagnostics;

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

        public PresentationMode(ElementViewModel start)
        {
            _currentNode = start;
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);
            Load();
            FullScreen();
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

        public bool Next()
        {
            return (_nextNode != null);
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            _currentNode = _nextNode;
            Load();
            FullScreen();
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
            _currentNode = _previousNode;
            Load();
            FullScreen();
        }

        public void ExitMode()
        {
            AnimatePresentation(_originalTransform.ScaleX, _originalTransform.CenterX, _originalTransform.CenterY, _originalTransform.TranslateX, _originalTransform.TranslateY);
        }

        /// <summary>
        /// Finds previous node for presentation if reverse is true, next node otherwise.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ElementModel GetNextOrPrevNode(ElementViewModel vm, bool reverse)
        {
            foreach (LinkElementController link in vm.LinkList)
            {
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

            // Define some variables that will be used in future translation/scaling
            var nodeWidth = _currentNode.Width;
            var nodeHeight = _currentNode.Height + 40; // 40 for title adjustment
            var sv = SessionController.Instance.SessionView;
            var x = _currentNode.Model.X + nodeWidth / 2;
            var y = _currentNode.Model.Y-40 + nodeHeight / 2;
            var widthAdjustment = sv.ActualWidth / 2;
            var heightAdjustment = sv.ActualHeight / 2;

            // Reset the scaling and translate the free form viewer so that the passed in element is at the center
            var scaleX = 1;
            var scaleY = 1;
            var translateX = widthAdjustment - x;
            var translateY = heightAdjustment - y;
            double scale;
           
            // Scale based on the width and height proportions of the current node
            if (nodeWidth > nodeHeight) {    
               scale = sv.ActualWidth / nodeWidth;
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
            var transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;

            Storyboard storyboard = new Storyboard();
            storyboard.Duration = duration;
            DoubleAnimation scaleAnimationX = MakeAnimationElement(scale, transform.ScaleX, "ScaleX", duration);
            DoubleAnimation scaleAnimationY = MakeAnimationElement(scale, transform.ScaleY,"ScaleY", duration);
            DoubleAnimation centerAnimationX = MakeAnimationElement(x, transform.CenterX, "CenterX", duration);
            DoubleAnimation centerAnimationY = MakeAnimationElement(y, transform.CenterY,"CenterY", duration);
            DoubleAnimation translateAnimationX = MakeAnimationElement(translateX, transform.TranslateX,"TranslateX", duration);
            DoubleAnimation translateAnimationY = MakeAnimationElement(translateY, transform.TranslateY,"TranslateY", duration);

            storyboard.Children.Add(scaleAnimationX);
            storyboard.Children.Add(scaleAnimationY);
            storyboard.Children.Add(centerAnimationX);
            storyboard.Children.Add(centerAnimationY);
            storyboard.Children.Add(translateAnimationX);
            storyboard.Children.Add(translateAnimationY);

            // Make the Storyboard a resource.
            SessionController.Instance.SessionView.Resources.Add("PresentationStoryboard", storyboard);

            // Begin the animation.
            storyboard.Begin();
            SessionController.Instance.SessionView.Resources.Remove("PresentationStoryboard");

        }

        private DoubleAnimation MakeAnimationElement(double to, double from, String name, Duration duration)
        {
            var transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            DoubleAnimation toReturn = new DoubleAnimation();
            toReturn.Duration = duration;
            Storyboard.SetTarget(toReturn, transform);
            Storyboard.SetTargetProperty(toReturn, name);
            toReturn.To = to;
            toReturn.From = from;
            
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
