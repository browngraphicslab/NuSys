using System.Threading.Tasks;
using Windows.Storage;

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

        public async static Task<ImageNodeViewModel> CreateNewImage(StorageFile storageFile)
        {
            var invm = new ImageNodeViewModel(new WorkspaceViewModel());
            await invm.InitializeImageNodeViewModel(storageFile);
            return invm;
        }

        public async static Task<PdfNodeViewModel> CreateNewPdfNodeViewModel(StorageFile storageFile)
        {
            var pnvm = new PdfNodeViewModel(new WorkspaceViewModel());
            await pnvm.InitializePdfNodeAsync(storageFile);
            return pnvm;
        }

        public static InkNodeViewModel CreateNewInk()
        {
            return new InkNodeViewModel(new WorkspaceViewModel());
        }
    }

    
}
 