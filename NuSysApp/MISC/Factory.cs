using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public class Factory
    {
        public static TextNodeViewModel CreateNewText(WorkspaceViewModel vm,  string data)
        {
            return new TextNodeViewModel(vm) { Data = data };
        }

        public static RichTextNodeViewModel CreateNewRichText(WorkspaceViewModel vm, string html)
        {
            return new RichTextNodeViewModel(vm) { Data = html };
        }

        public async static Task<ImageNodeViewModel> CreateNewImage(WorkspaceViewModel vm, StorageFile storageFile)
        {
            var invm = new ImageNodeViewModel(vm);
            await invm.InitializeImageNodeViewModel(storageFile);
            return invm;
        }

        public async static Task<PdfNodeViewModel> CreateNewPdfNodeViewModel(WorkspaceViewModel vm, StorageFile storageFile)
        {
            var pnvm = new PdfNodeViewModel(vm);
            await pnvm.InitializePdfNodeAsync(storageFile);
            return pnvm;
        }

        public static InkNodeViewModel CreateNewInk(WorkspaceViewModel vm)
        {
            return new InkNodeViewModel(vm);
        }
    }

    
}
 