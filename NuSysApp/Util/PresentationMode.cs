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
        private ElementModel _previousNode = null;
        private ElementModel _nextNode = null;
        private ElementModel _currentNode;

        public PresentationMode(ElementModel start)
        {
            _currentNode = start;
            Load();
            FullScreen(_currentNode);
        }

        /// <summary>
        /// Checks if there is a valid next node and stores it
        /// </summary>
        /// <returns></returns>
        private void Load()
        {
            var vmList = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                item => ((ElementViewModel)item.DataContext).Model.Id == _currentNode.Id);

            var vm = (ElementViewModel)vmList.Single().DataContext;

            _nextNode = GetNextOrPrevNode(vm, false);
            _previousNode = GetNextOrPrevNode(vm, true);
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
            FullScreen(_currentNode);
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
            FullScreen(_currentNode);
        }

        public void ExitMode()
        {
            
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
            if (e.Width > e.Height) { 
               scale = sv.ActualWidth / e.Width;
               //scale = scale/scaleXInitial;
        }
            else
            {
                scale = sv.ActualHeight / e.Height;
              //  scale = scale / scaleYInitial;
            }
                

            // Scale the active free form viewer so that the passed in element appears to be full screen.
            scale = scale * .7; // adjustment so things don't get cut off
            var center1 = new Point(compositeTransform.CenterX, compositeTransform.CenterY);
            var center2 = new Point(x, y);
            var scalex = new Point(scaleXInitial, scale);
            var scaley = new Point(scaleYInitial, scale);
            // anim(compositeTransform, center1, center2, scalex, scaley);


            testZoomOut(scale,x,y,translateX,translateY);

             //THIS WORKS, BUT NO ANIMATION
             /*
            compositeTransform.CenterX = x;
            compositeTransform.CenterY = y;
            compositeTransform.ScaleX = scale;
            compositeTransform.ScaleY = scale;
            */
            
        }

        private static void testZoomOut(double scale, double x , double y, double translateX, double translateY)
        {
            var transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            var scaleX1 = transform.ScaleX;
            var scaleY1 = transform.ScaleY;
           



            //Storyboard board = new Storyboard();
            // Create a duration of 2 seconds.
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
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

            DoubleAnimation centerAnimationX = new DoubleAnimation();
            DoubleAnimation centerAnimationY = new DoubleAnimation();
            centerAnimationX.Duration = duration;
            centerAnimationY.Duration = duration;
            //Storyboard justintimeStoryboard = new Storyboard();
            //justintimeStoryboard.Duration = duration;
            justintimeStoryboard.Children.Add(centerAnimationX);
            justintimeStoryboard.Children.Add(centerAnimationY);
            Storyboard.SetTarget(centerAnimationX, transform);
            Storyboard.SetTarget(centerAnimationY, transform);

            // Set the X and Y properties of the Transform to be the target properties
            // of the two respective DoubleAnimations.
            Storyboard.SetTargetProperty(scaleAnimationX, "ScaleX");
            Storyboard.SetTargetProperty(scaleAnimationY, "ScaleY");
            scaleAnimationX.To = scale;
            scaleAnimationY.To = scale;

            // Set the X and Y properties of the Transform to be the target properties
            // of the two respective DoubleAnimations.
            Storyboard.SetTargetProperty(centerAnimationX, "CenterX");
            Storyboard.SetTargetProperty(centerAnimationY, "CenterY");
            centerAnimationX.To = x;
            centerAnimationY.To = y;

            DoubleAnimation translateAnimationX = new DoubleAnimation();
            DoubleAnimation translateAnimationY = new DoubleAnimation();
            translateAnimationX.Duration = duration;
            translateAnimationY.Duration = duration;
            //Storyboard justintimeStoryboard = new Storyboard();
            //justintimeStoryboard.Duration = duration;
            justintimeStoryboard.Children.Add(translateAnimationX);
            justintimeStoryboard.Children.Add(translateAnimationY);
            Storyboard.SetTarget(translateAnimationX, transform);
            Storyboard.SetTarget(translateAnimationY, transform);

            // Set the X and Y properties of the Transform to be the target properties
            // of the two respective DoubleAnimations.
            Storyboard.SetTargetProperty(translateAnimationX, "TranslateX");
            Storyboard.SetTargetProperty(translateAnimationY, "TranslateY");
            translateAnimationX.To = translateX;
            translateAnimationY.To = translateY;

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
