using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.IO.Serialization;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class DetailViewFullScreenImageViewer : UserControl
    {
        /// <summary>
        /// Event fired whenever the user closes the image;
        /// </summary>
        public event EventHandler ImageClosed;


        private List<Uri> _currentListOfImageUris;
        private int _indexOfUri;

        // CHANGED: clip 
        private Size _xImageSize;   // size of cropped image 
        private Point _xImagePoint; // point on Image where cropping starts; (0,0) = upper left of image 

        // used for the clipping 
        public Size localSize { get; set; } 
        public Point localPoint { get; set; }

        public double ActualX { get; private set; }

        /// <summary>
        /// Parameterless constructor jsut initializes the components
        /// </summary>
        public DetailViewFullScreenImageViewer()
        {
            this.InitializeComponent();
            SizeChanged += FreeFormViewerOnSizeChanged;
            var color = Colors.Gray;
            color.A = 210;
            xCanvas.Background = new SolidColorBrush(color);
            xCanvas.DoubleTapped += DoubleTapped;
            xImage.ImageOpened += XImageOnOpened;

            // CHANGED: clip 
            _xImageSize = new Size(xImage.ActualWidth, xImage.ActualHeight);
        }

        /// <summary>
        /// dispose method shouldn't need to be called, but will correectly dispose and remoev handlers if this class is disposed
        /// </summary>
        public void Dispose()
        {
            xCanvas.DoubleTapped -= DoubleTapped;
            xImage.Loaded -= XImageOnOpened;
        }

        /// <summary>
        /// event handler called whenever the image loads a new image;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void XImageOnOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            ResetImage();

            // CHANGED: clip 
            // TODO get the actual localPoint and localSize; replace below 
         //   Point localPoint = new Point(0.5, 0);             // normalized point within xImage; x,y from 0-1
         //   Size localSize = new Size(0.5, 0.5);            // normalized size within xImage 

            Size size = new Size(localSize.Width * xImage.ActualWidth, localSize.Height * xImage.ActualHeight);
            Point point = new Point(localPoint.X * xImage.ActualWidth, localPoint.Y * xImage.ActualHeight); 

            RectangleGeometry clipRect = new RectangleGeometry
            {
                Rect = new Rect(point, size)
            };
            xImage.Clip = clipRect;

            _xImageSize = size;
            _xImagePoint = point; 

            ResetImage();
        }

        /// <summary>
        /// Event fired when image or canvas is double tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doubleTappedRoutedEventArgs"></param>
        private void DoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
        {
            ResetImage();
        }


        /// <summary>
        /// private method to put the image back in the middle of the screen at usual size
        /// </summary>
        private void ResetImage()
        {
       //     /* 
            // CHANGED: clip 
            var point = xImage.TransformToVisual(Window.Current.Content);
            Point screenCoord = point.TransformPoint(new Point(_xImagePoint.X + _xImageSize.Width / 2, _xImagePoint.Y + _xImageSize.Height / 2));

            RotateTransform rotate = new RotateTransform
            {
                CenterX = screenCoord.X,
                CenterY = screenCoord.Y,
                Angle = 0
            };

            TranslateTransform translate = new TranslateTransform
            {
                X = (xCanvas.ActualWidth - _xImageSize.Width) * .5 - _xImagePoint.X,
                Y = (xCanvas.ActualHeight - _xImageSize.Height) * .5 - _xImagePoint.Y
            };


            TransformGroup composite = new TransformGroup();
            composite.Children.Add(translate);
            composite.Children.Add(rotate);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };

          //  */
                /* 
            var point = xImage.TransformToVisual(Window.Current.Content);
            Point screenCoord = point.TransformPoint(new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2));

            RotateTransform rotate = new RotateTransform
            {
                CenterX = screenCoord.X, CenterY = screenCoord.Y,
                Angle = 0
            };
            
            TranslateTransform translate = new TranslateTransform
            {
                X = (xCanvas.ActualWidth - xImage.ActualWidth) * .5,
                Y = (xCanvas.ActualHeight - xImage.ActualHeight)*.5
            };
          
            
            TransformGroup composite = new TransformGroup();
            composite.Children.Add(translate);
            composite.Children.Add(rotate);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };
            */ 
        }

        /// <summary>
        /// Method called whenever the detail view changes size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sizeChangedEventArgs"></param>
        private void FreeFormViewerOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            SetCanvasSize();
        }

        /// <summary>
        /// Method to uopdate the canvas size.
        /// </summary>
        private void SetCanvasSize()
        {
            xCanvas.Height = SessionController.Instance.SessionView.FreeFormViewer.Height;
            xCanvas.Width = SessionController.Instance.SessionView.FreeFormViewer.Width;
            Canvas.SetTop(xCloseButton, ActualHeight - xCloseButton.Height - 15);
            Canvas.SetLeft(xCloseButton, ActualWidth/2 - xCloseButton.Width/2);

            Canvas.SetTop(xLeftButton, ActualHeight/2 - xLeftButton.Height/2);
            Canvas.SetLeft(xLeftButton, 15);
            Canvas.SetTop(xRightButton, ActualHeight/2 - xLeftButton.Height/2);
            Canvas.SetLeft(xRightButton, ActualWidth - xLeftButton.Width - 15);

        }
        /// <summary>
        /// Method to set the image curently being displayed
        /// </summary>
        /// <param name="imageUri"></param>
        public void ShowImage(List<Uri> imageUris, int index, bool pages)
        {
            SetCanvasSize();

            xLeftButton.Visibility = pages & index > 0 ? Visibility.Visible : Visibility.Collapsed;
            xRightButton.Visibility = pages & index < imageUris.Count - 1? Visibility.Visible : Visibility.Collapsed;

            //Update reference to list of URIs
            _currentListOfImageUris = imageUris;
            _indexOfUri = index;

            Visibility = Visibility.Visible;
            xImage.Source = new BitmapImage(imageUris[index]);
            xImage.Stretch=Stretch.UniformToFill;

            ResetImage();
        }


        /// <summary>
        /// Method for when the close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            xImage.Source = null;
            ImageClosed?.Invoke(this, EventArgs.Empty);
        }

        private void XCanvas_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
             //var transform = (xImage.RenderTransform as TransformGroup)?.Children?.First() as CompositeTransform;
            // Debug.Assert(transform != null);                                                                                 // uncomment these?  

        }

        private void XCanvas_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var point = xImage.TransformToVisual(Window.Current.Content);
            // Point screenCoord = point.TransformPoint(new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2));
            Point screenCoord = point.TransformPoint(new Point(_xImagePoint.X + _xImageSize.Width / 2, _xImagePoint.Y + _xImageSize.Height / 2));

            TranslateTransform translate = new TranslateTransform
            {
                X = e.Delta.Translation.X,
                Y = e.Delta.Translation.Y
            };

            ScaleTransform scale = new ScaleTransform
            {
                CenterX = screenCoord.X, 
                CenterY = screenCoord.Y,
                ScaleX = e.Delta.Scale, ScaleY = e.Delta.Scale
            };

            RotateTransform rotate = new RotateTransform
            {
                CenterX = screenCoord.X, 
                CenterY = screenCoord.Y, 
                Angle = e.Delta.Rotation
            };

            TransformGroup composite = new TransformGroup();
            composite.Children.Add(xImage.RenderTransform);
            composite.Children.Add(translate);
            composite.Children.Add(rotate);
            composite.Children.Add(scale);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };

        }

        private void XImage_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // var transform = (xImage.RenderTransform as TransformGroup)?.Children?.First() as CompositeTransform;
            // Debug.Assert(transform != null);                                                                                 // uncomment these  

            //transform.CenterX = e.Position.X;
            //transform.CenterY = e.Position.Y;


            //transform.TranslateX += transform.CenterX;
            //transform.TranslateY += transform.CenterY;
        }

        private void XImage_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            //  /* 
            var delta = e.GetCurrentPoint(xImage).Properties.MouseWheelDelta;
            var point = xImage.TransformToVisual(Window.Current.Content);
//            Point screenCoord = point.TransformPoint(new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2));
            Point screenCoord = point.TransformPoint(new Point(_xImagePoint.X + _xImageSize.Width / 2, _xImagePoint.Y + _xImageSize.Height / 2));

            ScaleTransform scale = new ScaleTransform
            {
                CenterX = screenCoord.X, CenterY = screenCoord.Y
            };

            if (delta > 0)
            {
                scale.ScaleX = 1.2;
                scale.ScaleY = 1.2;
            }
            if (delta < 0)            
            {
                scale.ScaleX = 0.83333;
                scale.ScaleY = 0.83333;
            }

            TransformGroup composite = new TransformGroup();
            composite.Children.Add(xImage.RenderTransform);
            composite.Children.Add(scale);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };

        }

        private void XLeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_currentListOfImageUris != null);
            var newIndex = _indexOfUri - 1;
            if (newIndex >= 0)
            {
                ShowImage(_currentListOfImageUris, newIndex, true);

            }
        }

        private void XRightButton_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_currentListOfImageUris != null);
            var newIndex = _indexOfUri + 1;
            if (newIndex < _currentListOfImageUris.Count)
            {
                ShowImage(_currentListOfImageUris, newIndex, true);
            }
        }
    }
}
