using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace MicrosoftOfficeInterop
{
    class Program
    {
        private static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string DirToWatch = DocumentsPath + @"\NuSys\OfficeToPdf";
        private static readonly string DirToWriteTo = DocumentsPath + @"\NuSys\Media";
        private static string _currentFilePath;
        private static string _previousFilePath;
        static void Main()
        {
            _currentFilePath = null;
            _previousFilePath = null;
            Run();
        }

        public static void Run()
        {
            var watcher = new FileSystemWatcher()
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Path = DirToWatch,
                Filter = "*.nusys",
                EnableRaisingEvents = true
            };

            File.Delete(DirToWatch + @"\path_to_pdf.nusys");
            File.Delete(DirToWatch + @"\path_to_office.nusys");

            watcher.Changed += OnChanged;

            Console.WriteLine("Watching for Office to PDF conversion requests from NuSys...");
            while (true) { }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                if (e.Name != "path_to_office.nusys") return;
                _currentFilePath = File.ReadAllText(e.FullPath);
                if (_previousFilePath == _currentFilePath) return; // prevent repeated calls
                _previousFilePath = _currentFilePath;
                var pathToOfficeFile = _currentFilePath; // path to .docx/.pptx file
                ProcessOfficeFile(pathToOfficeFile);
            }
            catch
            {
                // ignore
            }
        }

        private static void ProcessOfficeFile(string officeFilePath)
        {
            if (officeFilePath.Length < 6) return;
            if (officeFilePath.Last() != 'x') return;
            var extension = GetExtension(officeFilePath, 4);
            switch (extension)
            {
                case ".pptx":
                    ProcessPowerPointFile(officeFilePath);
                    break;
                case ".docx":
                    ProcessWordFile(officeFilePath);
                    break;
            }
            Thread.Sleep(500);
        }

        private static string GetExtension(string filePath, int extensionLength)
        {
            return filePath.Substring(filePath.Length - 1 - extensionLength);
        }

        private static string GetFileName(string filePath)
        {
            var startPos = filePath.LastIndexOf(@"\", StringComparison.Ordinal) + 1;
            var endPos = filePath.LastIndexOf(".", StringComparison.Ordinal);
            return filePath.Substring(startPos, endPos - startPos);
        }

        private static void ProcessPowerPointFile(string powerPointFilePath)
        {
            var pathToPdfFile = OfficeInterop.SavePresentationAsPdf(powerPointFilePath,
                DirToWriteTo + @"\" + GetFileName(powerPointFilePath) + ".pdf");
            File.WriteAllText(DirToWatch + @"\path_to_pdf.nusys", pathToPdfFile);
            Console.WriteLine("Sent path: " + pathToPdfFile);
        }

        private static void ProcessWordFile(string wordFilePath)
        {
            var pathToPdfFile = OfficeInterop.SaveWordAsPdf(wordFilePath,
                DirToWriteTo + @"\" + GetFileName(wordFilePath) + ".pdf");
            File.WriteAllText(DirToWatch + @"\path_to_pdf.nusys", pathToPdfFile);
            Console.WriteLine("Sent path: " + pathToPdfFile);
        }
    }
}
