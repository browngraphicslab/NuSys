using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;


namespace RuntimeComponent1
{

    sealed class Blah : XamlRenderingBackgroundTask
    {
        public static Image GetBlah()
        {
            var a = new Image();
            string url = "ms-appx:///Assets/powerpointIcon.png";
            a.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
            return a;
        }
    }


    public sealed class aaa : IBackgroundTask
    {


        // BackgroundTaskDeferral _deferral = 
        IBackgroundTaskInstance _taskInstance = null;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _taskInstance = taskInstance;
            //_taskInstance.
            Debug.WriteLine("Hello!");
            Debug.WriteLine("This is the last line in the test");
            var a = Blah.GetBlah();
            taskInstance.GetDeferral().Complete();

        }
    }
}
