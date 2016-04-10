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

namespace NuSysApp.Util
{
    /// <summary>
    /// Implements PresentationMode for nodes.
    /// </summary>
    class PresentationMode
    {
        private ObservableCollection<ElementModel> previousNodes;
        private ElementModel currentNode;
        private ElementModel nextNode;

        public PresentationMode(ElementModel start)
        {
            previousNodes = new ObservableCollection<ElementModel>();
            currentNode = start;
            FullScreen(currentNode);
        }

        /// <summary>
        /// Checks if there is a valid next node and stores it
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            var vmList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                item => ((ElementViewModel)item.DataContext).Model.Id == currentNode.Id);

            var vm = (ElementViewModel)vmList.Single().DataContext;

            nextNode = getNextNode(vm);

            return (nextNode != null);
        }

        /// <summary>
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            previousNodes.Add(currentNode);
            currentNode = nextNode;
            FullScreen(currentNode);
        }

        /// <summary>
        /// Checks if there are any previous nodes
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (previousNodes.Count > 0);
        }

        /// <summary>
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            currentNode = previousNodes.Last();
            previousNodes.Remove(previousNodes.Last());
            FullScreen(currentNode);
        }

        public void ExitMode()
        {
            
        }

        /// <summary>
        /// Finds next possible node for presentation. Currently searches for any node linked to the current node 
        /// that is not the previous.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private ElementModel getNextNode(ElementViewModel vm)
        {
            var prev = Previous() ? previousNodes.Last() : null;
            foreach (LinkElementController link in vm.LinkList)
            {
                if (link.OutElement.Model.Equals(vm.Model) && !link.InElement.Model.Equals(prev))
                {
                    return link.InElement.Model;
                }

                if (link.InElement.Model.Equals(vm.Model) && !link.OutElement.Model.Equals(prev))
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

        public static void FullScreen(ElementModel e)
        {
            // Define some variables that will be used in future translation/scaling
            
            var sv = SessionController.Instance.SessionView;
            var x = e.X + e.Width / 2;
            var y = e.Y + e.Height / 2;
            var widthAdjustment = sv.ActualWidth / 2;
            var heightAdjustment = sv.ActualHeight / 2;

            // Reset the scaling and translate the free form viewer so that the passed in element is at the center
            var compositeTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            var scaleXInitial = compositeTransform.ScaleX;
            var scaleYInitial = compositeTransform.ScaleY;
            var scaleX = 1;
            var scaleY = 1;
            var translateX = widthAdjustment - x;
            var translateY = heightAdjustment - y;

            /*compositeTransform.ScaleX = 1;
            compositeTransform.ScaleY = 1;
            compositeTransform.TranslateX = widthAdjustment - x;
            compositeTransform.TranslateY = heightAdjustment - y;
            */

            // Obtain correct scale value based on width/height ratio of passed in element
            double scale;
            if (e.Width > e.Height)
                scale = sv.ActualWidth / e.Width;
            else
                scale = sv.ActualHeight / e.Height;

            // Scale the active free form viewer so that the passed in element appears to be full screen.
            scale = scale * .7; // adjustment so things don't get cut off
            var center1 = new Point(compositeTransform.CenterX, compositeTransform.CenterY);
            var center2 = new Point(x, y);
            var scalex = new Point(scaleXInitial, scale);
            var scaley = new Point(scaleYInitial, scale);
            // anim(compositeTransform, center1, center2, scalex, scaley);
            testZoomOut();

            /* THIS WORKS, BUT NO ANIMATION
            compositeTransform.CenterX = x;
            compositeTransform.CenterY = y;
            compositeTransform.ScaleX = scale;
            compositeTransform.ScaleY = scale;
            */
        }

        private static void testZoomOut()
        {
            var transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            var scaleX1 = transform.ScaleX;
            var scaleY1 = transform.ScaleY;
           



            //Storyboard board = new Storyboard();
            // Create a duration of 2 seconds.
            Duration duration = new Duration(TimeSpan.FromSeconds(2));
            // Create two DoubleAnimations and set their properties.
            DoubleAnimation scaleAnimationX = new DoubleAnimation();
            DoubleAnimation scaleAnimationY = new DoubleAnimation();
            scaleAnimationX.Duration = duration;
            scaleAnimationY.Duration = duration;
            Storyboard justintimeStoryboard = new Storyboard();
            justintimeStoryboard.Duration = duration;
            justintimeStoryboard.Children.Add(scaleAnimationX);
            justintimeStoryboard.Children.Add(scaleAnimationY);
            Storyboard.SetTarget(scaleAnimationX, transform);
            Storyboard.SetTarget(scaleAnimationY, transform);

            // Set the X and Y properties of the Transform to be the target properties
            // of the two respective DoubleAnimations.
            Storyboard.SetTargetProperty(scaleAnimationX, "ScaleX");
            Storyboard.SetTargetProperty(scaleAnimationY, "ScaleY");
            scaleAnimationX.To = scaleX1*2;
            scaleAnimationY.To = scaleY1*2;

            // Make the Storyboard a resource.
            SessionController.Instance.SessionView.Resources.Add("justintimeStoryboard", justintimeStoryboard);

            // Begin the animation.
            justintimeStoryboard.Begin();

            SessionController.Instance.SessionView.Resources.Remove("justintimeStoryboard");

        }

        /*
        private static void anim(CompositeTransform ct, Point p1, Point p2, Point scaleX, Point scaleY)
        {


            Storyboard board = new Storyboard();

            var animateX = getAnimation(p1.X, p2.X);
            var animateY = getAnimation(p1.Y, p2.Y);
            var animateScaleX = getAnimation(scaleX.X, scaleX.Y);
            var animateScaleY = getAnimation(scaleY.X, scaleY.Y);

            Storyboard.SetTarget(animateX, ct);
            Storyboard.SetTargetProperty(animateY, "TranslateTransform.CenterX");

            Storyboard.SetTarget(animateY, ct);
            Storyboard.SetTargetProperty(animateY, "TranslateTransform.CenterY");

            Storyboard.SetTarget(animateScaleX, ct);
            Storyboard.SetTargetProperty(animateScaleX, "TranslateTransform.ScaleX");

            Storyboard.SetTarget(animateScaleY, ct);
            Storyboard.SetTargetProperty(animateScaleY, "TranslateTransform.ScaleY");

            board.Children.Add(animateX);
            board.Children.Add(animateY);
            board.Children.Add(animateScaleX);
            board.Children.Add(animateScaleY);
            board.Duration = new Duration(TimeSpan.FromMilliseconds(400));
            //SessionController.Instance.SessionView.Resources.Add("justintimeStoryboard", board);
            
            board.Begin();

        }

        private static DoubleAnimation getAnimation(double from, double to)
        {
            var animate = new DoubleAnimation();
            animate.From = from;
            animate.To += to;
            animate.Duration = TimeSpan.FromMilliseconds(400);
            return animate;
        }
        */
    }
}
