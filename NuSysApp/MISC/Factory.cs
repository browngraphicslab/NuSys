using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace NuSysApp
{
    public class Factory
    {
        public static RichTextNodeViewModel CreateNewRichText(WorkspaceViewModel vm, string id, string html)
        {
            return new RichTextNodeViewModel(vm, id) { Data = html };
        }

        public async static Task<ImageNodeViewModel> CreateNewImage(WorkspaceViewModel vm, string id, StorageFile storageFile)
        {
            var invm = new ImageNodeViewModel(vm, id);
            await invm.InitializeImageNodeAsync(storageFile);
            return invm;
        }

        public async static Task<PdfNodeViewModel> CreateNewPdfNodeViewModel(WorkspaceViewModel vm, string id, StorageFile storageFile)
        {
            var pnvm = new PdfNodeViewModel(vm, id);
            await pnvm.InitializePdfNodeAsync(storageFile);
            return pnvm;
        }

        public static InkNodeViewModel CreateNewInk(WorkspaceViewModel vm, string id)
        {
            return new InkNodeViewModel(vm, id);
        }
        public static InkNodeViewModel CreateNewPromotedInk(WorkspaceViewModel vm, string id)
        {
            var inkNode = new InkNodeViewModel(vm, id);
            ((InkNodeView2)inkNode.View).UpdateInk();
            return inkNode;
        }
    }

    
}
 