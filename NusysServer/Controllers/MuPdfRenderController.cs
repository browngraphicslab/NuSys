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
using System.Web.Http;

namespace NusysServer.Controllers
{
    public class MuPdfRenderController : ApiController
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


        // GET api/<controller>

        public HttpResponseMessage Get()
        {

            // get the bytes of a pdf
            var webClient = new WebClient();
            byte[] pdfBytes = webClient.DownloadData("http://cs.brown.edu/~peichmann/downloads/cted.pdf");

            // Open the pdf with MuPdf a store a reference to it (doc)
            var doc = Open(pdfBytes, pdfBytes.Length);

            // Active the pdf document
            ActivateDocument(doc);

            // Goto a page
            GotoPage(1);

            // Get aspect ratio of the page
            var aspectRatio = GetPageWidth()/(double)GetPageHeight();

            // Render the Page
            var size = 2000;
            var numBytes = RenderPage((int)(size*aspectRatio), size);

            // Get a reference to the buffer that contains the rendererd page
            var buffer = GetBuffer();

            // Copy the buffer from unmanaged to managed memory (mngdArray contains the bytes of the pdf page rendered as PNG)
            byte[] mngdArray = new byte[numBytes];
            try
            {
                Marshal.Copy(buffer, mngdArray, 0, numBytes);
            }
            catch (Exception e)
            {
                // Return the exception as HTTP response
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StringContent(e.Message);
                return result;
            }
            finally
            {
                // Free up memory used by active pdf document
                Dispose(doc);
            }
            // Return the rendered image as HTTP response
            var r = new HttpResponseMessage(HttpStatusCode.OK);
            r.Content = new ByteArrayContent(mngdArray);
            r.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return r;
            
        }



        // GET api/<controller>/5
        public string Get(int id)
        {
            var str = "result";


            return str;
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