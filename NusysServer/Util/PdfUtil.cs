using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using MuPDFLib;

namespace NusysServer.Util
{
    public class PdfUtil
    {
        public static List<byte[]> GetPdfPageBytes(string pdfFilePath)
        {
            int maxSideSize = 2500;

            //create the pdf object
            var pdf = new MuPDF("C:\\Users\\graphics_lab\\Downloads\\CS15_Lecture_19_Hashing_11_12_15.pdf", "");
            pdf.Initialize();

            var byteArrayList = new List<byte[]>(pdf.PageCount);

            var converter = new ImageConverter();//create an image converter
            for (int p = 1; p <= pdf.PageCount; p++)//for each 1-based page...
            {
                pdf.Page = p;//set the page

                var ratio = pdf.Width / pdf.Height;//get the size ratio
                int width;
                int height;
                if (ratio > 1) //depending on the bigger side, set the height and width
                {
                    width = Convert.ToInt32(maxSideSize);
                    height = Convert.ToInt32(maxSideSize / ratio);
                }
                else
                {
                    height = Convert.ToInt32(maxSideSize);
                    width = Convert.ToInt32(maxSideSize / ratio);
                }


                var img = pdf.GetBitmap(width, height, 1, 1, 0, RenderType.RGB, false, false, 50000); //get the image as a bitmap
                var bytes = converter.ConvertTo(img, typeof(byte[])) as byte[];
                byteArrayList.Add(bytes);
            }
            return byteArrayList;
        }
    }
}