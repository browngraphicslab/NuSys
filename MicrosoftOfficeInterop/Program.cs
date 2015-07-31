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
            //const string dirToWatch = @"C:\Users\Gary\Documents\NuSys\OfficeToPdf";
            var watcher = new FileSystemWatcher()
            {
                //NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName |
                //               NotifyFilters.DirectoryName,
                NotifyFilter = NotifyFilters.LastWrite,
                Path = DirToWatch,
                Filter = "*.nusys",
                EnableRaisingEvents = true
            };

            File.Delete(DirToWatch + @"\path_to_pdf.nusys");
            File.Delete(DirToWatch + @"\path_to_pptx.nusys");

            watcher.Changed += OnChanged;

            //Console.WriteLine("Press 'q' to quit the sample.");
            //while (Console.Read() != 'q') { }
            while (true) { }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("FILE CHANGED: " + e.FullPath + " " + e.ChangeType);
            try
            {
                if (e.Name != "path_to_pptx.nusys") return;
                _currentFilePath = File.ReadAllText(e.FullPath);

                //File.Delete(e.FullPath);

                if (_previousFilePath == _currentFilePath) return; // prevent repeated calls
                _previousFilePath = _currentFilePath;
                var pathToOfficeFile = _currentFilePath; // path to .docx/.pptx file
                Console.WriteLine("CONTENTS: " + pathToOfficeFile);
                if (pathToOfficeFile.Length < 6) return;
                if (pathToOfficeFile.Last() != 'x') return;
                var length = pathToOfficeFile.Length;
                var extension = pathToOfficeFile.Substring(length - 5);
                Console.WriteLine("EXTENSION: "+ extension);
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
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
