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

        public static int findDepth(HtmlNode node)
        {
            int max = 0;
            foreach (var child in node.ChildNodes)
            {
                var curr = findDepth(child);
                if (curr > max)
                {
                    max = curr;
                }
            }
            return max + 1;
        }

        public static HtmlNode GetArticle(HtmlDocument doc)
        {
            if (findDepth(doc.DocumentNode) < 8)
            {
                return doc.DocumentNode;
            }

            return RecursiveGetArticle(doc.DocumentNode);
        }
        /// <summary>
        /// DEPRECATED
        /// used for classifier but that is no longer in use, kept for possible future use
        /// </summary>
        /// <param name="node"></param>
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


        /// <summary>
        /// When looking through different tags, these are the ones we want to exclude 
        /// </summary>
        /// <summary>
        /// This finds out whether the website has certain tags that would make it a good candidate for search
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static HtmlNode RecursiveGetArticle(HtmlNode node)
        {
            if (HtmlImporter.TagsToRemove.IsMatch(node.Name.ToLower()) || HtmlImporter.TagsToRemove.IsMatch(node.Id.ToLower()) || HtmlImporter.TagsToRemove.IsMatch(node.GetAttributeValue("class", "").ToLower()))
            {
                return null;
            }
            
            //These are the tags that we want to search for in a website, article is generic and bodyCotent is a wikipedia thing
            var re = new Regex("^(?:article|mw-body)$");
            //These are tags we don't want to see
            var re1 = new Regex("(?:review|comment)");
            if ((re.IsMatch(node.Name) || re.IsMatch(node.Id??"") ||
                                      re.IsMatch(node.GetAttributeValue("class", "")) )&& !re1.IsMatch(node.GetAttributeValue("class","")))
            {
                return node;
            }
            //Recurse through all of the children to keep searching
            foreach (var child in node.ChildNodes)
            {
                // fun debugging times! Maybe I should throw in an await to really get you :9
                var possible = RecursiveGetArticle(child);
                if (possible != null)
                {
                    return possible;
                }
            }
            // Not the node you are looking for
            return null;
        }
    }
}
