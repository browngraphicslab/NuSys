using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
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
    public sealed partial class ImageDetailHomeTabView : UserControl, Regionable<RectangleRegionView>
        {
            public RectangleRegionView SelectedRegion { set; get; }
            public ImageDetailHomeTabView(ImageDetailHomeTabViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            
            //var token = model.GetMetaData("Token");
            //if (token == null || String.IsNullOrEmpty(token?.ToString()))
            //{
            //    SourceBttn.Visibility = Visibility.Collapsed;
            //}
            //else if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            //{
            //    SourceBttn.Visibility = Visibility.Collapsed;
            //}
            vm.Controller.Disposed += ControllerOnDisposed;
        }


        public void AddRegion()
        {

            var displayedRegion = new Windows.UI.Xaml.Shapes.Rectangle();
            Canvas.SetLeft(displayedRegion, 0);
            Canvas.SetTop(displayedRegion, 0);
            displayedRegion.Width = 100;
            displayedRegion.Height = 100;

            displayedRegion.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            displayedRegion.StrokeThickness = 3;
            displayedRegion.HorizontalAlignment = HorizontalAlignment.Stretch;
            displayedRegion.VerticalAlignment = VerticalAlignment.Stretch;

            totalStackPanel.Children.Add(displayedRegion);
        }

        
        public void RemoveRegion(RectangleRegionView region)
        {
            totalStackPanel.Children.Remove(region);
        }

        public void DisplayRegion(Region region)
        {
            var rectangleRegion = (RectangleRegion)region;

            //var displayedRegion = new Windows.UI.Xaml.Shapes.Rectangle();

            var displayedRegion = new RectangleRegionView(rectangleRegion);
            displayedRegion.OnSelected += DisplayedRegion_OnSelected;
            DisplayedRegion_OnSelected(displayedRegion, true);
            totalStackPanel.Children.Add(displayedRegion);

        }

        private void DisplayedRegion_OnSelected(object sender, bool selected)
        {
            SelectedRegion?.Deselected();
            SelectedRegion = (RectangleRegionView)sender;
            SelectedRegion.Selected();
           
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (ImageElementViewModel) DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (ImageElementModel)((ImageElementViewModel)DataContext).Model;

            var libraryElementController = (DataContext as ImageDetailHomeTabViewModel)?.Controller;
            string token = libraryElementController?.GetMetadata("Token")?.ToString();

            if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                return;
            }

            string ext = Path.GetExtension(libraryElementController.GetMetadata("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                string bookmarkId = libraryElementController.GetMetadata("BookmarkId").ToString();
                StorageFile writeBookmarkFile = await StorageUtil.CreateFileIfNotExists(NuSysStorages.OpenDocParamsFolder, token);

                using (StreamWriter writer = new StreamWriter(await writeBookmarkFile.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(bookmarkId);
                }

                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimeWord.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }
            else if (Constants.PowerpointFileTypes.Contains(ext))
            {
                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimePowerpoint.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }

            await AccessList.OpenFile(token);
        }

        private void xImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SelectedRegion?.Deselected();
        }
    }
}
