using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Html;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using HtmlAgilityPack;

namespace NuSysApp
{
    class HtmlRichTextConverter
    {

        public static void yo() { }

        public static List<Block> GenerateBlockFromHtml(string html, string baselink)
        {
            var rtBlocks = new List<Block>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var root = doc.DocumentNode;
            string lastLinkText = "";
            foreach (var node in root.DescendantsAndSelf())
            {
                if (node.Name.Equals("img"))
                {
                    string url = node.Attributes["src"].Value;
                    if (!url.StartsWith("http"))
                    {
                        if (url.StartsWith("//"))
                        {
                            url = "https:" + url;
                        }
                        else
                        {
                            url = baselink + url;
                        }
                    }

                    Image i = new Image();
                    i.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
                    InlineUIContainer imageContainer = new InlineUIContainer();
                    imageContainer.Child = i;
                    Paragraph imageParagraph = new Paragraph();
                    imageParagraph.Inlines.Add(imageContainer);
                    rtBlocks.Add(imageParagraph);
                    lastLinkText = "";
                    

                }
                else if (node.Name.Equals("a"))
                {
                    Hyperlink link = new Hyperlink();
                    Run linkText = new Run();
                    linkText.Text = node.InnerText;
                    lastLinkText = node.InnerText;
                    link.Inlines.Add(linkText);
                    string uri = node.Attributes["href"].Value;
                    if (!uri.StartsWith("http"))
                    {
                        uri = baselink + uri;
                    }
                    link.NavigateUri = new Uri(uri);
                    link.Click += delegate (Hyperlink sender, HyperlinkClickEventArgs args)
                    {
                        Debug.WriteLine("navigate to the clicked link");
                    };
                    if (rtBlocks.Count == 0)
                    {
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(link);
                        rtBlocks.Add(paragraph);
                    }
                    else
                    {
                        Paragraph lastParagraph = rtBlocks.Last() as Paragraph;
                        lastParagraph.Inlines.Add(link);
                    }
                }
                else if (node.Name.Equals("#text"))
                {

                    var text = node.InnerText;
                    if (!string.IsNullOrEmpty(text) && !text.Equals(lastLinkText))
                    {
                        Paragraph p;
                        if (lastLinkText.Equals(""))
                        {
                            p = new Paragraph();
                            rtBlocks.Add(p);
                        }
                        else
                        {
                            p = rtBlocks.Last() as Paragraph;
                        }
                        var r = new Run();
                        r.Text = text;
                        p.Inlines.Add(r);
                        lastLinkText = "";
                    }

                }
            }
            return rtBlocks;
        }
    }
}
