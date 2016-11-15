using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
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

        /// <summary>
        /// True if we we need to update the regions
        /// </summary>
        private bool _updateRegions;

        /// <summary>
        /// True if we need to rerender
        /// </summary>
        private bool _reRender;

        /// <summary>
        /// The currently selected region
        /// </summary>
        private AudioRegionRenderItem _activeRegion;

        /// <summary>
        /// True if we need to refresh the crop mask, the dark rectangle around the active region
        /// </summary>
        private bool _refreshMask;

        /// <summary>
        /// The mask use to darken the background around the active region
        /// </summary>
        private CanvasGeometry _mask;

        private Color _maskColor = UIDefaults.AudioRegionMaskColor;

        /// <summary>
        /// True if regions are visible false otherwise
        /// </summary>
        public bool IsRegionsVisible { get; set; }

        public override float Width
        {
            get { return base.Width; }
            set
            {
                if (Math.Abs(base.Width - value) > .001)
                {
                    _updateRegions = true;
                }
                base.Width = value;
            }
        }

        public AudioMediaContentUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, AudioLibraryElementController controller, MediaElement mediaElement, bool showRegions) : base(parent, resourceCreator)
        {
            _controller = controller;
            _mediaElement = mediaElement;
            _mediaElement.MediaOpened += MediaElementOnMediaOpened;
            _isLoading = true;
            _shadowRect = new RectangleUIElement(this, resourceCreator);
            InitializeShadowRectUI(_shadowRect);
            _shadowRect.IsHitTestVisible = false;
            AddChild(_shadowRect);

            IsRegionsVisible = showRegions;

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
        /// Rerenders the image if the size of the region has changed
        /// </summary>
        protected void ReRender()
        {
            // we cannot render while the image is loading
            if (_isLoading) return;

            var croppy = new CropEffect()
            {
                Source = _audioWaveImage
            };

            var lib = (_controller.LibraryElementModel as AudioLibraryElementModel);
            var x = lib.NormalizedStartTime* _audioWaveImage.Size.Width;
            var w = lib.NormalizedDuration * _audioWaveImage.Size.Width;

            croppy.SourceRectangle = new Rect(x, 0, w, _audioWaveImage.Size.Height);

            Image = croppy;

            UpdateRegionBounds();

        }

        /// <summary>
        /// Fired whenever the duration of this audio element changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDurationChanged(object sender, double e)
        {
            ReRender();
        }

        /// <summary>
        /// Fired whenver the start time of this audio element changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        private void OnStartTimeChanged(object sender, double start)
        {
            ReRender();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (_isLoading)
            {
                return;
            }

            base.Draw(ds);
            if (_mask == null) return; // only draw the mask if it isn't null
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            ds.FillGeometry(_mask, _maskColor);
            ds.Transform = orgTransform;
        }

        public override void Dispose()
        {
            UITask.Run(() =>
            {
                _mediaElement.MediaOpened -= MediaElementOnMediaOpened;
            });
            Tapped -= AudioMediaContentUIElement_Tapped;
            Dragged -= AudioMediaContentUIElement_Dragged;
            _controller.TimeChanged -= OnStartTimeChanged;
            _controller.DurationChanged -= OnDurationChanged; //todo find out why this is unpredictable

            _controller.ContentDataController.ContentDataModel.OnRegionAdded -= OnRegionRemoved;
            _controller.ContentDataController.ContentDataModel.OnRegionRemoved -= OnRegionAdded;

            var children = GetChildren();
            foreach (var child in children)
            {
                var region = child as AudioRegionRenderItem;
                if (region == null) continue;
                region.OnRegionMoved -= RegionOnRegionMoved;
                region.OnRegionSelected -= RegionOnRegionSelected;
                region.OnRegionUnselected -= RegionOnRegionUnselected;
                region.OnRegionManualResize -= RegionOnRegionManualResize;

            }
            base.Dispose();
        }

        private void MediaElementOnMediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _durationInMillis = _mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void AudioMediaContentUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;
            UITask.Run(() =>
            {
                SetMediaElementToCurrentShadowPosition(_mediaElement);

            });
        }

        /// <summary>
        /// Set the Media Element to the current shadow position
        /// </summary>
        /// <param name="mediaElement"></param>
        private void SetMediaElementToCurrentShadowPosition(MediaElement mediaElement)
        {
            var startTime = (float)(_controller.AudioLibraryElementModel.NormalizedStartTime * _durationInMillis);
            var endTime = (float)(startTime + _controller.AudioLibraryElementModel.NormalizedDuration * _durationInMillis);
            var duration = endTime - startTime;
            var currPositionInMilliSeconds = duration * _currentShadowPosition + startTime;
            var ts = new TimeSpan(0, 0, 0, 0, (int)currPositionInMilliSeconds);
            mediaElement.Position = ts;
        }

        private void AudioMediaContentUIElement_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _currentShadowPosition = currPoint.X / Width;

            UITask.Run(() =>
            {
                SetMediaElementToCurrentShadowPosition(_mediaElement);

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

            ReRender();
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
                _currentShadowPosition = GetShadowPosition();

            });

            if (_updateRegions)
            {
                ComputeRegions();
                _updateRegions = false;
            }

            if (_refreshMask)
            {
                ComputeMask();
                _refreshMask = false;
            }

            if (float.IsNaN(_currentShadowPosition))
            {
                _currentShadowPosition = 0;
                _shadowRect.Width = 0;
            }
            else
            {
                _shadowRect.Width = Math.Min(1, Math.Max(_currentShadowPosition, 0)) * Width;
            }
            
            _shadowRect.Height = Height;


            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Computes the mask for the audio regions
        /// </summary>
        private void ComputeMask()
        {
            // if there is no active region there is no mask so just return
            if (_activeRegion == null)
            {
                _mask = null;
                return;
            }
            

            // otherwise calculate the mask based off the active region
            var fullRect = CanvasGeometry.CreateRectangle(Canvas, 0, 0, Width, Height);
            var activeRect = CanvasGeometry.CreateRectangle(Canvas, _activeRegion.Transform.LocalX,
                _activeRegion.Transform.LocalY, _activeRegion.Width, _activeRegion.Height);
            _mask = fullRect.CombineWith(activeRect, Matrix3x2.Identity, CanvasGeometryCombine.Xor);

        }

        /// <summary>
        /// Gets the normalized position of the audio being played. for example if we were 1/3 of the way through the audio region
        /// this would return .33 If the audio had finished and had reached the end of the region this would return 1
        /// </summary>
        /// <returns></returns>
        private float GetShadowPosition()
        {
            var startTime = (float)(_mediaElement.Position.TotalMilliseconds - _controller.AudioLibraryElementModel.NormalizedStartTime * _durationInMillis);
            var endTime = (float)(startTime + _controller.AudioLibraryElementModel.NormalizedDuration * _durationInMillis);
            var duration = endTime - startTime;
            return startTime / duration;
        }

        /// <summary>
        /// Call this to update the widths of the regions, basically when the current region being displayed changes start time or duration.
        /// should probably only be called in rerender
        /// </summary>
        protected void UpdateRegionBounds()
        {
            var children = GetChildren();
            foreach (var child in children)
            {
                var region = child as AudioRegionRenderItem;
                region?.UpdateAudioRegionBounds(_controller.AudioLibraryElementModel.NormalizedStartTime, _controller.AudioLibraryElementModel.NormalizedDuration, Width);
            }
        }

        protected virtual void ComputeRegions()
        {
            // if regions are not visible then return
            if (!IsRegionsVisible)
            {
                return;
            }

            // remove all the current regions and dispose of their events and everything properly
            var children = GetChildren();         
            foreach (var child in children)
            {
                var region = child as AudioRegionRenderItem;
                if (region == null) continue;
                region.OnRegionMoved -= RegionOnRegionMoved;
                region.OnRegionSelected -= RegionOnRegionSelected;
                region.OnRegionUnselected -= RegionOnRegionUnselected;
                region.OnRegionManualResize -= RegionOnRegionManualResize;

                RemoveChild(region);
            }

            // get all the other library elment models that are assoicated with this content data model
            var others = SessionController.Instance.ContentController.AllLibraryElementModels.Where(l => l.ContentDataModelId == _controller.ContentDataController.ContentDataModel.ContentId).Cast<AudioLibraryElementModel>();
            others = others.Where(l => l.LibraryElementId != _controller.LibraryElementModel.LibraryElementId); // dont include this audio library element

            // for each library elmeent model instantiate a new region
            foreach (var l in others)
            {
                var regionController = SessionController.Instance.ContentController.GetLibraryElementController(l.LibraryElementId) as AudioLibraryElementController;
                Debug.Assert(regionController != null);
                var region = new AudioRegionRenderItem(this, Canvas, regionController, _controller.AudioLibraryElementModel.NormalizedStartTime,
                    _controller.AudioLibraryElementModel.NormalizedDuration, Width)
                {
                    Height = Height // set the hight to the height of the content
                };
                region.OnRegionMoved += RegionOnRegionMoved; // if you add region events here remove them earlier in this method and in the dispose event
                region.OnRegionSelected += RegionOnRegionSelected;
                region.OnRegionUnselected += RegionOnRegionUnselected;
                region.OnRegionManualResize += RegionOnRegionManualResize;

                AddChild(region);
            }

            // sort is ascending so 1 means a is below b in terms of z indexing and -1 is the inverse
            SortChildren((a, b) => {
                if (a is AudioRegionRenderItem && !(b is AudioRegionRenderItem))
                {
                    return 1; // regions should be on top of non regions
                } 
                if (!(a is AudioRegionRenderItem) && b is AudioRegionRenderItem)
                {
                    return -1; // non regions should be below regions
                }

                // otherwise put the larger region after the smaller region
                var areaA = a.GetLocalBounds();
                var areaB = b.GetLocalBounds();
                return areaA.Width * areaA.Height >= areaB.Width * areaB.Height ? -1 : 1;
            });
        }

        /// <summary>
        /// When a region is resized manually we need to refresh the mask
        /// </summary>
        /// <param name="sender"></param>
        private void RegionOnRegionManualResize(AudioRegionRenderItem sender)
        {
            _refreshMask = true;
        }

        /// <summary>
        /// Fired whenever a region is unselected
        /// </summary>
        /// <param name="sender"></param>
        private void RegionOnRegionUnselected(AudioRegionRenderItem sender)
        {
            _activeRegion = null;
            _refreshMask = true;
        }

        /// <summary>
        /// Fired whenever a region is selected
        /// </summary>
        /// <param name="sender"></param>
        private void RegionOnRegionSelected(AudioRegionRenderItem sender)
        {
            _activeRegion = sender;
            _refreshMask = true;

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
            var newRegionEnd = newRegionStart + region.LibraryElementModel.NormalizedDuration;
            if (newRegionEnd > normalizedEndOfCurrentRegion)
            {
                newRegionStart = normalizedEndOfCurrentRegion - region.LibraryElementModel.NormalizedDuration;
            }
            
            // get the controller for the region
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(region.LibraryElementModel.LibraryElementId) as AudioLibraryElementController;

            Debug.Assert(controller != null);

            // set the start time based on the calculated start time
            controller.SetStartTime(newRegionStart);

            // since the region being dragged is the one we are masking, we must reset the mask
            _refreshMask = true;
        }
    }
}
