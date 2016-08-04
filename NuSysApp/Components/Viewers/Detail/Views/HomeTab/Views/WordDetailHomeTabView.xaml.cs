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
            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            //vm.CreateRegionViews();
            DataContext = vm;

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {

            };

            //vm.MakeTagList();

            vm.LibraryElementController.Disposed += ControllerOnDisposed;
        }


        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (WordDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.Disposed += ControllerOnDisposed;
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
        /// when the word capture button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCapture(object sender, RoutedEventArgs e)
        {
            var vm = (WordDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
            {
                return;
            }

            Task.Run(async delegate
            {
                var m = new Message();

                // Get text from the pdf
                var myDoc = await MediaUtil.DataToPDF(vm.LibraryElementController.LibraryElementModel.Data);
                string pdf_text = "";
                int numPages = myDoc.PageCount;
                int currPage = 0;
                while (currPage < numPages)
                {
                    pdf_text = pdf_text + myDoc.GetAllTexts(currPage);
                    currPage++;
                }

                m["id"] = SessionController.Instance.GenerateId();
                m["data"] = vm.LibraryElementController.LibraryElementModel.Data;
                if (!string.IsNullOrEmpty(pdf_text))
                {
                    m["pdf_text"] = pdf_text;
                }
                m["type"] = ElementType.PDF.ToString();
                m["title"] = vm.LibraryElementController.LibraryElementModel.Title + " CAPTURED "+DateTime.Now.ToString();
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(m));
            });
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
                Debug.Assert(vm?.LibraryElementController?.LibraryElementModel?.LibraryElementId != null);
                string path = null;
                await System.Threading.Tasks.Task.Run( async delegate{
                        path = await SessionController.Instance.NuSysNetworkSession.DownloadDocx( vm.LibraryElementController.LibraryElementModel.LibraryElementId);
                        
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
    }
}
