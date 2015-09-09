using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public class OfficeIntermediate
    {

        /// <summary>
        /// Takes in a storageFile (either .docx or .pptx), waits for OfficeInterop to convert
        /// it to PDF, and opens the PDF in the workspace.
        /// </summary>
        /// <param name="storageFile"></para//m>
        /// <returns></returns>
        public static async Task WatchForOfficeConversions(StorageFile storageFile)
        {
            var taskComplete = false;
            var folder = NuSysStorages.OfficeToPdfFolder;
            string previousPathToPdf = null;
            var folderWatcher = new FolderWatcher(NuSysStorages.OfficeToPdfFolder);
            folderWatcher.FilesChanged += async () =>
            {
                var files = await NuSysStorages.OfficeToPdfFolder.GetFilesAsync();
                foreach (var pdfPathFile in files.Where(file => file.Name == "path_to_pdf.nusys"))
                {
                    var tempPath = await FileIO.ReadTextAsync(pdfPathFile);
                    if (tempPath == previousPathToPdf) continue;
                    previousPathToPdf = tempPath;
                    var pdfFilePath = tempPath;
                    if (string.IsNullOrEmpty(pdfFilePath)) continue;
                    storageFile = await StorageFile.GetFileFromPathAsync(pdfFilePath);
                    taskComplete = true;
                }
            };
            var outputFile = await StorageUtil.CreateFileIfNotExists(folder, "path_to_office.nusys");
            await FileIO.WriteTextAsync(outputFile, storageFile.Path); // write path to office file
            while (!taskComplete) { await Task.Delay(50); } // loop until office file is converted and opened in workspace
            await DeleteInteropTransferFiles(); // to prevent false file-change notifications

            /*

            this.RenderedPages = await PdfRenderer.RenderPdf(pdfStorageFile);
            this.PageCount = (uint)this.RenderedPages.Count();
            this.CurrentPageNumber = 0;
            var firstPage = RenderedPages[0]; // to set the aspect ratio of the node
            this.Width = firstPage.PixelWidth;
            this.Height = firstPage.PixelHeight;
            this.InkContainer.Capacity = (int)this.PageCount;
            for (var i = 0; i < PageCount; i++)
            {
                this.InkContainer.Add(new HashSet<InqLine>());
            }

    */
        }


        /// <summary>
        /// Deletes all .nusys files involved in the office to PDF conversion process
        /// in order to prevent false-flags and accidental creation of PDF nodes.
        /// </summary>
        /// <returns></returns>
        private static async Task DeleteInteropTransferFiles()
        {
            var path = NuSysStorages.OfficeToPdfFolder.Path;
            var pathToOfficeFile = await StorageFile.GetFileFromPathAsync(path + @"\path_to_office.nusys");
            var pathToPdfFile = await StorageFile.GetFileFromPathAsync(path + @"\path_to_pdf.nusys");
            await pathToOfficeFile.DeleteAsync(StorageDeleteOption.PermanentDelete); // PermanentDelete bypasses the Recycle Bin
            await pathToPdfFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
    }
}
