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
        public String DocPath;
        public String DocName;
        public List<String> ImageNames;
        public String DateTimeExported;
    }

    public class ContentImporter
    {
        public event ContentImportedHandler ContentImported;
        public delegate void ContentImportedHandler(List<string> markdown);

        public ContentImporter()
        {
            SetupChromeIntermediate();
            SetupWordTransfer();
            SetupPowerPointTransfer();
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
                        SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                            new Point(width / 2, height / 2));

                    var contentId = SessionController.Instance.GenerateId();

                    m["contentId"] = contentId;
                    m["x"] = centerpoint.X - 200;
                    m["y"] = centerpoint.Y - 200;
                    m["width"] = 400;
                    m["height"] = 400;
                    m["nodeType"] = NodeType.Text.ToString();
                    m["autoCreate"] = true;
                    m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.Id };


                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));

                    await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, text), NetworkClient.PacketType.TCP, null, true);



                });

                await file.DeleteAsync();
            };
        }

        private async Task AddinTransfer(List<SelectionItem> selectionItems)
        {
            foreach (SelectionItem selectionItem in selectionItems)
            {
                await UITask.Run(async () =>
                {
                    var width = SessionController.Instance.SessionView.ActualWidth;
                    var height = SessionController.Instance.SessionView.ActualHeight;
                    var centerpoint =
                        SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                            new Point(width / 2, height / 2));

                    var hasRtf = !String.IsNullOrEmpty(selectionItem.RtfContent);

                    if (hasRtf)
                    {
                        var rtfContent = selectionItem.RtfContent.Replace("\\\\", "\\");
                        var contentId = SessionController.Instance.GenerateId();
                        var m = new Message();

                        m["contentId"] = contentId;
                        m["x"] = centerpoint.X - 200;
                        m["y"] = centerpoint.Y - 200;
                        m["width"] = 400;
                        m["height"] = 400;
                        m["autoCreate"] = true;
                        m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.Id };
                        m["nodeType"] = NodeType.Text.ToString();

                        var metadata = new Dictionary<string, object>();
                        metadata["BookmarkId"] = selectionItem.BookmarkId;
                        metadata["IsExported"] = selectionItem.IsExported;
                        metadata["DocPath"] = selectionItem.DocPath;
                        metadata["DateTimeExported"] = selectionItem.DateTimeExported;
                        m["metadata"] = metadata;

                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));

                         await
                            SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(
                                new NewContentSystemRequest(contentId,
                                    rtfContent),NetworkClient.PacketType.TCP,null,true);
                    }

                    var hasImage = selectionItem.ImageNames.Count > 0;
                    if (hasImage)
                    {
                        foreach (String imageName in selectionItem.ImageNames)
                        {
                            var contentId = SessionController.Instance.GenerateId();

                            var m = new Message();
                            m["contentId"] = contentId;
                            m["x"] = centerpoint.X - 200;
                            m["y"] = centerpoint.Y - 200;
                            m["width"] = 400;
                            m["height"] = 400;
                            m["autoCreate"] = true;
                            m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.Id };
                            m["nodeType"] = NodeType.Image.ToString();

                            StorageFile imgFile;
                            try {
                                imgFile = await NuSysStorages.Media.GetFileAsync(imageName);
                                var ba = await MediaUtil.StorageFileToByteArray(imgFile);

                                await
                                    SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(
                                        new NewContentSystemRequest(contentId,
                                            Convert.ToBase64String(ba)));

                                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));

                            }
                            catch (Exception ex)
                            {

                            }


                        }
                    }
                });
            }
        }
        private async void SetupWordTransfer()
        {
            var fw = new FolderWatcher(NuSysStorages.WordTransferFolder);
            fw.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.WordTransferFolder.GetFilesAsync().AsTask();
                var contents = new List<string>();
                var count = 0;

                foreach (var file in transferFiles)
                {
                    var text = await FileIO.ReadTextAsync(file);
                    var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    List<SelectionItem> selectionItems = JsonConvert.DeserializeObject<List<SelectionItem>>(text, settings);

                    await AddinTransfer(selectionItems);
                }

                foreach (var file in transferFiles)
                {
                    try
                    {
                        await file.DeleteAsync();
                    }
                    catch (FileNotFoundException)
                    {
                        //TODO EXCEPTION HANDLING
                    }
                }

                ContentImported?.Invoke(contents.ToList());
            };
        }

        private async void SetupPowerPointTransfer()
        {
            var fw = new FolderWatcher(NuSysStorages.PowerPointTransferFolder);
            fw.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.PowerPointTransferFolder.GetFilesAsync().AsTask();
                var contents = new List<string>();
                var count = 0;

                foreach (var file in transferFiles)
                {
                    var text = await FileIO.ReadTextAsync(file);
                    var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    List<SelectionItem> selectionItems = JsonConvert.DeserializeObject<List<SelectionItem>>(text, settings);

                    await AddinTransfer(selectionItems);
                }

                foreach (var file in transferFiles)
                {
                    try { 
                        await file.DeleteAsync();
                    }
                    catch (FileNotFoundException)
                    {
                        //TODO EXCEPTION HANDLING
                    }
            }

                ContentImported?.Invoke(contents.ToList());
            };
        }

    }
}
