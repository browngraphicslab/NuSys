using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class WordDetailHomeTabView : UserControl
    {
        //private InqCanvasView _inqCanvasView;

        public WordDetailHomeTabView(WordDetailHomeTabViewModel vm)
        {
            InitializeComponent();
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.PropertyChanged += PropertyChanged;
            vm.View = this;

            //vm.CreateRegionViews();
            DataContext = vm;

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {

            };

            //vm.MakeTagList();

            vm.Controller.Disposed += ControllerOnDisposed;
        }
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RegionViews":
                    break;
            }
        }

        private void XBorderOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //xBorder.Clip = new RectangleGeometry {Rect= new Rect(0,0,e.NewSize.Width, e.NewSize.Height)};
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (WordDetailHomeTabViewModel)DataContext;
            vm.Controller.Disposed += ControllerOnDisposed;
            DataContext = null;
        }

        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (WordDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipLeft();
        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (WordDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipRight();
        }
        /// <summary>
        /// When the source button is clicked, open the word plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            UITask.Run(async delegate
            {
                Debug.Assert(DataContext is WordDetailHomeTabViewModel);
                var vm = DataContext as WordDetailHomeTabViewModel;
                Debug.Assert(vm?.Controller?.LibraryElementModel?.LibraryElementId != null);
                string path = null;
                await System.Threading.Tasks.Task.Run( async delegate{
                        path = await SessionController.Instance.NuSysNetworkSession.DownloadDocx( vm.Controller.LibraryElementModel.LibraryElementId);
                        
                });
                if (path == null)
                {
                    //probably didn't have a docx file on the server for the given id
                }
                StorageFile storageFile = null;
                var launcherOptions = new LauncherOptions() { UI = { PreferredPlacement = Placement.Right,InvocationPoint = new Point(SessionController.Instance.SessionView.ActualWidth/2,0.0)} };
                launcherOptions.TreatAsUntrusted = false;
                launcherOptions.PreferredApplicationDisplayName = "NUSYS";
                launcherOptions.PreferredApplicationPackageFamilyName = "NuSys";
                launcherOptions.DesiredRemainingView = ViewSizePreference.UseHalf;
                await Task.Run(async delegate
                {
                    storageFile = await StorageFile.GetFileFromPathAsync(path);
                    File.SetAttributes(path, System.IO.FileAttributes.Normal);
                });
                await Launcher.LaunchFileAsync(storageFile, launcherOptions);
                

                //doc.
                //doc.LoadFromFile(path);
                //open the path
            });
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            
        }

        public double GetPdfHeight()
        {
            return xImg.ActualHeight;
        }

        public double GetPdfWidth()
        {
            return xImg.ActualWidth;
        }

        private void xImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as WordDetailHomeTabViewModel;
        }

    }
}