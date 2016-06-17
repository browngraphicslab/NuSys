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
using System.ComponentModel;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{  
    public sealed partial class ImageDetailHomeTabView : UserControl, Regionable<ImageRegionView>
        {
            public ImageRegionView SelectedRegion { set; get; }
            public ImageDetailHomeTabView(ImageDetailHomeTabViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();

            //var token = model.GetMetaData("Token");
            //if (token == null || String.IsNullOrEmpty(token?.ToString()))
            //{
            //    SourceBttn.Visibility = Visibility.Collapsed;
            //}
            //else if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            //{
            //    SourceBttn.Visibility = Visibility.Collapsed;
            //}
                foreach (var v in vm.Controller.LibraryElementModel.Regions)
                {
                    vm.RegionAdded(v,this);
                }

            vm.Controller.Disposed += ControllerOnDisposed;
                vm.PropertyChanged += PropertyChanged;
            
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RegionViews":
                    break;
            }
        }

        public void AddRegion()
        {
        }

        
        public void RemoveRegion(ImageRegionView region)
        {
        }

        public void DisplayRegion(Region region)
        {
            var rectangleRegion = (RectangleRegion)region;

            var displayedRegion = new ImageRegionView(rectangleRegion, this);

            (this.DataContext as ImageDetailHomeTabViewModel).RegionAdded(rectangleRegion,this);
            (this.DataContext as ImageDetailHomeTabViewModel).Controller.AddRegion(rectangleRegion);

            displayedRegion.OnSelected += DisplayedRegion_OnSelected;
            //displayedRegion.Select();
            //this.SelectRegion(displayedRegion);

        }


        private void SelectRegion(ImageRegionView region)
        {
            SelectedRegion?.Deselect();
            SelectedRegion = region;
            SelectedRegion.Select();
        }

        private void DisplayedRegion_OnSelected(ImageRegionView sender, bool selected)
        {
            sender.Select();
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
            //SelectedRegion?.Deselected();
            //SelectedRegion = null;
        }

        private void XImg_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var vm = this.DataContext as ImageDetailHomeTabViewModel;
            foreach (ImageRegionView irv in vm.RegionViews)
            {
               irv.ApplyNewSize(e.NewSize); 
            }
        }
        }
}
