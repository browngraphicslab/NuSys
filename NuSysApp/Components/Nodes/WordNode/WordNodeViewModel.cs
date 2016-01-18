using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class WordNodeViewModel : NodeViewModel
    {
        
        public WordNodeViewModel(WordNodeModel model) : base(model)
        {
            var title = Path.GetFileName(model.GetMetaData("FilePath").ToString());
            Title = title;
        }

        public override async Task Init()
        {

        }
    }
}