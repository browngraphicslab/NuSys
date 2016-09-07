using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class VideoPlayer : BaseMediaPlayer
    {
        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height);
            var progressTransform = ProgressBar.RenderTransform as TranslateTransform;
            var buttonTranform = PlayPauseButton.RenderTransform as TranslateTransform;
            Debug.Assert(progressTransform != null && buttonTranform != null);
            progressTransform.Y = MediaElement.Height;
            buttonTranform.Y = MediaElement.Height + ProgressBar.Height;
        }
    }
}
