using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class RenderTask : XamlRenderingBackgroundTask
    {
        private UIElement _elem;


        public RenderTask(UIElement elem)
        {
            _elem = elem;
        }

        protected async override void OnRun(IBackgroundTaskInstance taskInstance)
        {
            base.OnRun(taskInstance);
            var img = new RenderTargetBitmap();
            await img.RenderAsync(_elem);
            taskInstance.GetDeferral().Complete();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            System.Diagnostics.Debug.WriteLine("Run called!");
            OnRun(taskInstance);
        }
    }
}
