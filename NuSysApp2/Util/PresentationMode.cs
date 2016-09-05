﻿using System;
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

namespace NuSysApp2
{
    /// <summary>
    /// Implements a prezi-like presentation mode.
    /// </summary>
    class PresentationMode : IDisposable, IModable
    {
        private ElementViewModel _previousNode;
        private ElementViewModel _nextNode;
        private ElementViewModel _currentNode;

        // Animation Values
        private CompositeTransform _originalTransform;
        private DispatcherTimer _timer;
        private Storyboard _storyboard;
        private SolidColorBrush _backwardColor = Application.Current.Resources["lighterredcolor"] as SolidColorBrush;
        private SolidColorBrush _forwardColor = Application.Current.Resources["color4"] as SolidColorBrush;


        // IModeable Interface
        public ModeType Mode => ModeType.PRESENTATION; 

        public PresentationMode(ElementViewModel start)
        {
            Debug.Assert(start != null);

            // setup the animation variables
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1)};
            _timer.Tick += OnTick;
            _storyboard = new Storyboard();
            _currentNode = start;

            // get a copy of the session controllers transform so we can revert back to it at end of presentation
            _originalTransform = MakeShallowCopy(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);

            // set current, forward, and backward for presentation movement
            Load(_currentNode, out _previousNode, out _nextNode);

            // zoom in on the current node
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
            Debug.Assert(_currentNode != null, "the current node should always be set if we are in presentation mode");
            FullScreen(_currentNode);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Used to check if there is a valid presentation link going forward from the current Node
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            return (_nextNode != null);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Full screen zooms into the next node found
        /// </summary>
        public void MoveToNext()
        {
            _currentNode = _nextNode;
            Load(_currentNode, out _previousNode, out _nextNode);
            FullScreen(_currentNode);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Used to check if there is a valid presentation link going backward from the current Node
        /// </summary>
        /// <returns></returns>
        public bool Previous()
        {
            return (_previousNode != null);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Full screen zooms into the previous node
        /// </summary>
        public void MoveToPrevious()
        {
            _currentNode = _previousNode;
            Load(_currentNode, out _previousNode, out _nextNode);
            FullScreen(_currentNode);
        }

        /// <summary>
        /// From the IModeable Interface
        /// Exits presentation mode by resetting the original composite transform properties
        /// </summary>
        public void ExitMode()
        {
            AnimatePresentation(_originalTransform.ScaleX, _originalTransform.CenterX, _originalTransform.CenterY, _originalTransform.TranslateX, _originalTransform.TranslateY);
        }

        /// <summary>
        /// If there is a presenation link pointing away from param currentElemVM, returns the ElementViewModel
        /// at the end of that presentation link.
        /// Else returns nulll
        /// </summary>
        /// <param name="currentElemVm"></param>
        /// <returns></returns>
        private ElementViewModel GetNext(ElementViewModel currentElemVm)
        {
            Debug.Assert(currentElemVm != null);
            Debug.Assert(PresentationLinkViewModel.Models != null);
            // there might be more than one outgoing link but we always just choose one
            var outgoingLink = PresentationLinkViewModel.Models.FirstOrDefault(vm => vm.InElementId == currentElemVm.Id);
            var nextElemVm = outgoingLink?.OutElementViewModel;
            return nextElemVm;

        }

        /// <summary>
        /// If there is a presenation link pointing to the param currentElemVM, returns the ElementViewModel
        /// at the start of that presentation link.
        /// Else returns nulll
        /// </summary>
        /// <param name="currentElemVm"></param>
        /// <returns></returns>
        private ElementViewModel GetPrevious(ElementViewModel currentElemVm)
        {
            Debug.Assert(currentElemVm != null);
            Debug.Assert(PresentationLinkViewModel.Models != null);
            // there might be more than one outgoing link but we always just choose one
            var incomingLink = PresentationLinkViewModel.Models.FirstOrDefault(vm => vm.OutElementId == currentElemVm.Id);
            var prevElemVm = incomingLink?.InElementViewModel;
            return prevElemVm;
        }


        /// <summary>
        /// Sets previousElemVm to an ElemVM with a presentation link that ends at currentElemVM
        /// Sets nextElemVm to an ElemVM with a presentation link that starts at currentElemVM 
        /// </summary>
        /// <param name="currentElemVm"></param>
        /// <param name="previousElemVm"></param>
        /// <param name="nextElemVm"></param>
        private void Load(ElementViewModel currentElemVm, out ElementViewModel previousElemVm, out ElementViewModel nextElemVm)
        {
            previousElemVm = GetNext(currentElemVm);
            nextElemVm = GetPrevious(currentElemVm);
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
            _storyboard = new Storyboard {Duration = duration};

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
        }
    }
}