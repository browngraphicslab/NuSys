using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

namespace NuSysApp
{
    class PdfRenderer
    {
        private static PdfDocument _pdfDocument;
        public static BitmapImage Image;

        /// <summary>
        /// Takes in a StorageFile, an image file in this case, and returns the BitmapImage stored in the file.
        /// (Possible TODO: add exception handling code in case a non-image file is inputted)
        /// </summary>
        /// <param name="file">Image file (jpg, png, etc.)</param>
        /// <returns>A BitmapImage of the input image file</returns>
        private static async Task<BitmapImage> GetBitmapImageAsync(IStorageFile file)
        {
            var supportedTypes = new List<string> { ".jpeg", ".jpg", ".png" };
            if (!supportedTypes.Contains(file.FileType)) return null;
            var src = new BitmapImage();
            src.SetSource(await file.OpenAsync(FileAccessMode.Read));
            return src;
        }

        public static async Task<uint> GetPageCount(StorageFile pdfStorageFile)
        {
            try
            {
                _pdfDocument = await PdfDocument.LoadFromFileAsync(pdfStorageFile);
                return _pdfDocument.PageCount;
            }
            catch
            {
                throw new Exception("Can't get page count");
            }
        }

        public static async Task<List<BitmapImage>> RenderPdf(StorageFile pdfStorageFile, double zoomFactor = 1.0)
        {
            try
            {
                _pdfDocument = await PdfDocument.LoadFromFileAsync(pdfStorageFile);
                var numPages = _pdfDocument.PageCount;
                Debug.WriteLine("number of pages: {0}", numPages);
                var pages = new List<BitmapImage>();
                if (_pdfDocument != null && numPages > 0)
                {
                    for (uint pageNum = 0; pageNum < numPages; ++pageNum)
                    {
                        // Get PDF page
                        var pdfPage = _pdfDocument.GetPage(pageNum);
                        if (pdfPage == null) throw new NullReferenceException("Couldn't read all pages");
                        // generate a bitmap of the page
                        var tempStorageFolder = ApplicationData.Current.TemporaryFolder;
                        var pngStorageFile =
                            await
                                tempStorageFolder.CreateFileAsync(Guid.NewGuid() + ".png",
                                    CreationCollisionOption.ReplaceExisting);
                        if (pngStorageFile == null) throw new NullReferenceException("Couldn't read all pages");
                        var randomAccessStream = await pngStorageFile.OpenAsync(FileAccessMode.ReadWrite);
                        var pdfPageRenderOptions = new PdfPageRenderOptions();
                        // set zoom level
                        var pdfPageSize = pdfPage.Size;
                        pdfPageRenderOptions.DestinationHeight = (uint)(pdfPageSize.Height*zoomFactor);
                        await pdfPage.RenderToStreamAsync(randomAccessStream, pdfPageRenderOptions);
                        await randomAccessStream.FlushAsync();
                        randomAccessStream.Dispose();
                        pdfPage.Dispose();
                        pages.Add(await GetBitmapImageAsync(pngStorageFile));
                    }
                    return pages;
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("PDF rendering error caught D:");
                return null;
            }
            return null;
        }

        /// <summary>
        /// Takes in the filepath of a PDF file and updates Image (using the GetBitmapImageAsync function) to be displayed in the PDFNode.
        /// TODO: fill in the "catch" block.
        /// </summary>
        /// <param name="pdfFilePath">File path to PDF file</param>
        /// <param name="pageNumber">The desired page number to render (starts at 0)</param>
        /// <param name="zoomFactor">The zoom level of the output image </param>
        /// <returns>A BitmapImage of the specified page</returns>
        public static async Task<BitmapImage> RenderPdfPage(string pdfFilePath, uint pageNumber, double zoomFactor = 1.0)
        {
            try
            {
                // Load PDF file
                var pdfStorageFile = await KnownFolders.PicturesLibrary.GetFileAsync(pdfFilePath);
                _pdfDocument = await PdfDocument.LoadFromFileAsync(pdfStorageFile);
                if (_pdfDocument != null && _pdfDocument.PageCount > 0)
                {
                    // Get PDF page
                    var pdfPage = _pdfDocument.GetPage(pageNumber);
                    if (pdfPage != null)
                    {
                        // generate a bitmap of the page
                        var tempStorageFolder = ApplicationData.Current.TemporaryFolder;
                        var pngStorageFile = await tempStorageFolder.CreateFileAsync(Guid.NewGuid() + ".png", CreationCollisionOption.ReplaceExisting);
                        if (pngStorageFile != null)
                        {
                            System.Diagnostics.Debug.WriteLine("RENDERING PDF");
                            var randomAccessStream = await pngStorageFile.OpenAsync(FileAccessMode.ReadWrite);
                            var pdfPageRenderOptions = new PdfPageRenderOptions();

                            // set zoom level
                            var pdfPageSize = pdfPage.Size;
                            pdfPageRenderOptions.DestinationHeight = (uint)(pdfPageSize.Height * zoomFactor);
                            // render PDF page by passing pdfPageRenderOptions with DestinationLength set to the zoomed in length
                            await pdfPage.RenderToStreamAsync(randomAccessStream, pdfPageRenderOptions);

                            await randomAccessStream.FlushAsync();
                            randomAccessStream.Dispose();
                            pdfPage.Dispose();
                            Image = await GetBitmapImageAsync(pngStorageFile);
                            return Image;
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("not rendering :(");
                return null;
            }
            catch
            {
                // TODO: add some kind of catch code
                System.Diagnostics.Debug.WriteLine("PDF rendering error caught D:");
                return null;
            }
        }
    }
}
