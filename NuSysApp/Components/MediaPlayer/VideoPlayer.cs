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

        /// <summary>
        /// bottom padding for the progress bar and button
        /// </summary>
        private float _bottomPadding = 40;

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, height - 100);
            var progressTransform = ProgressBar.RenderTransform as TranslateTransform;
            var buttonTransform = PlayPauseButton.RenderTransform as TranslateTransform;
            Debug.Assert(progressTransform != null && buttonTransform != null);
            progressTransform.Y = MediaElement.Height;
            buttonTransform.Y = MediaElement.Height - ProgressBar.Height;
        }

        public override void SetLibraryElement(AudioLibraryElementController controller, bool autoStartWhenLoaded = true)
        {
            ProgressBar.SetBackgroundColor(MediaUtil.GetHashColorFromString(controller.AudioLibraryElementModel.LibraryElementId),100);
            base.SetLibraryElement(controller, autoStartWhenLoaded);
        }
    }
}
