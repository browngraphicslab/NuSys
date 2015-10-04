using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MicrosoftOfficeInterop;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private static string _currentFilePath;
        private static string _previousFilePath;

        public MainPage()
        {
            this.InitializeComponent();
            _currentFilePath = null;
            _previousFilePath = null;

            OfficeInterop.SaveWordAsPdf(@"C:\Users\Philipp\Desktop\test.docx");
        }

        public static void Run()
        {

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

        }

        private static void ProcessWordFile(string wordFilePath)
        {

        }
    }
}
