using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class VideoNodeViewModel : NodeViewModel
    {
        public VideoNodeViewModel(VideoNodeModel model) : base(model)
        {
         //   this.Width = 400;
         //   this.Height = 150;

            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public AudioCapture AudioRecorder { get; set; }
    }
}
