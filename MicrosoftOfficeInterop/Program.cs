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
            File.Delete(DirToWatch + @"\path_to_pptx.nusys");

            watcher.Changed += OnChanged;

            Console.WriteLine("Watching for Office to PDF conversion requests from NuSys...");
            while (true) { }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                if (e.Name != "path_to_pptx.nusys") return;
                _currentFilePath = File.ReadAllText(e.FullPath);

                if (_previousFilePath == _currentFilePath) return; // prevent repeated calls
                _previousFilePath = _currentFilePath;
                var pathToOfficeFile = _currentFilePath; // path to .docx/.pptx file
                if (pathToOfficeFile.Length < 6) return;
                if (pathToOfficeFile.Last() != 'x') return;
                var extension = pathToOfficeFile.Substring(pathToOfficeFile.Length - 5);
                switch (extension)
                {
                    case ".pptx":
                        var pathToPdfFile = OfficeInterop.SavePresentationAsPdf(pathToOfficeFile);
                        pathToPdfFile = pathToPdfFile + ".pdf";
                        Console.WriteLine("PDF PATH: " + pathToPdfFile);
                        File.WriteAllText(DirToWatch + @"\path_to_pdf.nusys", pathToPdfFile);
                        Thread.Sleep(500);
                        break;
                    case ".docx":
                        //TODO
                        break;
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
