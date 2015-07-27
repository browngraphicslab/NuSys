using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class Factory
    {
        public static TextNodeViewModel CreateNewText(string data)
        {
            return new TextNodeViewModel(new WorkspaceViewModel()) { Data = data };
        }

        public static RichTextNodeViewModel CreateNewRichText(string html)
        {
            return new RichTextNodeViewModel(new WorkspaceViewModel()) { Data = html };
        }

        public static ImageNodeViewModel CreateNewImage(BitmapImage bmi)
        {
            return new ImageNodeViewModel(new WorkspaceViewModel(), bmi);
        }

        public async static Task<PdfNodeViewModel> CreateNewPdfNodeViewModel()
        {
            var pnvm = new PdfNodeViewModel(new WorkspaceViewModel());
            await pnvm.InitializePdfNodeAsync();
            return pnvm;
        }

        public static InkNodeViewModel CreateNewInk()
        {
            return new InkNodeViewModel(new WorkspaceViewModel());
        }
    }

    
}
 