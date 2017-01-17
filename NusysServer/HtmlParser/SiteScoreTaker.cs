﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace NusysServer
{
    public class SiteScoreTaker
    {
        private static double _numberOfImageTags = 0;
        private static double _numberOfTextTags = 0;
        private static double _numberOfHeaderTags = 0;

        private static double _cumulativeImageArea = 0;
        private static double _cumulativeCharacterCount = 0;


        public static HtmlNode GetArticle(HtmlDocument doc)
        {
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
            if (HtmlImporter._tagsToRemove.IsMatch(node.Name.ToLower()) || HtmlImporter._tagsToRemove.IsMatch(node.Id.ToLower()) || HtmlImporter._tagsToRemove.IsMatch(node.GetAttributeValue("class", "").ToLower()))
            {
                return null;
            }

            //These are the tags that we want to search for in a website, article is generic and bodyCotent is a wikipedia thing
            var re = new Regex("^(?:article|bodyContent)$");
            //These are tags we don't want to see
            var re1 = new Regex("(?:review|comment)");
            if ((re.IsMatch(node.Name) || re.IsMatch(node.Id ?? "") ||
                                      re.IsMatch(node.GetAttributeValue("class", ""))) && !re1.IsMatch(node.GetAttributeValue("class", "")))
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