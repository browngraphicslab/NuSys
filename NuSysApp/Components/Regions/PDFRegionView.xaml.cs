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
using Windows.Foundation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PDFRegionView : UserControl
    { 
        public Point _topLeft;
        public Point _bottomRight;

        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;

        public PDFRegionView(PdfRegionViewModel regionVM)
        {
            this.DataContext = regionVM;
            this.InitializeComponent();

            this.Selected();
            this.RenderTransform = new CompositeTransform();
            OnSelected?.Invoke(this, true);

        }

        private void XResizingRectangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //TODO
        }

        private void XResizingRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            xMainRectangle.Width += e.Delta.Translation.X;
            xMainRectangle.Height += e.Delta.Translation.Y;
            RectangleTranform.CenterX += e.Delta.Translation.X;
            RectangleTranform.CenterY += e.Delta.Translation.Y;
            xGrid.Width += e.Delta.Translation.X;
            xGrid.Height += e.Delta.Translation.Y;
            GridTranform.CenterX += e.Delta.Translation.X;
            GridTranform.CenterY += e.Delta.Translation.Y;
            ResizerTransform.TranslateX += e.Delta.Translation.X;
            ResizerTransform.TranslateY += e.Delta.Translation.Y;
        }

        private void XResizingRectangle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            OnSelected?.Invoke(this, true);
            e.Handled = true;
        }

        private void RectangleRegionView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

           ((CompositeTransform)this.RenderTransform).TranslateX += e.Delta.Translation.X;
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
