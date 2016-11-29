using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web.Http;

namespace NusysServer.Controllers
{
    public class MuPdfController : ApiController
    {

        [DllImport("mupdftest")]
        public static extern IntPtr Open(byte[] data, int length);

        [DllImport("mupdftest")]
        public static extern void ActivateDocument(IntPtr document);

        [DllImport("mupdftest")]
        public static extern void Free(IntPtr pointer);

        [DllImport("mupdftest")]
        public static extern int RenderPage(int width, int height, out IntPtr output);
        [DllImport("mupdftest")]
        public static extern int RenderToPng(int width, int height, IntPtr data);

        [DllImport("mupdftest")]
        public static extern int GetPageWidth();

        [DllImport("mupdftest")]
        public static extern int GetPageHeight();
        [DllImport("mupdftest")]
        public static extern int GetNumComponents();
        [DllImport("mupdftest")]
        public static extern int GetNumPages();
        [DllImport("mupdftest")]
        public static extern bool GotoPage(int page);

        [DllImport("mupdftest")]
        public static extern void DeleteArray(IntPtr pointer);

        // GET api/<controller>
        public IEnumerable<string> Get()
        {

            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData("http://cs.brown.edu/~peichmann/downloads/timesketch.pdf");

            var d = Open(imageBytes, imageBytes.Length);
            ActivateDocument(d);


            return new string[]
            {
                "pdf width: ", GetPageWidth().ToString(),
                "pdf height: ", GetPageHeight().ToString(),
                "pdf pages: ", GetNumPages().ToString()
            };
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