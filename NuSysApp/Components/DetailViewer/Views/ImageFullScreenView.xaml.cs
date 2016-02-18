using NuSysApp.Util;
using System;
using System.Collections.Generic;
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

        public ImageFullScreenView(ImageNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            var model = (ImageNodeModel)vm.Model;
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
                var sw = SessionController.Instance.SessionView.ActualWidth /2;
                var sh = SessionController.Instance.SessionView.ActualHeight /2;

                var ratio = vm.Width > vm.Height ? vm.Width/sw : vm.Height/sh;
                xImg.Width = vm.Width/ratio;
                xImg.Height = vm.Height/ratio;
                //  xBorder.Width = xImg.Width + 5;
                // xBorder.Height = xImg.Height +5;


                //_inqCanvasView = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(xImg.Width, xImg.Height)));
                //xWrapper.Children.Add(_inqCanvasView);
                //_inqCanvasView.IsEnabled = true;
                //_inqCanvasView.Background = new SolidColorBrush(Colors.Aqua);
                //_inqCanvasView.Width = xImg.Width;
                //_inqCanvasView.Height = xImg.Height;   
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (ImageNodeModel)((ImageNodeViewModel)DataContext).Model;

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
