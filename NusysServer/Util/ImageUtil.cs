using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace NusysServer
{
    public class ImageUtil
    {
        private static object _lockObj = new object();
        public static async Task<string> GetImageUrlFromUrl(string url, string id, int pixelVerticalCutoff = 1250)
        {
            var client = new HttpClient();
            var result = await client.GetStreamAsync(new Uri(url));
            MemoryStream ms = new MemoryStream();
            result.CopyTo(ms);
            byte[] data = ms.ToArray();

            var exePath = Constants.WWW_ROOT + "wkhtmltoimage.exe";

            var imagePath = Constants.WWW_ROOT + id+".jpg";

            var htmlPath = Constants.WWW_ROOT + "temp.html";
            lock (_lockObj)
            {
                File.WriteAllBytes(htmlPath, data);

                ProcessStartInfo startInfo = new ProcessStartInfo(exePath,
                    " --crop-h " + pixelVerticalCutoff + " " + htmlPath + " " + imagePath);
                Process process = new Process() {StartInfo = startInfo};
                process.Start();

                return Constants.SERVER_ADDRESS + id + ".jpg";
            }
        }
    }
}