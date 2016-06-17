using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageRegionView : UserControl
    {

        public double TempWidth { set; get; }
        public double TempHeight { set; get; }
        public ImageDetailHomeTabView RegionView { set; get; }

        public Point _topLeft;
        public Point _bottomRight;



        public delegate void RegionSelectedEventHandler(ImageRegionView sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public RectangleRegion RectangleRegion { set; get; }
        public ImageRegionView(RectangleRegion region, ImageDetailHomeTabView contentView)

        {

            this.InitializeComponent();

            RectangleRegion = region;
            RegionView = contentView;

            
            xMainRectangle.Width = (RectangleRegion.Point2.X - RectangleRegion.Point1.X) * RegionView.ActualWidth;
            xMainRectangle.Height = (RectangleRegion.Point2.Y - RectangleRegion.Point1.Y) * RegionView.ActualHeight;
            TempWidth = Width;
            TempHeight = Height;

            _topLeft = RectangleRegion.Point1;
            _bottomRight = RectangleRegion.Point2;

            //this.Selected();
            this.RenderTransform = new CompositeTransform();


        }

        private void XResizingTriangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            
        }

        private void XResizingTriangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            //  if (xMainRectangle.Width + e.Delta.Translation.X > 50 && (xMainRectangle.Width + e.Delta.Translation.X <= RegionView.ActualWidth))
            if (xMainRectangle.Width + e.Delta.Translation.X > 50 && (_bottomRight.X + e.Delta.Translation.X / RegionView.ActualWidth)<=1)
            {
                xMainRectangle.Width += e.Delta.Translation.X;
                _bottomRight.X += e.Delta.Translation.X / RegionView.ActualWidth;

                //((CompositeTransform)this.RenderTransform).TranslateX += e.Delta.Translation.X / 2;


            }

            //if (xMainRectangle.Height + e.Delta.Translation.Y > 50 && (xMainRectangle.Height + e.Delta.Translation.Y <= RegionView.ActualHeight))
            if (xMainRectangle.Height + e.Delta.Translation.Y > 50 && (_bottomRight.Y + e.Delta.Translation.Y / RegionView.ActualHeight) <= 1)
            {
                xMainRectangle.Height += e.Delta.Translation.Y;
                _bottomRight.Y += e.Delta.Translation.Y / RegionView.ActualHeight;


            }

            RectangleRegion.Point2 = _bottomRight;



        }

        private void XResizingTriangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            //this.Select();
            //OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            if (_topLeft.X + e.Delta.Translation.X / RegionView.ActualWidth >= 0 && _bottomRight.X + e.Delta.Translation.X / RegionView.ActualWidth <= 1)
            {
                ((CompositeTransform)this.RenderTransform).TranslateX += e.Delta.Translation.X;
                _bottomRight.X += e.Delta.Translation.X / RegionView.ActualWidth;
                _topLeft.X += e.Delta.Translation.X / RegionView.ActualWidth;
            }
            if (_topLeft.Y + e.Delta.Translation.Y / RegionView.ActualHeight >= 0 && _bottomRight.Y + e.Delta.Translation.Y / RegionView.ActualHeight <= 1)
            {
                ((CompositeTransform)this.RenderTransform).TranslateY += e.Delta.Translation.Y;
                _bottomRight.Y += e.Delta.Translation.Y / RegionView.ActualHeight;
                _topLeft.Y += e.Delta.Translation.Y / RegionView.ActualHeight;

            }
            RectangleRegion.Point1 = _topLeft;
            RectangleRegion.Point2 = _bottomRight;
            
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            //OnSelected?.Invoke(this, true);

            //this.Select();
            e.Handled = true;

        }

        public void Deselect()
        {
            xMainRectangle.StrokeThickness = 3;
            xMainRectangle.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            xResizingTriangle.Visibility = Visibility.Collapsed;

        }
        
        public void Select()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
            xResizingTriangle.Visibility = Visibility.Visible;

        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //OnSelected?.Invoke(this, true);
            //xMainRectangle.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
            //this.Select();

        }

        public void ApplyNewSize(Size s)
        {
            (this.RenderTransform as CompositeTransform).TranslateX = s.Width * _topLeft.X;
            (this.RenderTransform as CompositeTransform).TranslateY = s.Height * _topLeft.Y;
            xMainRectangle.Height = s.Height * (_bottomRight.Y - _topLeft.Y);
            xMainRectangle.Width = s.Width * (_bottomRight.X - _topLeft.X);
        }
    }
}
