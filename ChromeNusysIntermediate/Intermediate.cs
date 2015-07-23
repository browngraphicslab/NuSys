﻿using System;
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

namespace ChromeNusysIntermediate
{
    class Intermediate
    {

        static Stream stdin = Console.OpenStandardInput();

        static void Main(string[] args)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSysTransfer";
            Directory.CreateDirectory(dir);
            var fileDir = dir + "\\chromeSelections.nusys";

            //string input = File.ReadAllText("Assets/sample.html");

            //Debug.WriteLine(Resources.image.ToString());
           
            string input = OpenStandardStreamIn();
            while (input != null && input != "")
            {
                //input = input.Remove(0, 1);
                // input = input.Remove(input.Length - 1, 1);
                input = Regex.Unescape(input);
                string imgSrc = "http:" +
                                Regex.Match(input, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1]
                                    .Value;
                string pattern = @"<img[^>]+\>";
                Regex rgx = new Regex(pattern);
                input = rgx.Replace(input, "---IMAGE---");

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

                    imageRtf = Resources.image.ToString();
                    imageRtf = imageRtf.Replace("---IMG_W---", imgW.ToString());
                    imageRtf = imageRtf.Replace("---IMG_H---", imgH.ToString());
                    imageRtf = imageRtf.Replace("---IMG_GOAL_W---", imgGoalW.ToString());
                    imageRtf = imageRtf.Replace("---IMG_GOAL_H---", imgGoalH.ToString());
                    imageRtf = imageRtf.Replace("---IMG_DATA---", imgData);
                }

                input = HtmlToRichText(input, "");

                if (!imgSrc.Equals("http:"))
                {
                    input = input.Replace("---IMAGE---", imageRtf);
                }

                string[] line = { input };
                File.WriteAllLines(fileDir, line);
                System.IO.File.SetLastWriteTimeUtc(fileDir, DateTime.UtcNow);
                File.Move(fileDir, fileDir);
                input = OpenStandardStreamIn();
            }


            /*
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSysTransfer";
            Directory.CreateDirectory(dir);
            var fileDir = dir + "\\chromeSelections.nusys";
            string input = OpenStandardStreamIn();
            
            //if the input is the empty string, then we are no longer connected to chrome and should shut down
            while (input != null && input != "")
            {
                string imgSrc = "https://upload.wikimedia.org" + Regex.Match(input, " <img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                string pattern = @"<img[^>]+\>";
                string replacement = "IMAGE";
                Regex rgx = new Regex(pattern);
                input = rgx.Replace(input, replacement);
                
                input = HtmlToRichText(input, "");
                string[] line = {input};
                File.AppendAllLines(fileDir, line);
                input = OpenStandardStreamIn();
            } */
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
            var str = System.Text.Encoding.Default.GetString(buffer);

            return str;
        }
    }
}
