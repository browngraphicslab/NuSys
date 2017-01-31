using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Text;
using Windows.UI.Xaml.Input;
using HtmlAgilityPack;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public struct ParseItem
    {
        public string Tag;
        public int Size;
        public string Text;
        public int StartIndex;
        public int Length;
    }
    public class HTMLParser
    {
        private ICanvasResourceCreator _resourceCreator;
        private CanvasTextFormat _textFormat;

        public HTMLParser(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat = null)
        {
            _resourceCreator = resourceCreator;

            if (textFormat == null)
            {
                _textFormat = new CanvasTextFormat();
                _textFormat.WordWrapping = CanvasWordWrapping.Wrap;
                _textFormat.FontSize = UIDefaults.FontSize;
                _textFormat.FontFamily = UIDefaults.TextFont;
            }
            else
            {
                _textFormat = textFormat;
            }


        }

        /// <summary>
        /// Update the canvas text format used to format text
        /// </summary>
        /// <param name="newCanvasTextFormat"></param>
        public void UpdateCanvasTextFormat(CanvasTextFormat newCanvasTextFormat)
        {
            _textFormat = newCanvasTextFormat;

        }

        public CanvasTextLayout GetParsedText(string html, double canvasWidth, double canvasHeight)
        {
            //_textFormat.FontSize = 13;
            var parsedItems = new List<ParseItem>();
            var htmlDocument = GetHTMLDocumentFromString(html);
            RecursiveParsing(htmlDocument.DocumentNode.ChildNodes, 0, parsedItems);
            var text = System.Net.WebUtility.HtmlDecode(HTMLHelper.StripTagsRegex(AddWhiteSpace(html)));
            var textLayout = new CanvasTextLayout(_resourceCreator, text, _textFormat, (float) canvasWidth, (float) canvasHeight);
            ApplyFormatting(textLayout, parsedItems);
            return textLayout;

        }

        private void ApplyFormatting(CanvasTextLayout textLayout, List<ParseItem> parsedItems)
        {
            foreach (var parsedItem in parsedItems.ToArray())
            {

                if (parsedItem.Tag == "strong")
                {
                    textLayout.SetFontWeight(parsedItem.StartIndex, parsedItem.Length, FontWeights.Bold);
                }

                if (parsedItem.Tag == "u")
                {
                    textLayout.SetUnderline(parsedItem.StartIndex, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "em")
                {
                    textLayout.SetFontStyle(parsedItem.StartIndex, parsedItem.Length, FontStyle.Italic);
                }

                if (parsedItem.Tag == "del")
                {
                    textLayout.SetStrikethrough(parsedItem.StartIndex, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "h1")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 36);
                }
                if (parsedItem.Tag == "h2")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 28);
                }
                if (parsedItem.Tag == "h3")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 22);
                }
                if (parsedItem.Tag == "h4")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 18);
                }
                if (parsedItem.Tag == "h5")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 13);
                }
                if (parsedItem.Tag == "h6")
                {
                    textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 11);
                }
                if (parsedItem.Tag == "font")
                {
                    if (parsedItem.Size == 3)
                    {
                        textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 10);
                    }
                    if (parsedItem.Size == 4)
                    {
                        textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 18);
                    }
                    if (parsedItem.Size == 5)
                    {
                        textLayout.SetFontSize(parsedItem.StartIndex, parsedItem.Length, 28);
                    }
                }

            }
        }

        private HtmlDocument GetHTMLDocumentFromString(string htmlString)
        {
            HtmlDocument document = new HtmlDocument();
            htmlString = AddWhiteSpace(htmlString);
            document.LoadHtml(htmlString);
            return document;
        }

        private void RecursiveParsing(IEnumerable<HtmlNode> children, int currentIndex, List<ParseItem> parsedItems)
        {
            var characterIndex = currentIndex;
            foreach (HtmlNode node in children)
            {
                if (node.HasChildNodes)
                {
                    RecursiveParsing(node.ChildNodes, currentIndex, parsedItems);
                }

                var innerString = HTMLHelper.StripTagsRegex(node.InnerHtml);
                var item = new ParseItem();
                item.Text = innerString;
                item.Length = innerString.Length;
                item.StartIndex = characterIndex;
                item.Tag = node.Name;
                item.Size = node.GetAttributeValue("size", 3);
                parsedItems.Add(item);
                currentIndex += item.Length;
                characterIndex += item.Length;
            }
        }

        /// <summary>
        /// Replaces elements with whtiespace
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private string AddWhiteSpace(string htmlString)
        {
            htmlString = htmlString ?? "";
            htmlString = htmlString.Replace("<br>", "\n");
            htmlString = htmlString.Replace("<p>", "");
            htmlString = htmlString.Replace("</p>", "");
            htmlString = htmlString.Replace("<ul>", "");
            htmlString = htmlString.Replace("</ul>", "\n");
            htmlString = htmlString.Replace("<ol>", "");
            htmlString = htmlString.Replace("</ol>", "\n");
            htmlString = htmlString.Replace("<li>", "\u2022 \u0020");
            htmlString = htmlString.Replace("&nbsp;", " ");
            htmlString = htmlString.Replace("<div>", "");
            htmlString = htmlString.Replace("</div>", "\n");
           
            return htmlString;
        }


    }
}