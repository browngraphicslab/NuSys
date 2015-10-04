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
        public VideoNodeViewModel(VideoNodeModel model,WorkspaceViewModel vm) : base(model,vm)
        {
            this.View = new VideoNodeView(this);
            this.Transform = new MatrixTransform();
         //   this.Width = 400;
         //   this.Height = 150;
            this.IsSelected = false;
            this.IsEditing = false;
            this.IsEditingInk = false;
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            this.NodeType = NodeType.Video; 
        }

        public AudioCapture AudioRecorder { get; set; }
    }
}
