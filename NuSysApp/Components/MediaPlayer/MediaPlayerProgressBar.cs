using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    /// <summary>
    /// the public class used as the progress bar object in the media player classes.
    /// Should be used to create region views and maintain the regions correctly.
    /// </summary>
    public class MediaPlayerProgressBar : Canvas
    {
        /// <summary>
        /// the current library element controller of this progress bar.
        /// Can be null;
        /// </summary>
        public AudioLibraryElementController CurrentLibraryElementController { get; private set; }

        private const double PROGRESS_BAR_DEFAULT_HEIGHT = 50;

        private Rectangle _progressBar = new Rectangle();
        private Rectangle _backgroundRectangle = new Rectangle() {Fill = new SolidColorBrush(Colors.Transparent)};
        private RectangleGeometry _clipping = new RectangleGeometry()
        {
            Transform = new TranslateTransform() { Y = - 2 * RegionView.HANDLE_EXTENSION_HEIGHT}
        };

        private IEnumerable<RegionView> _regionViews {
            get
            {
                return Children.OfType<RegionView>();
            }
        }

        public double NormalizedWidth
        {
            get
            {
                Debug.Assert(CurrentLibraryElementController != null);
                return CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration;
            }
        }

        public MediaPlayerProgressBar()
        {
            Children.Add(_progressBar);
            Children.Add(_backgroundRectangle);
            this.Clip = _clipping;
            _progressBar.Fill = new SolidColorBrush(Colors.Chartreuse);
            _progressBar.Height = PROGRESS_BAR_DEFAULT_HEIGHT;
            _backgroundRectangle.Height = PROGRESS_BAR_DEFAULT_HEIGHT;
            Height = PROGRESS_BAR_DEFAULT_HEIGHT;
        }

        public void SetBackgroundColor(Color color, byte opacity)
        {
            color.A = opacity;
            _backgroundRectangle.Fill = new SolidColorBrush(color);
        }

        public void SetLibraryElementController(AudioLibraryElementController libaryElementController)
        {
            Debug.Assert(libaryElementController != null);
            RemoveOldController(CurrentLibraryElementController);
            
            libaryElementController.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            libaryElementController.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;


            CurrentLibraryElementController = libaryElementController;
            _progressBar.Fill = new SolidColorBrush(MediaUtil.GetHashColorFromString(libaryElementController.AudioLibraryElementModel.LibraryElementId)) {Opacity = 65};

            foreach(var model in SessionController.Instance.ContentController.ContentValues.Where(item => item.ContentDataModelId == libaryElementController.AudioLibraryElementModel.ContentDataModelId))
            { 
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryElementId) as AudioLibraryElementController;
                if (controller != null && controller.AudioLibraryElementModel.LibraryElementId != libaryElementController.AudioLibraryElementModel.LibraryElementId)
                {
                    var child = new RegionView(controller, this);
                    Children.Add(child);
                }
            }
            foreach (var region in _regionViews)
            {
                if (!(region.LibraryElementController.AudioLibraryElementModel.NormalizedStartTime < //if the region start and end is not entirely out of view,
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime &&
                    region.LibraryElementController.AudioLibraryElementModel.NormalizedStartTime +
                    region.LibraryElementController.AudioLibraryElementModel.NormalizedDuration >
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime +
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration))
                {
                    Canvas.SetZIndex(region,5);
                }
            }
        }

        private void ContentDataModelOnOnRegionRemoved(string regionLibraryElementModelId)
        {
            foreach (var region in _regionViews)
            {
                if (region?.LibraryElementController?.LibraryElementModel?.LibraryElementId == regionLibraryElementModelId)
                {
                    Children.Remove(region);
                    region?.Dispose();
                    break;
                }
            }
        }

        private void ContentDataModelOnOnRegionAdded(string regionLibraryElementModelId)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementModelId) as AudioLibraryElementController;
            if (controller != null && controller != CurrentLibraryElementController)
            {
                var child = new RegionView(controller, this);
                Children.Add(child);
                if (!(controller.AudioLibraryElementModel.NormalizedStartTime < //if the region start and end is not entirely out of view,
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime &&
                    controller.AudioLibraryElementModel.NormalizedStartTime +
                    controller.AudioLibraryElementModel.NormalizedDuration >
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime +
                    CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration))
                {
                    Canvas.SetZIndex(child, 5);
                }
            }
        }

        private void RemoveOldController(AudioLibraryElementController controller)
        {
            foreach (var region in _regionViews.ToList())
            {
                Children.Remove(region);
                region.Dispose();
            }
            if (controller == null)
            {
                return;
            }
            if (controller.ContentDataController?.ContentDataModel != null)
            {
                controller.ContentDataController.ContentDataModel.OnRegionAdded -= ContentDataModelOnOnRegionAdded;
                controller.ContentDataController.ContentDataModel.OnRegionRemoved -= ContentDataModelOnOnRegionRemoved;
            }
            
        }

        public void SetHeight(double height)
        {
            Height = (double)height;
            _progressBar.Height = height;
            _backgroundRectangle.Height = height;
            _regionViews.ForEach(region => region.SetHeight(height));
        }

        public void SetWidth(double width)
        {
            Width = width;
            _backgroundRectangle.Width = width;
            _clipping.Rect = new Rect() { Height = Height + 4 * RegionView.HANDLE_EXTENSION_HEIGHT, Width = width};
            foreach (var region in _regionViews)
            {
                region?.SetDuration(this, region.LibraryElementController.AudioLibraryElementModel.NormalizedDuration);
                region?.SetLeft(this, region.LibraryElementController.AudioLibraryElementModel.NormalizedStartTime);
            }

        }

        public void Dispose()
        {
            Children.ForEach(view => (view as RegionView)?.Dispose());
        }

        public void UpdateTime(double normalizedTime)
        {
            Debug.Assert(_progressBar != null);
            if (_progressBar == null || double.IsNaN(Width))
            {
                return;
            }
            _progressBar.Width = Math.Max(0,Width*((normalizedTime - CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime)/CurrentLibraryElementController.AudioLibraryElementModel.NormalizedDuration));
        }

        private class RegionView : Canvas
        {
            public static readonly double HANDLE_EXTENSION_HEIGHT = 10;

            private static byte CIRCLE_OPACITY = 160;

            private Color _color;
            private Color _circleColor;

            private Rectangle _leftHitBox = new Rectangle();
            private Rectangle _rightHitBox = new Rectangle();
            private Ellipse _topRightCircle = new Ellipse() ;
            private Ellipse _topLeftCircle = new Ellipse();
            private Ellipse _bottomLeftCircle = new Ellipse();
            private Ellipse _bottomRightCircle = new Ellipse();

            public AudioLibraryElementController LibraryElementController;
            private MediaPlayerProgressBar _progressBar;
            public RegionView(AudioLibraryElementController controller, MediaPlayerProgressBar progressBar)
            {
                Debug.Assert(controller != null && progressBar != null);

                _color = MediaUtil.GetHashColorFromString(controller.LibraryElementModel.LibraryElementId);
                _color.A = 50;

                _circleColor = _color;
                _circleColor.A = CIRCLE_OPACITY;

                LibraryElementController = controller;
                _progressBar = progressBar;

                Width = ((controller.AudioLibraryElementModel.NormalizedDuration) * progressBar.Width) / (progressBar.NormalizedWidth);
                Height = progressBar.Height;
                Background = new SolidColorBrush(_color);

                _leftHitBox = new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Height = progressBar.Height + 4 * HANDLE_EXTENSION_HEIGHT,
                    Width = 3*HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = - 2 * HANDLE_EXTENSION_HEIGHT, X = -1.5*HANDLE_EXTENSION_HEIGHT }
                };

                _rightHitBox = new Rectangle()
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Height = progressBar.Height + 4 * HANDLE_EXTENSION_HEIGHT,
                    Width = 3 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = - 2 * HANDLE_EXTENSION_HEIGHT, X = this.Width - 1.5* HANDLE_EXTENSION_HEIGHT}
                };

                _topLeftCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(_circleColor),
                    Height = 2* HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT , X = -HANDLE_EXTENSION_HEIGHT}
                };

                _topRightCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(_circleColor) ,
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT, X =  this.Width -  HANDLE_EXTENSION_HEIGHT}
                };

                _bottomLeftCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(_circleColor),
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = progressBar.Height - HANDLE_EXTENSION_HEIGHT, X = -HANDLE_EXTENSION_HEIGHT }
                };

                _bottomRightCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(_circleColor),
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = progressBar.Height - HANDLE_EXTENSION_HEIGHT, X = this.Width - HANDLE_EXTENSION_HEIGHT }
                };
                
                Children.Add(_topRightCircle);
                Children.Add(_topLeftCircle);
                Children.Add(_bottomLeftCircle);
                Children.Add(_bottomRightCircle);
                Children.Add(_leftHitBox);
                Children.Add(_rightHitBox);

                RenderTransform = new TranslateTransform();
                SetLeft(this,LibraryElementController.AudioLibraryElementModel.NormalizedStartTime);


                _leftHitBox.ManipulationMode = ManipulationModes.All;
                _rightHitBox.ManipulationMode = ManipulationModes.All;
                ManipulationMode = ManipulationModes.All;

                _leftHitBox.ManipulationDelta += LeftHitBoxOnManipulationDelta;
                _rightHitBox.ManipulationDelta += RightHitBoxOnManipulationDelta;
                ManipulationDelta += OnManipulationDelta;
                DoubleTapped += OnDoubleTapped;

                _leftHitBox.PointerPressed += OnPointerPressed;
                _rightHitBox.PointerPressed += OnPointerPressed;

                PointerPressed += OnPointerPressed;
                PointerReleased += OnPointerReleased;

                LibraryElementController.DurationChanged += SetDuration;
                LibraryElementController.TimeChanged += SetLeft;
            }

            private void OnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
            {
                SessionController.Instance.SessionView.FreeFormViewer.Unfreeze();
            }

            private void OnPointerPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
            {
                SessionController.Instance.SessionView.FreeFormViewer.Freeze();
                CapturePointer(pointerRoutedEventArgs.Pointer);
            }

            private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
            {
                SessionController.Instance.SessionView.DetailViewerView.ShowElement(LibraryElementController,DetailViewTabType.Home);
            }


            private void RightHitBoxOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
            {
                var scale = ((_progressBar?.Parent as Canvas)?.RenderTransform as CompositeTransform)?.ScaleX ?? 1;
                var delta = manipulationDeltaRoutedEventArgs.Delta.Translation.X / scale;
                UpdateDuration(delta);
                manipulationDeltaRoutedEventArgs.Handled = true;
            }

            private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
            {
                var scale = ((_progressBar?.Parent as Canvas)?.RenderTransform as CompositeTransform)?.ScaleX ?? 1;
                var delta = manipulationDeltaRoutedEventArgs.Delta.Translation.X / scale;
                UpdateLeft(delta);
                manipulationDeltaRoutedEventArgs.Handled = true;
            }

            private void LeftHitBoxOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
            {
                var scale = ((_progressBar?.Parent as Canvas)?.RenderTransform as CompositeTransform)?.ScaleX ?? 1;
                var delta = manipulationDeltaRoutedEventArgs.Delta.Translation.X / scale;
                UpdateDuration(-delta);
                UpdateLeft(delta);
                manipulationDeltaRoutedEventArgs.Handled = true;
            }

            private void UpdateDuration(double delta)
            {
                var newNormalizedDuration = ((Width + delta) / _progressBar.Width) * _progressBar.NormalizedWidth;
                LibraryElementController.SetDuration(newNormalizedDuration);
            }

            private void UpdateLeft(double delta)
            {
                var newNormalizedStart = ((((RenderTransform as TranslateTransform).X + delta) / _progressBar.Width) * _progressBar.NormalizedWidth) +
                         _progressBar.CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime;
                if (newNormalizedStart + LibraryElementController.AudioLibraryElementModel.NormalizedDuration <= 1)
                {
                    LibraryElementController.SetStartTime(newNormalizedStart);
                }
            }

            public void Dispose()
            {
                LibraryElementController.DurationChanged -= SetDuration;
                LibraryElementController.TimeChanged -= SetLeft;
                _leftHitBox.ManipulationDelta -= LeftHitBoxOnManipulationDelta;
                _rightHitBox.ManipulationDelta -= RightHitBoxOnManipulationDelta;
                ManipulationDelta -= OnManipulationDelta;
                DoubleTapped -= OnDoubleTapped;

                _leftHitBox.PointerPressed -= OnPointerPressed;
                _rightHitBox.PointerPressed -= OnPointerPressed;
                PointerPressed -= OnPointerPressed;
                PointerReleased -= OnPointerReleased;

                _leftHitBox = null;
                _rightHitBox = null;
                _bottomLeftCircle = null;
                _topRightCircle = null;
                _bottomRightCircle = null;
                _topLeftCircle = null;
            }

            public void SetLeft(object sender, double newNormalizedStartTime)
            {
                Debug.Assert(RenderTransform is TranslateTransform);
                Debug.Assert(_progressBar?.CurrentLibraryElementController?.AudioLibraryElementModel != null);
                (RenderTransform as TranslateTransform).X = (newNormalizedStartTime - _progressBar.CurrentLibraryElementController.AudioLibraryElementModel.NormalizedStartTime) * (_progressBar.Width/_progressBar.NormalizedWidth);
            }

            public void SetDuration(object sender, double newNormalizedDuration)
            {
                Width = ((newNormalizedDuration) * _progressBar.Width) / (_progressBar.NormalizedWidth);

                var topRightCircleTransform = _topRightCircle.RenderTransform as TranslateTransform;
                var bottomRightCircleTransform = _bottomRightCircle.RenderTransform as TranslateTransform;
                var rightHitBoxTransform = _rightHitBox.RenderTransform as TranslateTransform;

                Debug.Assert(topRightCircleTransform != null);
                Debug.Assert(bottomRightCircleTransform != null);
                Debug.Assert(rightHitBoxTransform != null);

                var newX = this.Width - HANDLE_EXTENSION_HEIGHT;

                rightHitBoxTransform.X = this.Width - 1.5* HANDLE_EXTENSION_HEIGHT;
                bottomRightCircleTransform.X = newX;
                topRightCircleTransform.X = newX;
            }

            public void SetHeight(double height)
            {
                Height = height;

                var bottomRightCircleTransform = _bottomRightCircle.RenderTransform as TranslateTransform;
                var bottomLeftCircleTransform = _bottomLeftCircle.RenderTransform as TranslateTransform;

                Debug.Assert(bottomLeftCircleTransform != null);
                Debug.Assert(bottomRightCircleTransform != null);

                bottomLeftCircleTransform.Y = height - HANDLE_EXTENSION_HEIGHT;
                bottomRightCircleTransform.Y = height - HANDLE_EXTENSION_HEIGHT;

                _leftHitBox.Height = _progressBar.Height + 4*HANDLE_EXTENSION_HEIGHT;
                _rightHitBox.Height = _progressBar.Height + 4 * HANDLE_EXTENSION_HEIGHT;
            }
        }
    }

}
