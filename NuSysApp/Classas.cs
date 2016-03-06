using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public sealed class Classas : XamlRenderingBackgroundTask, IBackgroundTask
    {

        public Classas()
        {
            
        }

        protected override void OnRun(IBackgroundTaskInstance taskInstance)
        {
            base.OnRun(taskInstance);
        }


        public void Run(IBackgroundTaskInstance taskInstance)
        {
           
        }
    }
}
