using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using NAudio;
using NAudio.Wave;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using System.Diagnostics;
using NetTopologySuite.Utilities;
using NusysIntermediate;
using BitConverter = System.BitConverter;
using Line = Windows.UI.Xaml.Shapes.Line;

namespace NuSysApp
{
    public class AudioNodeViewModel : ElementViewModel
    {
        private Grid _visualGrid;
        public delegate void VisualizationLoadedEventHandler();

        public event VisualizationLoadedEventHandler OnVisualizationLoaded;

        public AudioNodeViewModel(ElementController controller) : base(controller)
        {
            Width = controller.Model.Width;
            Height = controller.Model.Height;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

   

        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            //SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.SetAudioSize(width, height);
            base.OnSizeChanged(source, width, height);
        }
    }
}
