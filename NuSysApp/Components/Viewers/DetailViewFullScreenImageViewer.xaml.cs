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
            var transform = xImage.RenderTransform as CompositeTransform;
            Debug.Assert(transform != null);
            transform.ScaleX = 1;
            transform.ScaleY = 1;
            transform.TranslateX = (xCanvas.Width - xImage.ActualWidth)/2;
            transform.TranslateY = (xCanvas.Height - xImage.ActualHeight) / 2;
            transform.Rotation = 0;
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
            
        }
        /// <summary>
        /// Method to set the image curently being displayed
        /// </summary>
        /// <param name="imageUri"></param>
        public void ShowImage(Uri imageUri)
        {
            SetCanvasSize();
            Visibility = Visibility.Visible;
            xImage.Source = new BitmapImage(imageUri);
            xImage.RenderTransform = new CompositeTransform();
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
            ImageClosed?.Invoke(this,EventArgs.Empty);
        }

        private void XCanvas_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var transform = xImage.RenderTransform as CompositeTransform;
            Debug.Assert(transform != null);
        }

        private void XCanvas_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var transform = xImage.RenderTransform as CompositeTransform;
            Debug.Assert(transform != null);

            /*
            //var transPoint = transform.Inverse.TransformPoint(e.Position);

            transform.CenterX = e.Position.X / transform.ScaleX;
            transform.CenterY = e.Position.Y / transform.ScaleY;

            transform.TranslateX += e.Delta.Translation.X;//- ((1-transform.ScaleX) * e.Position.X);
            transform.TranslateY += e.Delta.Translation.Y;// - ((1 - transform.ScaleY) * e.Position.Y);

            transform.ScaleX *= e.Delta.Scale;
            transform.ScaleY *= e.Delta.Scale;
            //transform.Rotation += e.Delta.Rotation;*/


            var compositeTransform = transform;



            var center = e.Position;

            var tmpTranslate = new CompositeTransform()
            {
                TranslateX  = compositeTransform.CenterX,
                TranslateY = compositeTransform.CenterY,
                Rotation =  compositeTransform.Rotation,
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
        }

        private void XImage_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            var transform = xImage.RenderTransform as CompositeTransform;
            Debug.Assert(transform != null);

            //transform.CenterX = e.Position.X;
            //transform.CenterY = e.Position.Y;


            //transform.TranslateX += transform.CenterX;
            //transform.TranslateY += transform.CenterY;
        }
    }
}
