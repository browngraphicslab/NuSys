﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;
using ChromeNusysIntermediate.Properties;
using SautinSoft;
using Newtonsoft.Json.Linq;
using Image = System.Drawing.Image;

namespace ChromeNusysIntermediate
{
    class Intermediate
    {

        static Stream stdin = Console.OpenStandardInput();
        [STAThread]
        static void Main(string[] args)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\NuSys\\ChromeTransfer";
            var fileDir = dir + "\\selectionX.nusys";

            string input = OpenStandardStreamIn();
            var selections = new ArrayList();
            
            while (input != null && input != "")
            {
                var b = input.Remove(0, 1);
                b = b.Remove(b.Length - 1, 1);
                File.WriteAllText(fileDir, b);
                /*
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
                    string imgSrc = Regex.Match(input, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    if (imgSrc != "" && !imgSrc.Contains("http"))
                    {
                        imgSrc = "http://" + imgSrc;
                    }


                    selections.Add(\
                    File.WriteAllText(fileDir, input);
                   // System.IO.File.SetLastWriteTimeUtc(fileDir, DateTime.UtcNow);
                    //File.Move(fileDir, fileDir);
                }*/

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
