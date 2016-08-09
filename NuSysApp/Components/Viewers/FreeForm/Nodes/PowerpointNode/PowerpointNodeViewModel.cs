using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public class PowerpointNodeViewModel : ElementViewModel
    {
        
        public PowerpointNodeViewModel(ElementController controller) : base(controller)
        {
            String path = controller.LibraryElementController.GetMetadata("FilePath").First();

            if (!String.IsNullOrEmpty(path))
            {
                Title = Path.GetFileName(path);
            }

            WatchForPdf();
        }

        private async void WatchForPdf()
        {
            string token = Controller.LibraryElementController.GetMetadata("Token").First();

            if (!String.IsNullOrEmpty(token) && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        var fileList = await NuSysStorages.Media.GetFilesAsync();
                        bool foundPdf = false;

                        foreach (StorageFile file in fileList)
                        {
                            string ext = Path.GetExtension(file.Path);
                            string name = Path.GetFileNameWithoutExtension(file.Path);

                            if (Constants.PdfFileTypes.Contains(ext) && token == name)
                            {
                                foundPdf = true;
                                try
                                {
                                    await CreatePdfNode(file);
                                }
                                catch (Exception ex)
                                {
                                //TODO error handling
                            }
                            }
                        }

                        if (!foundPdf)
                        {
                            await Task.Delay(1000 * 5);
                        }
                        else
                        {
                            return;
                        }
                    }
                });
            }
        }

        private async Task CreatePdfNode(StorageFile pdfFile)
        {
            var wordModel = ((PowerpointNodeModel)this.Model);
            var wordId = wordModel.Id;

            var contentId = SessionController.Instance.GenerateId();
            Message m = new Message();
            m["contentId"] = contentId;
            m["x"] = wordModel.X;
            m["y"] = wordModel.Y;
            m["width"] = 400;
            m["height"] = 400;
            m["autoCreate"] = true;
            m["creator"] =  SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
            m["type"] = NusysConstants.ElementType.PDF.ToString();

            var metadata = new Dictionary<string, object>();
            metadata["BookmarkId"] = Controller.LibraryElementController.GetMetadata("BookmarkId");
            metadata["IsExported"] = Controller.LibraryElementController.GetMetadata("IsExported");
            metadata["FilePath"] = Controller.LibraryElementController.GetMetadata("FilePath");
            metadata["DateTimeExported"] = Controller.LibraryElementController.GetMetadata("DateTimeExported");
            metadata["Token"] = Controller.LibraryElementController.GetMetadata("Token");
            m["metadata"] = metadata;

            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await pdfFile.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            var pdfContent = Convert.ToBase64String(fileBytes);


            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new NewElementRequest(m));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, pdfContent, NusysConstants.ElementType.PDF));
            /*
            await
                SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(
                    new NewContentSystemRequest(contentId,
                        pdfContent), NetworkClient.PacketType.TCP, null, true);*/

            Request deleteRequest = new DeleteElementRequest(wordId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(deleteRequest);
        }

        public override async Task Init()
        {
            
        }

    }
}