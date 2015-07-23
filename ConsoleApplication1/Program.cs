using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using SautinSoft;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {


            string filename = args[0];
            string outputFileName = args[1];


            SautinSoft.HtmlToRtf h = new HtmlToRtf();
            string htmlPath = filename;
            string htmlString = File.ReadAllText(htmlPath);
            string rtfPath = outputFileName;

            List<HtmlToRtf.SautinImage> imgList = new List<HtmlToRtf.SautinImage>();

            h.BaseURL = @"d:\";

            // Convert HTML to and place all images inside imgList.
            var reuslt = h.ConvertString(htmlString, imgList);
            File.WriteAllText(outputFileName, reuslt);

            // Save all images to HDD.
            foreach (HtmlToRtf.SautinImage img in imgList)
            {
                
            }



            // Save document into PDF Format




            /*
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();

            // C# doesn't have optional arguments so we'll need a dummy value
            object oMissing = System.Reflection.Missing.Value;

            // Get list of Word files in specified directory
            DirectoryInfo dirInfo = new DirectoryInfo(@"C:\Users\phili_n4ua7ts\Desktop");


            word.Visible = false;
            word.ScreenUpdating = false;
           
   
                // Cast as Object for word Open method
                Object filename = (Object)@"C:\Users\phili_n4ua7ts\Desktop\bla2.html";

                // Use the dummy value as a placeholder for optional arguments
                Document doc = word.Documents.Open(ref filename, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                doc.Activate();

                object outputFileName = @"C:\Users\phili_n4ua7ts\Desktop\bla_out.rtf";
                object fileFormat = WdSaveFormat.wdFormatRTF;

                // Save document into PDF Format
                doc.SaveAs(ref outputFileName,
                    ref fileFormat, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                // Close the Word document, but leave the Word application open.
                // doc has to be cast to type _Document so that it will find the
                // correct Close method.                
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
                doc = null;
            

// word has to be cast to type _Application so that it will find
// the correct Quit method.
((_Application)word).Quit(ref oMissing, ref oMissing, ref oMissing);
            word = null;
            */

        }
    }
}
