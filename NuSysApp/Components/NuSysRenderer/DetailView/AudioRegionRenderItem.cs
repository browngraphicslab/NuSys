using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class AudioRegionRenderItem : RectangleUIElement
    {
        /// <summary>
        /// The multiplier of the parent regions scale to fit the full width of the audio player
        /// </summary>
        private double _scaleMultiplier;

        /// <summary>
        /// controller for this region
        /// </summary>
        private AudioLibraryElementController _controller;

        /// <summary>
        /// The normalized start time of the parent audio element
        /// </summary>
        private double _normalizedCropStart;

        /// <summary>
        /// The normalized duration of the parent audio element
        /// </summary>
        private double _normalizedCropDuration;

        /// <summary>
        /// The total width of the parent audio element as it is displayed on screen
        /// </summary>
        private double _totalWidth;

        /// <summary>
        /// boolean helper for whether or not the region is modifiable
        /// </summary>
        private bool _isModifiable;

        /// <summary>
        /// True if the region can be resized or interacted with, false if it can only be displayed
        /// </summary>
        public bool IsModifiable
        {
            get { return _isModifiable; }
            set
            {
                _isModifiable = value;
                IsVisible = _isModifiable;
            }
        }

        /// <summary>
        /// Handler for the on region moved event. sender is the audio region render item that was moved, deltaX is the distance it was moved in on screen coordinates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deltaX"></param>
        public delegate void OnRegionMovedHandler(AudioRegionRenderItem sender, float deltaX);

        /// <summary>
        /// The event fired when the region is moved
        /// </summary>
        public event OnRegionMovedHandler OnRegionMoved;

        /// <summary>
        /// Handler for the on region selected event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deltaX"></param>
        public delegate void OnRegionSelectedHandler(AudioRegionRenderItem sender);

        /// <summary>
        /// The event fired when the region is selected
        /// </summary>
        public event OnRegionSelectedHandler OnRegionSelected;

        /// <summary>
        /// Handler for the on region unselected event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deltaX"></param>
        public delegate void OnRegionUnSelectedHandler(AudioRegionRenderItem sender);

        /// <summary>
        /// The event fired when the region is selected
        /// </summary>
        public event OnRegionSelectedHandler OnRegionUnselected;

        /// <summary>
        /// The handler for the on region resized event
        /// </summary>
        /// <param name="sneder"></param>
        public delegate void OnRegionManualResizedHandler(AudioRegionRenderItem sender);

        /// <summary>
        /// The event fired when the region is resized manually
        /// </summary>
        public event OnRegionManualResizedHandler OnRegionManualResize;

        /// <summary>
        /// The AudioLibraryElementModel associated with this region
        /// </summary>
        public AudioLibraryElementModel LibraryElementModel => _controller.AudioLibraryElementModel;

        /// <summary>
        /// the resizer handle on the left of the audio region
        /// </summary>
        private AudioRegionResizerUIElement _leftResizer;

        /// <summary>
        /// the resizer handle on the right of the audio region
        /// </summary>
        private AudioRegionResizerUIElement _rightResizer;

        public AudioRegionRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, double normalizedCropStart, double normalizedCropDuration, double totalWidth ) : base(parent, resourceCreator)
        {
            // set all the class variables, do this first since methods rely on them later
            _normalizedCropStart = normalizedCropStart;
            _normalizedCropDuration = normalizedCropDuration;
            _totalWidth = totalWidth;
            _controller = controller;

            // Add the resizers and handlers
            _leftResizer = new AudioRegionResizerUIElement(this, Canvas);
            _leftResizer.Dragged += OnResizerDragged;
            _leftResizer.Pressed += OnResizerPressed;
            _leftResizer.Released += OnResizerReleased;
            AddChild(_leftResizer);
            _rightResizer = new AudioRegionResizerUIElement(this, Canvas);
            _rightResizer.Dragged += OnResizerDragged;
            _rightResizer.Pressed += OnResizerPressed;
            _rightResizer.Released += OnResizerReleased;
            AddChild(_rightResizer);


            // the region is only modifiable if it is fully contained by the parent
            IsModifiable = IsFullyContained(_normalizedCropStart, _normalizedCropDuration);

            // upate the audio region bounds to the correct position on the parent
            UpdateAudioRegionBounds(_normalizedCropStart, _normalizedCropDuration, _totalWidth);

            // set the background to the audio region color
            Background = UIDefaults.AudioRegionColor;

            // add events for when the region changes size
            _controller.TimeChanged += ControllerOnStartTimeChanged;
            _controller.DurationChanged += ControllerOnDurationChanged;

            // add events for when the region is dragged
            Dragged += RegionOnDragged;
            DoubleTapped += RegionOnDoubleTapped;
            Pressed += RegionOnPressed;
            Released += RegionOnReleased;
        }

        /// <summary>
        /// Fired whenver the region resizer is released, deselects the region
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnResizerReleased(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            RegionOnReleased(item, pointer);
        }

        /// <summary>
        /// fired whenver the region resizer is pressed, selects the region
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnResizerPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            RegionOnPressed(item, pointer);
        }

        /// <summary>
        /// Fired whenver the region is released, unselected the region
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void RegionOnReleased(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnRegionUnselected?.Invoke(this);
        }

        /// <summary>
        /// Fired whenever the region is pressed, selects the region
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void RegionOnPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnRegionSelected?.Invoke(this);
        }

        /// <summary>
        /// Show the region in the detail view  when it is double tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void RegionOnDoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.NuSessionView.ShowDetailView(_controller);
        }

        /// <summary>
        /// Called whenever one of the resizers is dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnResizerDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // get the normalized delta for the region
            var normalizedDelta = pointer.DeltaSinceLastUpdate.X/_totalWidth * _normalizedCropDuration;

            // get the curr normalized start and duration for the region
            var currNormStartTime = _controller.AudioLibraryElementModel.NormalizedStartTime;
            var currNormDurr = _controller.AudioLibraryElementModel.NormalizedDuration;

            // set the new start and duration based on which resizer was dragged
            if (item == _leftResizer)
            {
                var newStartTime = currNormStartTime + normalizedDelta;
                if (newStartTime > _normalizedCropStart)
                {
                    _controller.SetStartTime(currNormStartTime + normalizedDelta);
                    _controller.SetDuration(currNormDurr - normalizedDelta);
                }

            } else if (item == _rightResizer)
            {
                _controller.SetDuration(currNormDurr + normalizedDelta);
            }

            //The region has been resized so invoke the event
            OnRegionManualResize?.Invoke(this);
        }

        private void RegionOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnRegionMoved?.Invoke(this, pointer.DeltaSinceLastUpdate.X);
        }

        /// <summary>
        /// Remove all evnets here
        /// </summary>
        public override void Dispose()
        {
            _controller.TimeChanged -= ControllerOnStartTimeChanged;
            _controller.DurationChanged -= ControllerOnDurationChanged; //todo why is this unpredictable?????
            _leftResizer.Dragged -= OnResizerDragged;
            _rightResizer.Dragged -= OnResizerDragged;
            Dragged -= RegionOnDragged;
            Pressed -= RegionOnPressed;
            Released -= RegionOnReleased;
            _leftResizer.Pressed -= OnResizerPressed;
            _leftResizer.Released -= OnResizerReleased;
            _rightResizer.Pressed -= OnResizerPressed;
            _rightResizer.Released -= OnResizerReleased;

            base.Dispose();
        }

        /// <summary>
        /// If the region's duration changes update its bounds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControllerOnDurationChanged(object sender, double e)
        {
            UpdateAudioRegionBounds(_normalizedCropStart, _normalizedCropDuration, _totalWidth);
        }

        /// <summary>
        /// If the region's start time changes update its bounds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        private void ControllerOnStartTimeChanged(object sender, double start)
        {
            UpdateAudioRegionBounds(_normalizedCropStart, _normalizedCropDuration, _totalWidth);
        }

        /// <summary>
        /// Places the rectangle of the region in the correct spot on the parent window
        /// </summary>
        /// <param name="normalizedCropStart">The normalized start time of the parent audio element</param>
        /// <param name="normalizedCropDuration">The normalized duration of the parent audio element</param>
        /// <param name="totalWidth">The total width of the parent audio element as it is rendered on screen</param>
        public void UpdateAudioRegionBounds(double normalizedCropStart, double normalizedCropDuration, double totalWidth)
        {
            // find how much we have to scale up by
            _scaleMultiplier = totalWidth / normalizedCropDuration;

            // update class variables
            _normalizedCropStart = normalizedCropStart;
            _normalizedCropDuration = normalizedCropDuration;
            _totalWidth = totalWidth;

            // set the width based on duration times scale
            Width = (float) (_controller.AudioLibraryElementModel.NormalizedDuration * _scaleMultiplier);

            // figure out how much the parent start is offset from the base content start
            var parentOffsetX = (_controller.AudioLibraryElementModel.NormalizedStartTime - normalizedCropStart) * _scaleMultiplier;

            // set the offset based on startTime times scale. but subtract the offset from the base content start
            Transform.LocalPosition = new Vector2((float) parentOffsetX , Transform.LocalPosition.Y);

           // check to see if the region is still modifiable i.e. fully contained in the parent
            IsModifiable = IsFullyContained(_normalizedCropStart, _normalizedCropDuration);
        }

        /// <summary>
        /// Returns true if the audio region is fully contained in the parent
        /// </summary>
        /// <param name="normalizedCropStart"></param>
        /// <param name="normalizedCropDuration"></param>
        /// <returns></returns>
        private bool IsFullyContained(double normalizedCropStart, double normalizedCropDuration)
        {
            if (_controller.AudioLibraryElementModel.NormalizedStartTime >= normalizedCropStart &&
                // if this audio region starts after the parent starts

                _controller.AudioLibraryElementModel.NormalizedStartTime <= normalizedCropStart + normalizedCropDuration &&
                // and starts before the parent ends

                _controller.AudioLibraryElementModel.NormalizedStartTime +
                _controller.AudioLibraryElementModel.NormalizedDuration <= normalizedCropStart + normalizedCropDuration)
                // and ends before the parent ends
            {
                // then it is fully contained
                return true;
            }
            // otherwise it is not
            return false;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // offset the right resizer to the right side of the rectangle
            _rightResizer.Transform.LocalPosition = new Vector2(Width, 0);

            // set the resizer heights to the height of the rectangle
            _leftResizer.Height = Height;
            _rightResizer.Height = Height;

            base.Update(parentLocalToScreenTransform);
        }
    }
}