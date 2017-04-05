// compile with: /unsafe
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Windows.Devices.Bluetooth.Advertisement;

namespace NusysServer.Controllers
{
    public class MuPdfTextController : ApiController
    {
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Open(byte[] data, int length);
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ActivateDocument(IntPtr document);
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderPage(int width, int height);
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextBytes(byte[] sb);
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBuffer();
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageWidth();
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageHeight();
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumComponents();
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumPages();
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GotoPage(int page);
        [DllImport("mupdfapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Dispose(IntPtr pointer);


        public async Task<HttpResponseMessage> Get()
        {
            
            var r2 = new HttpResponseMessage(HttpStatusCode.OK);
            r2.Content = new StringContent(await Get(5));
            return r2;
            // get the bytes of a pdf
            var webClient = new WebClient();
            byte[] pdfBytes = webClient.DownloadData("http://cs.brown.edu/~peichmann/downloads/cted.pdf");

            // Open the pdf with MuPdf a store a reference to it (doc)
            var doc = Open(pdfBytes, pdfBytes.Length);

            // Active the pdf document
            ActivateDocument(doc);

            // Goto a page
            GotoPage(1);

            // Allocate a buffer which the text will be written to
            byte[] textBuffer = new byte[10000];

            // Write the text on the pdf page to textBuffer
            var numTextBytes = GetTextBytes(textBuffer);

            // Convert the extracted text bytes to a utf8 string
            var textString = Encoding.UTF8.GetString(textBuffer.Take(numTextBytes).ToArray());

            // Free up memory
            Dispose(doc);

            // Return extracted text as http response
            var r = new HttpResponseMessage(HttpStatusCode.OK);
            r.Content = new StringContent(textString);
            return r;
        }
        
        [DllImport("ConsoleApplication2_122", CallingConvention = CallingConvention.Cdecl)]
        public static extern int test();


        //[DllImport("d6_2", CallingConvention = CallingConvention.Cdecl)]
        //public static unsafe extern int getImage (void* pointer);



        /*
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct test_struct
        {
            
            public string byte_array;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1234)]
            public string url;

        };*/

        public struct int_struct
        {
            public int myInt;
        };
        
        // GET api/<controller>/5
        public async Task<string> Get(int site)
        {
            try
            {
                var url = "http://www.pdf995.com/samples/pdf.pdf";
                var client = new HttpClient();
                var result = await client.GetStreamAsync(new Uri(url));
                MemoryStream ms = new MemoryStream();
                result.CopyTo(ms);
                byte[] data = ms.ToArray();

                var exePath = Constants.WWW_ROOT + "wkhtmltoimage.exe";

                var imagePath = Constants.WWW_ROOT + "temp.jpg";

                var htmlPath = Constants.WWW_ROOT + "temp.html";
                File.WriteAllBytes(htmlPath, data);

                ProcessStartInfo startInfo = new ProcessStartInfo(exePath, " --crop-h 1250 " + htmlPath + " " + imagePath);
                Process process = new Process() {StartInfo = startInfo};
                process.Start();

                return "done! " + process.ToString() + "   done";
            }
            catch (Exception e)
            {
                return "Failed: "+e.Message;
            }
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}