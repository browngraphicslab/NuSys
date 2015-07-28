using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;

namespace MicrosoftOfficeInterop
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenDocument(@"C:\Users\Gary\Google Drive\Brown\NuSys\MicrosoftOfficeProjects\Documents\testdoc.docx");
        }
        public static void OpenDocument(string filePath)
        {
            var wordApp = new Application {Visible = true};
            wordApp.Documents.Open(filePath);
        }
    }
}
