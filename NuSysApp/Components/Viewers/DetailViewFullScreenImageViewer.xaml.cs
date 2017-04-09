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
            var point = xImage.TransformToVisual(Window.Current.Content);
            Point center = new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2);
            Point screenCoord = point.TransformPoint(center);

            RotateTransform rotate = new RotateTransform
            {
                CenterX = screenCoord.X, CenterY = screenCoord.Y,
                Angle = 0
            };

            TranslateTransform translate = new TranslateTransform();
            ScaleTransform scale = new ScaleTransform
            {
                CenterX = screenCoord.X,
                CenterY = screenCoord.Y
            };

            double scalefactor;
            var screenRatio = xCanvas.ActualWidth / xCanvas.ActualHeight;

            if (xImage.ActualWidth > xImage.ActualHeight * screenRatio)
            {
                scalefactor = xCanvas.ActualWidth / xImage.ActualWidth;
                scalefactor /= 2;
                scale.ScaleX = scalefactor;
                scale.ScaleY = scalefactor;
                translate.X = xCanvas.ActualWidth * .25;
                translate.Y = xCanvas.ActualHeight * .25;
            }
            else
            {
                scalefactor = xCanvas.ActualHeight / xImage.ActualHeight;
                scalefactor /= 2;
                scale.ScaleX = scalefactor;
                scale.ScaleY = scalefactor;
//                translate.X = (xCanvas.ActualWidth - (xImage.ActualWidth * scalefactor)) / 2;
                translate.X = xCanvas.ActualWidth * .25;
                translate.Y = xCanvas.ActualHeight * .25;
            }
            
            TransformGroup composite = new TransformGroup();
            composite.Children.Add(translate);
            composite.Children.Add(rotate);
            composite.Children.Add(scale);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };

            // 
            /* 
            (xImage.RenderTransform as TransformGroup).Children[0] = new CompositeTransform();
            var transform = (xImage.RenderTransform as TransformGroup)?.Children?.First() as CompositeTransform;

            Debug.Assert(transform != null);
            if (transform == null)
            {
                return;
            }
            double scale;
            var screenRatio = xCanvas.ActualWidth/xCanvas.ActualHeight;

            if (xImage.ActualWidth > xImage.ActualHeight * screenRatio)
            {
                scale = xCanvas.ActualWidth/xImage.ActualWidth;
                scale /= 2;
                transform.ScaleX = scale;
                transform.ScaleY = scale;
                transform.TranslateX = xCanvas.ActualWidth * .25;
                transform.TranslateY = xCanvas.ActualHeight * .25;
            }
            else
            {
                scale = xCanvas.ActualHeight / xImage.ActualHeight;
                scale /= 2;
                transform.ScaleX = scale;
                transform.ScaleY = scale;
                transform.TranslateX = (xCanvas.ActualWidth - (xImage.ActualWidth * scale))/2;
                transform.TranslateY = xCanvas.ActualHeight * .25;
            }
            //transform.CenterX = xCanvas.ActualWidth / 2;
            //transform.CenterY = xCanvas.ActualHeight / 2;
            transform.Rotation = 0;
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
            var tg = new TransformGroup();
            tg.Children.Add(new CompositeTransform());
            tg.Children.Add(new CompositeTransform());
            tg.Children.Add(new CompositeTransform());
            tg.Children.Add(new CompositeTransform());
            xImage.RenderTransform = tg ;

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
            Point center = new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2); 
            Point screenCoord = point.TransformPoint(center);

            TranslateTransform translate = new TranslateTransform
            {
                X = e.Delta.Translation.X,
                Y = e.Delta.Translation.Y
            };

            ScaleTransform scale = new ScaleTransform
            {
                CenterX = screenCoord.X, // e.Position.X,
                CenterY = screenCoord.Y, // e.Position.Y,
                ScaleX = e.Delta.Scale, ScaleY = e.Delta.Scale
            };

            RotateTransform rotate = new RotateTransform
            {
                CenterX = screenCoord.X, // e.Position.X,
                CenterY = screenCoord.Y, // e.Position.Y,
                Angle = e.Delta.Rotation
            };

            TransformGroup composite = new TransformGroup();
            composite.Children.Add(xImage.RenderTransform);
            composite.Children.Add(translate);
            composite.Children.Add(rotate);
            composite.Children.Add(scale);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };
           

            ////////////////////////// 

            /* 
            var transform = (xImage.RenderTransform as TransformGroup)?.Children?.First() as CompositeTransform;
            Debug.Assert(transform != null);

            /*
            //var transPoint = transform.Inverse.TransformPoint(e.Position);

            transform.CenterX = e.Position.X / transform.ScaleX;
            transform.CenterY = e.Position.Y / transform.ScaleY;

            transform.TranslateX += e.Delta.Translation.X;// - ((1-transform.ScaleX) * e.Position.X);
            transform.TranslateY += e.Delta.Translation.Y;// - ((1 - transform.ScaleY) * e.Position.Y);

            transform.ScaleX *= e.Delta.Scale;
            transform.ScaleY *= e.Delta.Scale;
            //transform.Rotation += e.Delta.Rotation;
            */

            /* 
            var compositeTransform = transform;

            var center = e.Position;

            var tmpTranslate = new CompositeTransform()
            {
                TranslateX = compositeTransform.CenterX,
                TranslateY = compositeTransform.CenterY,
                Rotation = compositeTransform.Rotation,
                ScaleX = compositeTransform.ScaleX,
                ScaleY = compositeTransform.ScaleY
            };

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //Transform local space into world space again

            //...and balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift
            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;



            /* //////////////////  
            TranslateTransform translate = new TranslateTransform
            {
                X = compositeTransform.TranslateX,
                Y = compositeTransform.TranslateY
            };

            ScaleTransform scale = new ScaleTransform
            {
                CenterX = center.X, // e.Position.X,
                CenterY = center.Y, // e.Position.Y,
                ScaleX = compositeTransform.ScaleX,
                ScaleY = compositeTransform.ScaleY
            };

            TransformGroup composite = new TransformGroup();
            composite.Children.Add(xImage.RenderTransform);
            composite.Children.Add(translate);
           // composite.Children.Add(rotate);
            composite.Children.Add(scale);
            xImage.RenderTransform = new MatrixTransform { Matrix = composite.Value };
            */
            ///////////////////////// 


            /*                                                                                                  why do we even have this lever 
            var start = (xImage.RenderTransform as TransformGroup).Children[1] as CompositeTransform;
            start.CenterX = e.Position.X;
            start.CenterY = e.Position.Y;

            var rotation = (xImage.RenderTransform as TransformGroup).Children[2] as CompositeTransform;
            //rotate.Rotation += e.Delta.Rotation;  

            var end = (xImage.RenderTransform as TransformGroup).Children[3] as CompositeTransform;
            */

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
        //    var center = e.GetCurrentPoint(xImage).Position;
            var point = xImage.TransformToVisual(Window.Current.Content);
            Point center = new Point(xImage.ActualWidth / 2, xImage.ActualHeight / 2);
            Point screenCoord = point.TransformPoint(center);
            
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

           // */ 
            /* 
            var transform = (xImage.RenderTransform as TransformGroup)?.Children?.First() as CompositeTransform;
            var delta = e.GetCurrentPoint(xImage).Properties.MouseWheelDelta;
            var compositeTransform = transform;
            var center = e.GetCurrentPoint(xImage).Position;
            var tmpTranslate = new CompositeTransform()
            {
                TranslateX = compositeTransform.CenterX,
                TranslateY = compositeTransform.CenterY,
                Rotation = compositeTransform.Rotation,
                ScaleX = compositeTransform.ScaleX,
                ScaleY = compositeTransform.ScaleY
            };

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);
            if (delta > 0)
            {
                transform.ScaleX *= 1.2;
                transform.ScaleY *= 1.2;
            }
            if (delta < 0)
            {
                transform.ScaleX /= 1.2;
                transform.ScaleY /= 1.2;
            }
            transform.TranslateX += distance.X;
            transform.TranslateY += distance.Y;
            transform.CenterX = center.X;
            transform.CenterY = center.Y;

            */ 
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
