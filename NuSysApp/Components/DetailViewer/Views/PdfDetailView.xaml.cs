using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed partial class PdfDetailView : UserControl
    {
        public PdfDetailView(PdfNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {

                vm.PropertyChanged += delegate(object o, PropertyChangedEventArgs eventArgs)
                {
                    if (eventArgs.PropertyName != "ImageSource")
                        return;

                    var sw = SessionController.Instance.SessionView.ActualWidth / 2;
                    var sh = SessionController.Instance.SessionView.ActualHeight / 2;
                    var ratio = vm.Width > vm.Height ? vm.Width / sw : vm.Height/ sh;
                    xImg.Width = vm.Width / ratio;
                    xImg.Height = vm.Height / ratio;
                    xBorder.Width = xImg.Width + 5;
                    xBorder.Height = xImg.Height + 5;
                };

               

                await vm.InitPdfViewer();

                

    
            };
        }

        private void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipLeft();

          //  nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
          //  nodeTpl.inkCanvas.ReRenderLines();

        }

        private void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            vm.FlipRight();

         //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
         //   nodeTpl.inkCanvas.ReRenderLines();
        }
        
    }
}
