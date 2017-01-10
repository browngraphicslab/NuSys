using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ParserHelper
{
    public class SiteScoreTaker
    {
        private static double _numberOfImageTags = 0;
        private static double _numberOfTextTags = 0;
        private static double _numberOfHeaderTags = 0;

        private static double _cumulativeImageArea = 0;
        private static double _cumulativeCharacterCount = 0;

        public static SiteScore ScoreSite(HtmlDocument doc)
        {
            _numberOfImageTags = 0;
            _numberOfHeaderTags = 0;
            _numberOfHeaderTags = 0;
            _cumulativeImageArea = 0;
            _cumulativeCharacterCount = 0;
            RecursiveSearch(doc.DocumentNode);

            return new SiteScore()
            {
                AverageImageSize = _cumulativeImageArea / _numberOfImageTags,
                AverageTextBlockSize = _cumulativeCharacterCount / _numberOfTextTags,
                TextImageRatio = _numberOfTextTags / _numberOfImageTags,
                HeaderTextRatio = _numberOfHeaderTags / _numberOfTextTags
            };
        }

        private static void RecursiveSearch(HtmlNode node)
        {
            if (node.Name == "script")
            {
                return;
            }
            if (node.Name == "p")
            {
                _numberOfTextTags++;
                _cumulativeCharacterCount += HtmlImporter.RecursiveSpan(node).Length;
                return;
            }
            var re = new Regex(@"h\d");
            if (re.IsMatch(node.Name))
            {
                _numberOfHeaderTags++;
            }
            
            if (node.Name == "img")
            {
                _numberOfImageTags++;
                double width = node.GetAttributeValue("width", 0);
                double height = node.GetAttributeValue("height", 0);
                _cumulativeImageArea += width * height;
            }

            foreach (var child in node.ChildNodes)
            {
                RecursiveSearch(child);
            }
        }
    }
}
