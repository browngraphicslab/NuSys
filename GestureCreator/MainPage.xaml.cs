using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GestureCreator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Canvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                                                   Windows.UI.Core.CoreInputDeviceTypes.Touch |
                                                   Windows.UI.Core.CoreInputDeviceTypes.Pen;
        }

        public async void WriteData(string data)
        {
            var filename = fileInput.Text + ".stroke";

            Windows.Storage.StorageFolder storageFolder =
                  Windows.Storage.ApplicationData.Current.LocalFolder;

            if (await storageFolder.TryGetItemAsync(filename) == null)
            {
                await storageFolder.CreateFileAsync(filename);
            }

            Windows.Storage.StorageFile sampleF =
                await storageFolder.GetFileAsync(filename);
            await Windows.Storage.FileIO.AppendTextAsync(sampleF, data + "\n");
        }


        private async void Save_OnTapped(object sender, TappedRoutedEventArgs e) {
            var points = Canvas.InkPresenter.StrokeContainer.GetStrokes()[0].GetInkPoints();
            var data = "";
            foreach (var point in points)
            {
                data += point.Position.X + "," + point.Position.Y + "\n";
            }
            data = data.Substring(0, data.Length - 1);
             WriteData(data);
            Canvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
        }

        private void Delete_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Canvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
        }
    }
}
