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
    public sealed partial class ImageFullScreenView : UserControl
    {
        private InqCanvasView _inqCanvasView;
        private ImageElementViewModel _viewMod;

        public ImageFullScreenView(ImageElementViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _viewMod = vm;

            var model = (ImageElementModel)vm.Model;
            var token = model.GetMetaData("Token");
            if (token == null || String.IsNullOrEmpty(token?.ToString()))
            {
                SourceBttn.Visibility = Visibility.Collapsed;
            }
            else if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                SourceBttn.Visibility = Visibility.Collapsed;
            }

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                SetDimension(SessionController.Instance.SessionView.ActualWidth / 2 - 30, SessionController.Instance.SessionView.ActualHeight);
            };
            SetDimension(SessionController.Instance.SessionView.ActualWidth / 2 - 30, SessionController.Instance.SessionView.ActualHeight);
        }

        public void SetDimension(double parentWidth, double parentHeight)
        {
            var ratio = _viewMod.Width > _viewMod.Height ? _viewMod.Width / parentWidth : _viewMod.Height / parentHeight;
            xImg.Width = _viewMod.Width / ratio;
            xImg.MaxWidth = parentWidth*0.9;
            xImg.Height = _viewMod.Height / ratio;
            xImg.MaxHeight = SessionController.Instance.SessionView.ActualHeight - 370;
            buttons.Width = xImg.ActualWidth;
        } 

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (ImageElementModel)((ImageElementViewModel)DataContext).Model;

            string token = model.GetMetaData("Token")?.ToString();

            if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                return;
            }

            string ext = Path.GetExtension(model.GetMetaData("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                string bookmarkId = model.GetMetaData("BookmarkId").ToString();
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
    }
}
