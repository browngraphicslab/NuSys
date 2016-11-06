using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class AudioMediaContentUIElement : RectangleUIElement
    {
        private AudioLibraryElementController _controller;
        private MediaElement _mediaElement;
        private CanvasBitmap _audioWaveImage;
        private bool _isLoading;
        private RectangleUIElement _shadowRect;
        private Color _shadowColor = UIDefaults.ShadowColor;
        private float _currentShadowPosition;
        private double _durationInMillis;
        private double _startTimeInMillis;

        public AudioMediaContentUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, MediaElement mediaElement) : base(parent, resourceCreator)
        {
            _controller = controller;
            _mediaElement = mediaElement;
            _mediaElement.MediaOpened += MediaElementOnMediaOpened;
            _isLoading = true;
            _shadowRect = new RectangleUIElement(this, resourceCreator);
            InitializeShadowRectUI(_shadowRect);
            _shadowRect.IsHitTestVisible = false;
            AddChild(_shadowRect);

            Tapped += AudioMediaContentUIElement_Tapped;
            Dragged += AudioMediaContentUIElement_Dragged;

            _controller.TimeChanged += OnStartTimeChanged;
            _controller.DurationChanged += OnDurationChanged;

            _controller.ContentDataController.ContentDataModel.OnRegionAdded += OnRegionRemoved;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved += OnRegionAdded;

            // compute all the regions so they are displayed
            ComputeRegions();
        }

        /// <summary>
        /// Fired whenever a region is added to this audio element
        /// </summary>
        /// <param name="regionlibraryelementmodelid"></param>
        private void OnRegionAdded(string regionlibraryelementmodelid)
        {
            // recompute all the regions for the audio element, basically place all of them in the correct spot, and make sure
            // you have every region that exists
            ComputeRegions();
        }

        /// <summary>
        /// Fired whenever a region is removed from this audio element
        /// </summary>
        /// <param name="regionlibraryelementmodelid"></param>
        private void OnRegionRemoved(string regionlibraryelementmodelid)
        {
            // recompute all the regions for the audio element, basically place all of them in the correct spot, and make
            // sure that any region that no longer exists is not being displayed
            ComputeRegions();
        }

        /// <summary>
        /// Fired whenever the duration of this audio element changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDurationChanged(object sender, double e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fired whenver the start time of this audio element changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        private void OnStartTimeChanged(object sender, double start)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            _mediaElement.MediaOpened -= MediaElementOnMediaOpened;
            Tapped -= AudioMediaContentUIElement_Tapped;
            Dragged -= AudioMediaContentUIElement_Dragged;
            _controller.TimeChanged += OnStartTimeChanged;
            _controller.DurationChanged += OnDurationChanged;

            _controller.ContentDataController.ContentDataModel.OnRegionAdded += OnRegionRemoved;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved += OnRegionAdded;

            var children = GetChildren();
            foreach (var child in children)
            {
                var region = child as AudioRegionRenderItem;
                Debug.Assert(region != null);
                region.OnRegionMoved -= RegionOnRegionMoved;
            }
            base.Dispose();
        }

        private void MediaElementOnMediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _durationInMillis = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            _startTimeInMillis = _controller.AudioLibraryElementModel.NormalizedStartTime *
                                 _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void AudioMediaContentUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;
            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        private void SetMediaElementToCurrentScrubPosition(MediaElement mediaElement)
        {
            var currPositionInMilliSeconds = _durationInMillis * _currentShadowPosition + _startTimeInMillis;
            var ts = new TimeSpan(0, 0, 0, 0, (int)currPositionInMilliSeconds);
            mediaElement.Position = ts;
        }

        private void AudioMediaContentUIElement_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;

            UITask.Run(() =>
            {
                SetMediaElementToCurrentScrubPosition(_mediaElement);

            });
        }

        private void InitializeShadowRectUI(RectangleUIElement shadowRect)
        {
            shadowRect.Background = _shadowColor;
        }

        public override async Task Load()
        {
            _audioWaveImage = await CanvasBitmap.LoadAsync(Canvas, _controller.LargeIconUri);
            Image = _audioWaveImage;
            _isLoading = false;

            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_isLoading)
            {
                return;
            }

            // set the scrubber position based on the current position of the media player
            UITask.Run(() =>
            {
                _currentShadowPosition = (float)_mediaElement.Position.TotalMilliseconds /
                                  (float)_mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            });

            if (float.IsNaN(_currentShadowPosition))
            {
                return;
            }

            _shadowRect.Width = Math.Min(1, Math.Max(_currentShadowPosition, 0))*Width;
            _shadowRect.Height = Height;


            base.Update(parentLocalToScreenTransform);
        }

        protected virtual void ComputeRegions()
        {
            // remove all the current regions and dispose of their events and everything properly
            var children = GetChildren();         
            foreach (var child in children)
            {
                var region = child as AudioRegionRenderItem;
                if (region == null) continue;
                region.OnRegionMoved -= RegionOnRegionMoved;
                region.Dispose();
                RemoveChild(region);
            }

            // get all the other library elment models that are assoicated with this content data model
            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<AudioLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId);

            // for each library elmeent model instantiate a new region
            foreach (var l in others)
            {
                var region = new AudioRegionRenderItem(this, Canvas, _controller, l.NormalizedStartTime,
                    l.NormalizedDuration, Width)
                {
                    Height = Height // set the hight to the height of the content
                };
                region.OnRegionMoved += RegionOnRegionMoved;
                AddChild(region);
            }

            // sort is ascending so 1 means a is below b in terms of z indexing and -1 is the inverse
            SortChildren((a, b) => {
                if (a is AudioRegionRenderItem && !(b is AudioRegionRenderItem))
                {
                    return -1; // regions should be on top of non regions
                } 
                if (!(a is AudioRegionRenderItem) && b is AudioRegionRenderItem)
                {
                    return 1; // non regions should be below regions
                }

                // otherwise put the larger region after the smaller region
                var areaA = a.GetLocalBounds(); var areaB = b.GetLocalBounds();
                return areaA.Width * areaA.Height >= areaB.Width * areaB.Height ? 1 : -1;
            });
        }

        private void RegionOnRegionMoved(AudioRegionRenderItem region, float deltax)
        {
            // ratio of how far we moved on the parent. If deltaX is half the parent's Width then we moved half of the parent
            // region's width
            var deltaOnParent = deltax/Width;

            // the normalized distance we moved compared to the parent. If we moved half of the parent regions width
            // and the parent had a normalized duration of 1/3 then we moved a normalized duration of 1/6
            var normalizedDelta = deltaOnParent*_controller.AudioLibraryElementModel.NormalizedDuration;

            // calculate the new region start time based on the delta
            var newRegionStart = region.LibraryElementModel.NormalizedStartTime + normalizedDelta;

            // make sure the new region start is greater than the start of the current region being displayed
            newRegionStart = Math.Max(newRegionStart, _controller.AudioLibraryElementModel.NormalizedStartTime);

            // make sure the new region end is less than the end of the current region being displayed
            var normalizedEndOfCurrentRegion = _controller.AudioLibraryElementModel.NormalizedStartTime +
                                               _controller.AudioLibraryElementModel.NormalizedDuration;
            if (newRegionStart > normalizedEndOfCurrentRegion)
            {
                newRegionStart = normalizedEndOfCurrentRegion - region.LibraryElementModel.NormalizedStartTime;
            }
            
            // get the controller for the region
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as AudioLibraryElementController;

            Debug.Assert(controller != null);

            // set the start time based on the calculated start time
            controller.SetStartTime(newRegionStart);
        }
    }
}
