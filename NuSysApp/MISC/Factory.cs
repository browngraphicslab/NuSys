using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace NuSysApp
{
    public class Factory
    {
        public static RichTextNodeViewModel CreateNewRichText(WorkspaceViewModel vm, string html, int id)
        {
            return new RichTextNodeViewModel(vm, id) { Data = html };
        }

        public async static Task<ImageNodeViewModel> CreateNewImage(WorkspaceViewModel vm, StorageFile storageFile, int id)
        {
            var invm = new ImageNodeViewModel(vm, id);
            await invm.InitializeImageNodeAsync(storageFile);
            return invm;
        }

        public async static Task<PdfNodeViewModel> CreateNewPdfNodeViewModel(WorkspaceViewModel vm, StorageFile storageFile, int id)
        {
            var pnvm = new PdfNodeViewModel(vm, id);
            await pnvm.InitializePdfNodeAsync(storageFile);
            return pnvm;
        }

        public static InkNodeViewModel CreateNewInk(WorkspaceViewModel vm, int id)
        {
            return new InkNodeViewModel(vm, id);
        }
        public static InkNodeViewModel CreateNewPromotedInk(WorkspaceViewModel vm, int id)
        {
            var inkNode = new InkNodeViewModel(vm, id);
            ((InkNodeView2)inkNode.View).UpdateInk();
            return inkNode;
        }
    }

    
}
 