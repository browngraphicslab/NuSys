using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ChromeNusysIntermediate.Properties;
using SautinSoft;
using Newtonsoft.Json.Linq;

namespace ChromeNusysIntermediate
{
    class Intermediate
    {

        static Stream stdin = Console.OpenStandardInput();

        static void Main(string[] args)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\ChromeTransfer";
            var fileDir = dir + "\\selectionX.nusys";

            string input = OpenStandardStreamIn();
            while (input != null && input != "")
            {
                var b = input.Remove(0, 1);
                b = b.Remove(b.Length - 1, 1);
                var a = JArray.Parse(b);

                var count = 0;

                foreach (var el in a.Children())
                {
                    count++;
                    fileDir = fileDir.Remove(fileDir.Length - 7, 7);
                    fileDir += count.ToString() + ".nusys";
                    input = el.ToString();
                    string imgSrc = "http:" +
                                    Regex.Match(input, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1]
                                        .Value;
                    string pattern = @"<img[^>]+\>";
                    Regex rgx = new Regex(pattern);
                    // input = rgx.Replace(input, "---IMAGE---");

                    string imageRtf = string.Empty;
                    if (!imgSrc.Equals("http:"))
                    {
                        var imgPath = dir + "\\image.png";
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.DownloadFile(imgSrc, imgPath);
                        }


                        var img = Image.FromFile(imgPath);

                        var imgW = img.Width;
                        var imgH = img.Height;
                        img.Dispose();
                        var imgGoalW = imgW * 15;
                        var imgGoalH = imgH * 15;
                        var imgData = BitConverter.ToString(File.ReadAllBytes(imgPath)).Replace("-", "");

                        File.Delete(imgPath);

                        imageRtf = Resources.image.ToString();
                        imageRtf = imageRtf.Replace("---IMG_W---", imgW.ToString());
                        imageRtf = imageRtf.Replace("---IMG_H---", imgH.ToString());
                        imageRtf = imageRtf.Replace("---IMG_GOAL_W---", imgGoalW.ToString());
                        imageRtf = imageRtf.Replace("---IMG_GOAL_H---", imgGoalH.ToString());
                        imageRtf = imageRtf.Replace("---IMG_DATA---", imgData);
                    }

                    //input = HtmlToRichText(input, "");
                    var i = input.IndexOf("________________________________________________________");
                    // input = input.Remove(i, input.Length - i) + "}";

                    if (!imgSrc.Equals("http:"))
                    {
                        //  input = input.Replace("---IMAGE---", imageRtf);
                    }

                    string[] line = { input };
                    File.WriteAllLines(fileDir, line);
                    System.IO.File.SetLastWriteTimeUtc(fileDir, DateTime.UtcNow);
                    File.Move(fileDir, fileDir);
                }

                input = OpenStandardStreamIn();
            }
        }

        private static string HtmlToRichText(string html, string baseUrl)
        {
            SautinSoft.HtmlToRtf h = new HtmlToRtf();
            List<HtmlToRtf.SautinImage> imgList = new List<HtmlToRtf.SautinImage>();
            h.BaseURL = baseUrl;

            // Convert HTML to and place all images inside imgList.
            var reuslt = h.ConvertString(html, imgList);

            return reuslt;
        }

        private static string OpenStandardStreamIn()
        {
            //// We need to read first 4 bytes for length information

            int length = 0;
            byte[] bytes = new byte[4];

            stdin.Read(bytes, 0, 4);
            length = System.BitConverter.ToInt32(bytes, 0);

            var buffer = new byte[length];
            stdin.Read(buffer, 0, length);
            var str = System.Text.Encoding.UTF8.GetString(buffer);

            return Regex.Unescape(str);
        }
    }
}
