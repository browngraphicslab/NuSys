using System;
using System.IO;
using System.Linq;

namespace MicrosoftOfficeInterop
{
    class Program
    {
        private const string DirToWatch = @"C:\Users\Gary\Documents\NuSys\OfficeToPdf";
        private const string FileToGenerate = DirToWatch + @"\bluh.nusys";
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
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName |
                               NotifyFilters.DirectoryName,
                Path = DirToWatch,
                Filter = "*.nusys",
                EnableRaisingEvents = true
            };
            //watcher.Created += OnChanged;
            //watcher.Deleted += OnChanged;
            watcher.Changed += OnChanged;
            //watcher.Renamed += OnRenamed;

            Console.WriteLine("Press 'q' to quit the sample.");
            File.WriteAllText(FileToGenerate, "generated text");
            while (Console.Read() != 'q') { }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("FILE: " + e.FullPath + " " + e.ChangeType);
            try
            {
                if (e.Name != "path_to_pptx.nusys") return;
                _currentFilePath = File.ReadAllText(e.FullPath);
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
                        //OfficeInterop.SavePresentationAsPdf(pathToOfficeFile, DirToWatch + @"\convertedPDF.pdf");
                        pathToPdfFile = pathToPdfFile + ".pdf";
                        Console.WriteLine("PDF PATH: " + pathToPdfFile);
                        File.WriteAllText(DirToWatch + @"\path_to_pdf.nusys", pathToPdfFile);
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
