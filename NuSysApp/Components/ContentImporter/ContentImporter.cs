using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class ContentImporter
    {
        public event ContentImportedHandler ContentImported;
        public delegate void ContentImportedHandler(List<string> markdown);

        public ContentImporter()
        {
            SetupChromeIntermediate();
        }

        private void SetupChromeIntermediate()
        {
            var fw = new FolderWatcher(NuSysStorages.ChromeTransferFolder);
            fw.FilesChanged += async delegate
            {
                var transferFiles = await NuSysStorages.ChromeTransferFolder.GetFilesAsync().AsTask();
                var contents = new List<string>();
                var count = 0;
                
                foreach (var file in transferFiles)
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    await file.DeleteAsync();
                    DataReader reader = DataReader.FromBuffer(buffer);
                    byte[] fileContent = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(fileContent);
                    string text = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                    text = text.Replace("\n", "");
                    text = await ContentConverter.HtmlToMd(text);
                    contents.Add(text);
                }

                ContentImported?.Invoke(contents.ToList());
            };
        }

        
        private async void SetupOfficeTransfer()
        {
            //TODO put this back in
            //var fw = new FolderWatcher(NuSysStorages.PowerPointTransferFolder);
            //fw.FilesChanged += async delegate
            //{            
            //    var foundUpdate = await NuSysStorages.PowerPointTransferFolder.TryGetItemAsync("update.nusys").AsTask();
            //    if (foundUpdate == null)
            //    {
            //        Debug.WriteLine("no update yet!");
            //        return;
            //    }
            //    await foundUpdate.DeleteAsync();

            //    var transferFiles = await NuSysStorages.PowerPointTransferFolder.GetFilesAsync().AsTask();
            //    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            //    foreach (var file in transferFiles) { 

            //        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            //        {
            //            var lines = await FileIO.ReadLinesAsync(file);
            //            if (lines[0].EndsWith(".png"))
            //            {
            //                var str = lines[0];
            //                var imageFile = await NuSysStorages.Media.GetFileAsync(lines[0]).AsTask();
            //                var p = CompositeTransform.Inverse.TransformPoint(new Point(250, 200));
            //                var nodeVm = CreateNewNode("null",NodeType.Image, p.X, p.Y, imageFile);//TODO make actual Id's
            //            } else {
            //                var readFile = await FileIO.ReadTextAsync(file);
            //                var p = CompositeTransform.Inverse.TransformPoint(new Point(250, 200));
            //                var nodeVm2 = CreateNewNode("null",NodeType.Richtext, p.X, p.Y, readFile);//TODO make actual Id's
            //            }
            //        });
            //    }

            //    foreach (var file in transferFiles)
            //    {
            //        await file.DeleteAsync();
            //    }
            //};
        }

        

    }
}
