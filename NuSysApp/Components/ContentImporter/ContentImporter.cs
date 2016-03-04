using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace NuSysApp
{
    public class SelectionItem
    {
        public String BookmarkId;
        public Boolean IsExported;
        public String RtfContent;
        public String FilePath;
        public List<String> ImageNames;
        public String DateTimeExported;
        public String Token;
    }

    public class ContentImporter
    {
        public event ContentImportedHandler ContentImported;
        public delegate void ContentImportedHandler(List<string> markdown);

        public ContentImporter()
        {
            SetupChromeIntermediate();
            SetupWordTransfer();
            SetupPowerpointTransfer();
        }

        private void SetupChromeIntermediate()
        {
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.ChromeTransferFolder.GetFilesAsync();
                if (transferFiles.Count == 0)
                    return;
                var file = transferFiles[0];
                var count = 0;

                var text = await FileIO.ReadTextAsync(file);
                
                await UITask.Run(async () =>
                {
                   // text = await ContentConverter.HtmlToRtf(text);
                    var m = new Message();
                    var width = SessionController.Instance.SessionView.ActualWidth;
                    var height = SessionController.Instance.SessionView.ActualHeight;
                    var centerpoint =
                        SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                            new Point(width / 2, height / 2));

                    var contentId = SessionController.Instance.GenerateId();

                    m["contentId"] = contentId;
                    m["x"] = centerpoint.X - 200;
                    m["y"] = centerpoint.Y - 200;
                    m["width"] = 400;
                    m["height"] = 400;
                    m["nodeType"] = ElementType.Text.ToString();
                    m["autoCreate"] = true;
                    m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;


                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));

                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                            new CreateNewLibraryElementRequest(contentId, text, ElementType.Text.ToString()));
                    //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, text), NetworkClient.PacketType.TCP, null, true);



                });

                await file.DeleteAsync();
            };
        }

        private Message CreateMessage(SelectionItem selectionItem, String contentId, Point centerpoint)
        {
            Message m = new Message();
            m["contentId"] = contentId;
            m["x"] = centerpoint.X - 200;
            m["y"] = centerpoint.Y - 200;
            m["width"] = 400;
            m["height"] = 400;
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;

            var metadata = new Dictionary<string, object>();
            metadata["BookmarkId"] = selectionItem.BookmarkId;
            metadata["IsExported"] = selectionItem.IsExported;
            metadata["FilePath"] = selectionItem.FilePath;
            metadata["DateTimeExported"] = selectionItem.DateTimeExported;
            metadata["Token"] = selectionItem.Token?.Trim();

            m["metadata"] = metadata;

            return m;
        }

        private async Task AddinTransfer(List<SelectionItem> selectionItems)
        {
            foreach (SelectionItem selectionItem in selectionItems)
            {
                double width, height;
                Point centerpoint;
                await UITask.Run(async () =>
                {
                    width = SessionController.Instance.SessionView.ActualWidth;
                    height = SessionController.Instance.SessionView.ActualHeight;
                    centerpoint =
                        SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                            new Point(width / 2, height / 2));
                });
                    var hasRtf = !String.IsNullOrEmpty(selectionItem.RtfContent);

                    if (hasRtf)
                    {
                        var rtfContent = selectionItem.RtfContent.Replace("\\\\", "\\");
                        var contentId = SessionController.Instance.GenerateId();

                        Message m = CreateMessage(selectionItem, contentId, centerpoint);
                        m["nodeType"] = ElementType.Text.ToString();

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest( new CreateNewLibraryElementRequest(contentId, rtfContent, ElementType.Text.ToString()));
                    /*
                    await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(
                                new NewContentSystemRequest(contentId,
                                    rtfContent),NetworkClient.PacketType.TCP,null,true);*/
                    }

                    var hasImage = selectionItem.ImageNames.Count > 0;
                    if (hasImage)
                    {
                        foreach (String imageName in selectionItem.ImageNames)
                        {
                            var contentId = SessionController.Instance.GenerateId();

                            Message m = CreateMessage(selectionItem, contentId, centerpoint);
                            m["nodeType"] = ElementType.Image.ToString();

                            StorageFile imgFile;
                            try {
                                imgFile = await NuSysStorages.Media.GetFileAsync(imageName);
                                var ba = await MediaUtil.StorageFileToByteArray(imgFile);
                                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
                                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, Convert.ToBase64String(ba), ElementType.Image.ToString()));
                            /*
                            await
                                    SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(
                                        new NewContentSystemRequest(contentId,
                                            Convert.ToBase64String(ba)), NetworkClient.PacketType.TCP, null, true);*/

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                
            }
        }

        private async void SetupWordTransfer()
        {
            Task.Run(async () =>
            {
                while (true) {
                    await WordTransfer();
                    await Task.Delay(1000);
                }
            });
        }

        private async void SetupPowerpointTransfer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await PowerpointTransfer();
                    await Task.Delay(1000);
                }
            });
        }

        private async Task WordTransfer()
        {
            var fileList = await NuSysStorages.WordTransferFolder.GetFilesAsync();

            foreach (var file in fileList)
            {
                var text = await FileIO.ReadTextAsync(file);
                var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                List<SelectionItem> selectionItems = JsonConvert.DeserializeObject<List<SelectionItem>>(text, settings);

                await AddinTransfer(selectionItems);
                await file.DeleteAsync();
            }
        }

        private async Task PowerpointTransfer()
        {
            var fileList = await NuSysStorages.PowerPointTransferFolder.GetFilesAsync();

            foreach (var file in fileList)
            {
                var text = await FileIO.ReadTextAsync(file);
                var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                List<SelectionItem> selectionItems = JsonConvert.DeserializeObject<List<SelectionItem>>(text, settings);

                await AddinTransfer(selectionItems);

                await file.DeleteAsync();
            }
        }

    }
}
