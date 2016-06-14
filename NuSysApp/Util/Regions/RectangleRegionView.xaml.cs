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
    public sealed partial class RectangleRegionView : UserControl
    {

        public double TempWidth { set; get; }
        public double TempHeight { set; get; }

        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public RectangleRegion RectangleRegion { set; get; }
        public RectangleRegionView(RectangleRegion region)

        {

            this.InitializeComponent();

            RectangleRegion = region;
            Canvas.SetLeft(this, region.Point1.X);
            Canvas.SetTop(this, region.Point1.Y);

            this.Width = region.Point2.X - region.Point1.X;
            this.Height = region.Point2.Y - region.Point1.Y;
            TempWidth = Width;
            TempHeight = Height;

            this.Selected();
            xMainRectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            xMainRectangle.VerticalAlignment = VerticalAlignment.Stretch;
            xMainRectangle.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);

            this.RenderTransform = new CompositeTransform();
            xMainRectangle.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            xMainRectangle.ManipulationStarted += RectangleRegionView_ManipulationStarted;
            xMainRectangle.ManipulationDelta += RectangleRegionView_ManipulationDelta;

            xResizingRectangle.RenderTransform = new CompositeTransform();
            xResizingRectangle.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            xResizingRectangle.ManipulationStarted += XResizingRectangle_ManipulationStarted;
            xResizingRectangle.ManipulationDelta += XResizingRectangle_ManipulationDelta;


            OnSelected?.Invoke(this, true);
            //var t = (CompositeTransform)rect.RenderTransform;


            //t.TranslateX += e.Delta.Translation.X;
            //t.TranslateY += e.Delta.Translation.Y;

            //_x += e.Delta.Translation.X;
            //_y += e.Delta.Translation.Y;

            //_propertiesWindow.Visibility = Visibility.Collapsed;

            //this.RenderTransfor

        }

        private void XResizingRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            //var originalX = Canvas.GetLeft(this);
            //var originalY = Canvas.GetTop(this);
            if(this.Width + e.Delta.Translation.X > 50)
                this.Width += e.Delta.Translation.X;
            if (this.Height + e.Delta.Translation.Y > 50)
                this.Height += e.Delta.Translation.Y;

            ((CompositeTransform)this.RenderTransform).TranslateX += e.Delta.Translation.X /2;
            ((CompositeTransform)this.RenderTransform).TranslateY += e.Delta.Translation.Y / 2;

            //Canvas.SetLeft(this, originalX + e.Delta.Translation.X);
            //Canvas.SetTop(this, originalY + e.Delta.Translation.Y);


            /*
                        this.Width += e.Delta.Translation.X;
            this.Height += e.Delta.Translation.Y;


                var leftRatio = Canvas.GetLeft(TempRegion)/width;
                var topRatio = Canvas.GetTop(TempRegion)/height;
                var widthRatio = TempRegion.Width/width;
                var heightRatio = TempRegion.Height/height;


            _vm.NodeHeight = nodeHeight;
            _vm.NodeWidth = nodeWidth;

            _vm.Left = _vm.NodeWidth*_vm.LeftRatio;
            _vm.Top = _vm.NodeHeight*_vm.TopRatio;
            _vm.RectWidth = _vm.RectWidthRatio*_vm.NodeWidth;
            _vm.RectHeight = _vm.RectHeightRatio*_vm.NodeHeight;

            Canvas.SetLeft(this, _vm.Left);
            Canvas.SetTop(this, _vm.Top);
            rectangle.Width = _vm.RectWidth;
            rectangle.Height = _vm.RectHeight;
            */
        }

        private void XResizingRectangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            OnSelected?.Invoke(this, true);

            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ((CompositeTransform) this.RenderTransform).TranslateX += e.Delta.Translation.X;
            ((CompositeTransform)this.RenderTransform).TranslateY += e.Delta.Translation.Y;
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);

            e.Handled = true;

        }

        public void Deselected()
        {
            xMainRectangle.StrokeThickness = 3;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            xResizingRectangle.Visibility = Visibility.Collapsed;

        }

        public void Selected()
        {
            xMainRectangle.StrokeThickness = 6;
            xMainRectangle.Stroke = new SolidColorBrush(Windows.UI.Colors.DarkBlue);
            xResizingRectangle.Visibility = Visibility.Visible;

        }
        private void xMainRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
           OnSelected?.Invoke(this, true);

        }
    }
}
