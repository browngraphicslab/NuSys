using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageElementIntanceController : ElementController
    {

        public ImageElementIntanceController(ElementModel model) : base(model)
        {

        }

        /// <summary>
        /// For proper image element resizing, first maintain ratio before calling SetSize on Controller
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="saveToServer"></param>
        public override void SetSize(double width, double height, bool saveToServer = true)
        {
            var ratio = Model.Width / Model.Height;
            base.SetSize(height * ratio, height, saveToServer); //preserve ratio
        }
        

    }
}