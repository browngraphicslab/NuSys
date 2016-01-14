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

namespace NuSysApp
{
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

                   
                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                            new NewContentSystemRequest(contentId,
                                text));

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));


                });

                await file.DeleteAsync();
            };
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
                    var jsonArray = (JArray)JsonConvert.DeserializeObject(text, settings);

                    foreach (var entry in jsonArray)
                    {
                        await UITask.Run(async () =>
                        {
                            var jsonObj = (JObject) entry;
                            var rtfContent = jsonObj["RtfContent"].ToString();
                            rtfContent = rtfContent.Replace("\\\\", "\\");
                            var imageName = jsonObj["ImageName"].ToString();
                            var isImage = imageName != "";

                            var m = new Message();
                            var width = SessionController.Instance.SessionView.ActualWidth;
                            var height = SessionController.Instance.SessionView.ActualHeight;
                            var centerpoint =
                                SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
                                    new Point(width/2, height/2));

                            var contentId = SessionController.Instance.GenerateId();

                            m["contentId"] = contentId;
                            m["x"] = centerpoint.X - 200;
                            m["y"] = centerpoint.Y - 200;
                            m["width"] = 400;
                            m["height"] = 400;
                            m["nodeType"] = isImage ? NodeType.Image.ToString() : NodeType.Text.ToString();
                            m["autoCreate"] = true;
                            m["creators"] = new List<string>() {SessionController.Instance.ActiveWorkspace.Id};
                            m["metadata"] = new Dictionary<string,object>();

                            var content = string.Empty;
                            if (isImage)
                            {
                                var imgFile = await NuSysStorages.Media.GetFileAsync(imageName);
                                var ba = await MediaUtil.StorageFileToByteArray(imgFile);
                                content = Convert.ToBase64String(ba);
                            }
                            else
                            {
                                content = rtfContent;
                            }
                        
                            await
                                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                                    new NewContentSystemRequest(contentId,
                                        content));

                            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
                        });
                    }
                }

                foreach (var file in transferFiles)
                {
                    await file.DeleteAsync();
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
                    var jsonArray = (JArray)JsonConvert.DeserializeObject(text, settings);

                    foreach (var entry in jsonArray)
                    {
                        await UITask.Run(async () =>
                        {
                            var jsonObj = (JObject)entry;
                            var rtfContent = jsonObj["RtfContent"].ToString();
                            var imageName = jsonObj["ImageName"].ToString();
                            var isImage = imageName != "";

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
                            m["nodeType"] = isImage ? NodeType.Image.ToString() : NodeType.Text.ToString();
                            m["autoCreate"] = true;
                            m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.Id };

                            var content = string.Empty;
                            if (isImage)
                            {
                                var imgFile = await NuSysStorages.Media.GetFileAsync(imageName);
                                var ba = await MediaUtil.StorageFileToByteArray(imgFile);
                                content = Convert.ToBase64String(ba);
                            }
                            else
                            {
                                content = rtfContent;
                            }

                            await
                                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                                    new NewContentSystemRequest(contentId,
                                        content));

                            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
                        });
                    }
                }

                foreach (var file in transferFiles)
                {
                    await file.DeleteAsync();
                }

                ContentImported?.Invoke(contents.ToList());
            };
        }

    }
}
