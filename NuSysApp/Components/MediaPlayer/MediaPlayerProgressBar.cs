using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
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

        private static readonly double PROGRESS_BAR_HEIGHT = 50;

        private Rectangle _progressBar = new Rectangle();

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
            _progressBar.Fill = new SolidColorBrush(Colors.Chartreuse);
            _progressBar.Height = PROGRESS_BAR_HEIGHT;
        }

        public void SetLibraryElementController(AudioLibraryElementController libaryElementController)
        {
            RemoveOldController(CurrentLibraryElementController);
            CurrentLibraryElementController = libaryElementController;
            _progressBar = _progressBar ?? new Rectangle();

            foreach(var model in SessionController.Instance.ContentController.ContentValues.Where(item => item.ContentDataModelId == libaryElementController.AudioLibraryElementModel.ContentDataModelId))
            { 
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryElementId) as AudioLibraryElementController;
                if (controller != null && controller != libaryElementController)
                {
                    var child = new RegionView(controller, this);
                    Children.Add(child);
                }
            }
        }

        private void RemoveOldController(AudioLibraryElementController controller)
        {
            _regionViews.Where(item => Children.Remove(item)).ForEach(item => item.Dispose());//remove and dispose existing regions
            if (controller == null)
            {
                return;
            }

        }

        public void SetSize(double width)
        {
            Width = width;
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
            _progressBar.Width = Width*normalizedTime;
            if ((int) (normalizedTime*10)%1 == 0)
            {
                //TESTING SHIT
            }
        }

        private class RegionView : Canvas
        {
            private static readonly double HANDLE_EXTENSION_HEIGHT = 10;
            private static readonly Color COLOR = Color.FromArgb(100,39,244,65);

            private Rectangle _leftHitBox = new Rectangle();
            private Rectangle _rightHitBox = new Rectangle();
            private Ellipse _topRightCircle = new Ellipse() ;
            private Ellipse _topLeftCircle = new Ellipse();
            private Ellipse _bottomLeftCircle = new Ellipse();
            private Ellipse _bottomRightCircle = new Ellipse();

            private AudioLibraryElementController _libraryElementController;
            private MediaPlayerProgressBar _progressBar;
            public RegionView(AudioLibraryElementController controller, MediaPlayerProgressBar progressBar)
            {
                _libraryElementController = controller;
                _progressBar = progressBar;

                Width = ((controller.AudioLibraryElementModel.NormalizedDuration) * progressBar.Width) / (progressBar.NormalizedWidth);
                Height = PROGRESS_BAR_HEIGHT;
                Background = new SolidColorBrush(COLOR);

                _leftHitBox = new Rectangle()
                {
                    Height = PROGRESS_BAR_HEIGHT + 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2*HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT }
                };

                _rightHitBox = new Rectangle()
                {
                    Height = PROGRESS_BAR_HEIGHT + 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT, X = this.Width - 2 * HANDLE_EXTENSION_HEIGHT}
                };

                _topLeftCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(COLOR),
                    Height = 2* HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT }
                };

                _topRightCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(COLOR),
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = -HANDLE_EXTENSION_HEIGHT, X =  this.Width - 2 * HANDLE_EXTENSION_HEIGHT}
                };

                _bottomLeftCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(COLOR),
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = PROGRESS_BAR_HEIGHT - HANDLE_EXTENSION_HEIGHT}
                };

                _bottomRightCircle = new Ellipse()
                {
                    Fill = new SolidColorBrush(COLOR),
                    Height = 2 * HANDLE_EXTENSION_HEIGHT,
                    Width = 2 * HANDLE_EXTENSION_HEIGHT,
                    RenderTransform = new TranslateTransform() { Y = PROGRESS_BAR_HEIGHT - HANDLE_EXTENSION_HEIGHT, X = this.Width - 2 * HANDLE_EXTENSION_HEIGHT }
                };

                Children.Add(_leftHitBox);
                Children.Add(_rightHitBox);
                Children.Add(_topRightCircle);
                Children.Add(_topLeftCircle);
                Children.Add(_bottomLeftCircle);
                Children.Add(_bottomRightCircle);

                SetLeft(this,_libraryElementController.AudioLibraryElementModel.NormalizedStartTime);

                _libraryElementController.DurationChanged += SetDuration;
                _libraryElementController.TimeChanged += SetLeft;
            }

            public void Dispose()
            {
                _libraryElementController.DurationChanged -= SetDuration;
                _libraryElementController.TimeChanged -= SetLeft;
            }

            public void SetLeft(object sender, double newNormalizedStartTime)
            {
                Debug.Assert(RenderTransform is TranslateTransform);
                (RenderTransform as TranslateTransform).X = newNormalizedStartTime * Width;
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

                var newX = this.Width - 2 * HANDLE_EXTENSION_HEIGHT;

                rightHitBoxTransform.X = newX;
                bottomRightCircleTransform.X = newX;
                topRightCircleTransform.X = newX;
            }
        }
    }

}
