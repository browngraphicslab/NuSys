﻿ using NuSysApp.Util;
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
    public sealed partial class ImageDetailHomeTabView : UserControl
        {
            public ImageRegionView SelectedRegion { set; get; }
            public Grid ImageGrid { get { return xImageGrid; } }
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



            vm.LibraryElementController.Disposed += ControllerOnDisposed;
                vm.PropertyChanged += PropertyChanged;
                vm.View = this;

 

            }

        public void RefreshRegions()
        {
            var vm = DataContext as ImageDetailHomeTabViewModel;
            vm.SetExistingRegions();
        }
        public double GetImgHeight()
        {
            //return ActualHeight;
            return xImg.ActualHeight;
        }

        private double _nonZeroPrevActualWidth = 0;

        // TODO: Very hacky, change later so that the width binds instead of xaml stretching
        public double GetImgWidth()
        {
            return xImg.ActualWidth;
            //return actualWidth;
            //if (actualWidth.Equals(0))
            //{
            //    return _nonZeroPrevActualWidth;
            //}
            //else
            //{
            //    _nonZeroPrevActualWidth = actualWidth;
            //    return actualWidth;
            //}
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RegionViews":
                    break;
            }
        }



        private void SelectRegion(ImageRegionView region)
        {
            SelectedRegion?.Deselect();
            SelectedRegion = region;
            SelectedRegion.Select();
        }


        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (ImageDetailHomeTabViewModel) DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (ImageElementModel)((ImageElementViewModel)DataContext).Model;

            var libraryElementController = (DataContext as ImageDetailHomeTabViewModel)?.LibraryElementController;
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
         /*   var vm = DataContext as ImageDetailHomeTabViewModel;
            foreach (var regionView in vm.RegionViews)
            {
                regionView.Deselect();
            }*/
        }

        private void BitmapImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var vm = (ImageDetailHomeTabViewModel)DataContext;
            vm.SetExistingRegions();

        }
    }
}
