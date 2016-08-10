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
        private List<ParseItem> _parsedItems = new List<ParseItem>();

        public HTMLParser(ICanvasResourceCreator resourceCreator)
        {
            _resourceCreator = resourceCreator;
            _textFormat = new CanvasTextFormat();
            _textFormat.WordWrapping = CanvasWordWrapping.Wrap;
            _textFormat.FontSize = 10;
        }

        public CanvasTextLayout GetParsedText(string html, double canvasHeight, double canvasWidth)
        {
            _parsedItems = new List<ParseItem>();
            var htmlDocument = GetHTMLDocumentFromString(html);
            RecursiveParsing(htmlDocument.DocumentNode.ChildNodes, 0);
            var text = HTMLHelper.StripTagsRegex(AddWhiteSpace(html));
            var textLayout = new CanvasTextLayout(_resourceCreator, text, _textFormat, (float) canvasWidth, (float) canvasHeight);
            ApplyFormatting(textLayout);
            return textLayout;

        }

        private void ApplyFormatting(CanvasTextLayout textLayout)
        {
            foreach (var parsedItem in _parsedItems)
            {
                Debug.WriteLine(parsedItem.Size);

                if (parsedItem.Tag == "b")
                {
                    textLayout.SetFontWeight(parsedItem.StartIndex, parsedItem.Length, FontWeights.ExtraBold);
                }

                if (parsedItem.Tag == "u")
                {
                    textLayout.SetUnderline(parsedItem.StartIndex, parsedItem.Length, true);
                }

                if (parsedItem.Tag == "i")
                {
                    textLayout.SetFontStyle(parsedItem.StartIndex, parsedItem.Length, FontStyle.Italic);
                }

                if (parsedItem.Tag == "strike")
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
        
        private void RecursiveParsing(IEnumerable<HtmlNode> children, int currentIndex)
        {
            var characterIndex = currentIndex;
            foreach (HtmlNode node in children)
            {
                if (node.HasChildNodes)
                {
                    RecursiveParsing(node.ChildNodes, currentIndex);
                }

                var innerString = HTMLHelper.StripTagsRegex(node.InnerHtml);
                var item = new ParseItem();
                item.Text = innerString;
                item.Length = innerString.Length;
                item.StartIndex = characterIndex;
                item.Tag = node.Name;
                item.Size = node.GetAttributeValue("size", 3);
                _parsedItems.Add(item);
                currentIndex += item.Length;
                characterIndex += item.Length;
            }
        }
        private string AddWhiteSpace(string htmlString)
        {
            htmlString = htmlString.Replace("<br>", "\n");
            htmlString = htmlString.Replace("<p>", "\n \t");
            htmlString = htmlString.Replace("</p>", "\n");
            htmlString = htmlString.Replace("<ul>", "");
            htmlString = htmlString.Replace("</ul>", "\n");
            htmlString = htmlString.Replace("<ol>", "");
            htmlString = htmlString.Replace("</ol>", "\n");
            htmlString = htmlString.Replace("<li>", "\n \u2022 \u0020");
            htmlString = htmlString.Replace("&nbsp;", " ");
            //htmlString = htmlString.Replace("<font size=\"5\">", "<title>");
            //htmlString = htmlString.Replace("<font size=\"4\">", "<subtitle>");
            //htmlString = htmlString.Replace("<font size=\"3\">", "<normalText>");
            htmlString = htmlString.Replace("<div>", "");
            htmlString = htmlString.Replace("</div>", "\n");
           
            return htmlString;
        }


    }
}