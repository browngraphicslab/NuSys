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
    public sealed partial class SessionViewPopupView : UserControl
    {
        private SessionViewPopupViewModel _vm;

        public SessionViewPopupView()
        {
            InitializeComponent();

            // the sessionController.Instance.sessionview.maincanvas onloaded event sets the data context
            // otherwise the main canvas may not be instantiated and could lead to null errors
            DataContextChanged += delegate (FrameworkElement sender, DataContextChangedEventArgs args)
            {
                _vm = DataContext as SessionViewPopupViewModel;
                if (_vm == null)
                {
                    return;
                }

                // initialize the ViewModel
                _vm.Init();

                // place the box in the center of the screen
                Canvas.SetTop(this, (SessionController.Instance.SessionView.MainCanvas.ActualHeight / 2.0) - _vm.Height / 2.0);
                Canvas.SetLeft(this, (SessionController.Instance.SessionView.MainCanvas.ActualWidth / 2.0) - _vm.Width / 2.0);


                // for recentering when the canvas changes
                SessionController.Instance.SessionView.MainCanvas.SizeChanged += _vm.MainCanvas_SizeChanged;
                // for closing the speech to text box
                //SessionController.Instance.SessionView.MainCanvas.Tapped += MainCanvas_Tapped;

            };
        }

        public void Open()
        {
            _vm.IsOpen = Visibility.Visible;
        }

        // when the rootgrid is manipulated, move the entired box
        private void RootGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            xMatrixTransform.Matrix = xTransformGroup.Value;
            xCompositeTransform.TranslateX = e.Delta.Translation.X;
            xCompositeTransform.TranslateY = e.Delta.Translation.Y;

            var transform = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(RootGrid);
            var point = transform.TransformPoint(new Point(0, 0));
            if (point.X > 0)
            {
                // arrived at the left side of the canvas
                xCompositeTransform.TranslateX += point.X;//-e.Delta.Translation.X;
                e.Complete();
            }
            if (point.Y > 0)
            {
                // arrived at the top of the canvas
                xCompositeTransform.TranslateY += point.Y;//-e.Delta.Translation.Y;
                e.Complete();
            }
            if (point.X - _vm.Width < -SessionController.Instance.SessionView.MainCanvas.ActualWidth)
            {
                // arrived at the right of the canvas
                xCompositeTransform.TranslateX += SessionController.Instance.SessionView.MainCanvas.ActualWidth + point.X - _vm.Width;
                e.Complete();
            }
            if (point.Y - _vm.Height < -SessionController.Instance.SessionView.MainCanvas.ActualHeight)
            {
                // arrived at the bottom of the canvas
                xCompositeTransform.TranslateY += SessionController.Instance.SessionView.MainCanvas.ActualHeight + point.Y - _vm.Height;

                e.Complete();
            }

            e.Handled = true;
        }

        private void Close_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _vm.IsOpen = Visibility.Collapsed;
            e.Handled = true;
        }
    }
}
