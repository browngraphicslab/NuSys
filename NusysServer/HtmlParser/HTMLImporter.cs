﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace NusysServer
{
    public class HtmlImporter
    {
        //Whoever maintains this code after me, text me (though calling is preferred)
        //401-999-2779
        //This is one of my final projects within NuSys
        //-Sahil
        //(1/5/17)

        public HtmlImporter()
        {
        }

        /// <summary>
        /// These are the expressions that once detected in a title will be removed because they are some type of spam
        /// </summary>
        private static Regex _titlesToRemove =
            new Regex(
                @"(?:buy|order|subscribe|oops|join|^\d*$|advertisement|reply|seen and heard|custom solutions|(reader )?offer|save.*?(?:$|£|€|¥)\d+|(?:$|£|€|¥)\d+ off|share this story!|posted!|sent!|\d+ (?:best|worst|funniest|dumbest|most)|learn more|references?|follow( me)?|e-handbook|posted by:|credits?|^\s*thank you\s*$|\d+.*?this (?:year|day|month|hour|minute|night|decade|lifetime)|\d+.*?(?:predictions?|trends?|news?|facts?)|see also)");

        /// <summary>
        /// These are the expressions that will remove a text in a textdataholder if they are in that text
        /// </summary>
        private static Regex _contentToRemove =
            new Regex(
                @"(?:^article$|advertisement|writing\? check your grammar now!|all headlines|\(.*?(?:repl(y)?(ies)?|ratings?|comments?).*?\)|save.*?(?:$|£|€|¥)\d+|(?:$|£|€|¥)\d+ off|login.*?(?:register|create|make)|(?:posts?|photos?|articles?) by)");

        /// <summary>
        /// These are the expressions that will remove a text in a textdataholder if they are in that text
        /// </summary>
        private static Regex _shortContentToRemove = new Regex(@"(?:more.*|quiz|test)");

        /// <summary>
        /// These are the expressions that will remove a text in a textdataholder if they are in that text
        /// </summary>
        private static Regex _longTitlesToRemove = new Regex(@"(?:download.*?free|^\s*content\s*$)");

        /// <summary>
        /// These are things that are going to be removed from the text but aren't markers for spam
        /// </summary>
        private static Regex _contentToReplace =
            new Regex(@"(?:\[.*?edit.*?\]|\[.*?\d+.*?\]|\[.*?citation needed.*?\]|&.*?;|\[.*?change.*?\])");

        /// <summary>
        /// When looking through different tags, these are the ones we want to exclude 
        /// </summary>
        public static Regex _tagsToRemove = new Regex(@"(?:sidebar|quiz|aside|o-hit|top-story|stickycolumn)");

        /// <summary>
        /// This is so that I can clean the whitespace that makes the sites look messy
        /// </summary>
        private static Regex _cleanWhiteSpace = new Regex(@"\s+");

        /// <summary>
        /// This is a list of websites that we don't want to parse
        /// </summary>
        private static List<string> blacklist = new List<string>()
        {
            "mapsoftheworld.com",
            "foodnetwork",
            "allrecipies",
            "worldatlas",
            "vectorstock.com",
            "freepik.com",
            "containerstore.com",
            "yourshot.nationalgeographic",
            "myspace.com",
            "store.",
            "treesdallas.com",
            "epicurious.com",
            "krispykreme.com",
            "imdb.com",
            "wikihow.com",
            "support.",
            "currys.co.uk"
        };

        /// <summary>
        /// We do not want a sub article structure because it usually is a fragment of an article or some kind of link so we only look at the top article
        /// </summary>
        private static bool isArticleFound = false;

        /// <summary>
        /// Running this will fetch and parse the html document that is specified by the uri of any useful information
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<DataHolder>> Run(Uri url)
        {
            //Load HTML from the website
            var doc = await GetDocumentFromUri(url);
            if (doc == null)
            {
                return null;
            }
            //We only want websites that qualify as an article so we check to see if certain tags are in the site
            var articleTopNode = SiteScoreTaker.GetArticle(doc);
            if (articleTopNode == null)
            {
                return null;
            }
            _paragraphCollections = new HashSet<HashSet<HtmlNode>>();
            _citationCollections = new HashSet<List<string>>();
            _citationUrlCollections = new List<List<string>>();
            //This stores a list of all of the information 
            var models = new List<DataHolder>();
            // This recursively goes through each node in the html document, depth first, and parses data
            await RecursiveAdd(articleTopNode, models);
            isArticleFound = false;
            //We store the data for the text nodes based on the headers and the content between those headers so we need 
            //to go through and make the text data holders
            var text = "";
            foreach (var paragraphs in _paragraphCollections)
            {
                var title = "";
                var hasTitle = false;
                foreach (var node in paragraphs)
                {
                    if (!hasTitle)
                    {
                        title = _cleanWhiteSpace.Replace(_contentToReplace.Replace(RecursiveSpan(node), ""), " ");
                        hasTitle = true;
                        continue;
                    }
                    text += _cleanWhiteSpace.Replace(_contentToReplace.Replace(RecursiveSpan(node), ""), " ") + "\n";
                }
                //This gets rid of any sections that dont contain anything and are therefore not usefull, this is helpful for 
                //footers and headers
                if (string.IsNullOrWhiteSpace(text))
                {
                    //Syncs up the citations to their paragraphs
                    if (_citationUrlCollections.Any())
                        _citationUrlCollections.RemoveAt(0);
                    continue;
                }
                //If theres a tiny title then we can just aggregate all of the tiny titles into a larger block
                if (title.Length <= 5)
                {
                    continue;
                }
                //If there is a match with any of the spam filters then we remove it
                if (!isTextValid(text) || !isTitleValid(title))
                {
                    text = "";
                    continue;
                }
                //We then create the data holder and then populate the links with what we have captured
                var content = new TextDataHolder(text, title ?? "")
                {
                    links = (_citationUrlCollections.Any()) ? _citationUrlCollections.First() : null
                };
                if (_citationUrlCollections.Any())
                    _citationUrlCollections.RemoveAt(0);
                models.Add(content);
                //reset the text so we don't aggregate it all into blocks larger than we want
                text = "";
            }
            return models;
        }

        /// <summary>
        /// From a uri we send a webrequest to get the website as an htmldocument or else we return null
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<HtmlDocument> GetDocumentFromUri(Uri url)
        {
            try
            {
                var doc = new HtmlDocument();
                var webRequest = WebRequest.Create(url.AbsoluteUri);
                HttpWebResponse response = (HttpWebResponse) (await webRequest.GetResponseAsync());
                Stream stream = response.GetResponseStream();
                doc.Load(stream);
                stream.Dispose();
                return doc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Stores each header and grouping of paragraphs to be turned into dataholders for later
        /// </summary>
        private HashSet<HashSet<HtmlNode>> _paragraphCollections = new HashSet<HashSet<HtmlNode>>();

        /// <summary>
        /// This is the id of each citation in the same order as each paragraph so that we can find the links later
        /// </summary>
        private HashSet<List<string>> _citationCollections = new HashSet<List<string>>();

        /// <summary>
        /// These are the links to each document that is being cited
        /// </summary>
        private List<List<string>> _citationUrlCollections = new List<List<string>>();

        /// <summary>
        /// This recursively goes through each node in the html document and parses different types of data within
        /// </summary>
        /// <param name="node"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        private async Task RecursiveAdd(HtmlNode node, List<DataHolder> models)
        {

            if (node.Name.ToLower() == "script" || node.Name.ToLower() == "article" && isArticleFound)
            {
                return;
            }
            isArticleFound = true;

            if (_tagsToRemove.IsMatch(node.Name.ToLower()) || _tagsToRemove.IsMatch(node.Id.ToLower()) ||
                _tagsToRemove.IsMatch(node.GetAttributeValue("class", "").ToLower()))
            {
                return;
            }

            //Matches if there is a header tag (h1,h2...) or a bold tag and uses that as a title
            var reh = new Regex(@"^(?:h\d|strong)");
            if (reh.IsMatch(node.Name))
            {
                //If there is a new header we made a new spot for the following paragraphs and the citations
                // that may exist within the text
                _paragraphCollections.Add(new HashSet<HtmlNode>());
                _citationCollections.Add(new List<string>());
                _citationUrlCollections.Add(new List<string>());
                //We add the title as the first part of each section
                _paragraphCollections.Last().Add(node);
            }
            // We take the html class of the tag so that we can see if it is a list item and if so we can see if it is a citation
            var classString = node.GetAttributeValue("class", null);
            if (!string.IsNullOrEmpty(node.Id) && node.Name == "li")
            {
                //We have to parse through all of the saved citation ids to then see where to save it in our url datastructure
                var i = 0;
                foreach (var citations in _citationCollections)
                {
                    foreach (var citid in citations)
                    {
                        if (citid != node.Id) continue;
                        var url = FormatSource(getUrl(node));
                        if (url == null)
                        {
                            continue;
                        }
                        _citationUrlCollections[i].Add(url);
                        break;
                    }
                    i++;
                }
            }
            //In Wikipedia the class reference means that there is a link to a citation
            if (classString == "reference" && _citationCollections.Any())
            {
                _citationCollections.Last()?.Add(getUrl(node)?.Substring(1));

            }
            //This is most of the text that we are parsing through to get our data for our text nodes
            if ((node.Name == "p" && _paragraphCollections.Any()))
                //node.ChildNodes.Count(c => c.NodeType == HtmlNodeType.Text) > node.ChildNodes.Count() / 2)
            {
                _paragraphCollections.Last().Add(node);
            }
            //Depth first search, we need to keep searching each child
            foreach (var child in node.ChildNodes)
            {
                //Makes sure that no javascrip is being parsed
                if (child.Name.ToLower() != "script")
                {
                    await RecursiveAdd(child, models);
                }
            }
            //Little rare of a tag but this should take any videos and put their uri into a dataholder
            if (node.Name == "video")
            {
                //This gets the url that the video is based out of
                var uristring = FormatSource(node.GetAttributeValue("src", null));
                if (!string.IsNullOrEmpty(uristring))
                {
                    var uri = new Uri(uristring);
                    //The title of the video is often in the alt tag but otherwise we just search for it with that function
                    var title = node.GetAttributeValue("alt", null);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = SearchForTitle(node);
                    }
                    if (isTitleValid(title))
                    {
                        //Create the dataholder and stash it with the rest
                        var content = new VideoDataHolder(uri, title);
                        models.Add(content);
                    }
                }
            }
            //This is the standard tag for images
            if (node.Name == "img")
            {
                //We then get the image source uri
                var src = FormatSource(node.GetAttributeValue("src", null));
                //We dont want any svgs because they mess up the server
                var re = new Regex(@"(?:svg|gif|(?:info|ico)\.png)$");
                var re1 = new Regex(@"https?:\/\/[^\/]*?\..*?\/.*\.");
                if (src == null || re.IsMatch(src) || !re1.IsMatch(src))
                {
                    return;
                }
                //Sometimes videos are stored in image tags so we can get those from here
                if ((src.Contains(".mp4") || src.Contains(".webm") || src.Contains(".ogv") || src.Contains(".ogg")))
                {
                    //TODO Implement Video Code
                    return;
                }
                //We then get the height and width of the image
                var height = node.GetAttributeValue("height", 1000);
                var width = node.GetAttributeValue("width", 1000);
                if (height > 75 && width > 75)
                {
                    //Usually there is a title in the alt tag but if not then we can search for a caption
                    var title = node.GetAttributeValue("alt", "");
                    if (string.IsNullOrEmpty(title))
                    {
                        title = SearchForTitle(node);
                    }
                    //removes any html or json titles, which are undesireable
                    var reg = new Regex(@"^(?:<!|\{.*\}$)");
                    //We then create the Data Holder and introduce it to the rest
                    if (isTitleValid(title) && !reg.IsMatch(title))
                    {
                        var content = new ImageDataHolder(new Uri(src),
                            _cleanWhiteSpace.Replace(_contentToReplace.Replace(title, ""), " "));
                        models.Add(content);
                    }
                }
            }
            //These tags could have a pdf that we can then steal!
            if (node.Name == "a")
            {
                //We take the link within the tag, think of it as the door to the home
                var href = node.GetAttributeValue("href", null);
                if (href == null)
                {
                    return;
                }
                //We see if it's a pdf, if so then we take the goods!
                if (href.Contains(".pdf"))
                {
                    var src = FormatSource(href);
                    //Just a title for the pdf, just a nice side to the pdf itself
                    if (string.IsNullOrEmpty(src))
                    {
                        return;
                    }
                    var title = RecursiveSpan(node);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = SearchForTitle(node);
                    }
                    if (isTitleValid(title))
                    {
                        //Aha! We've gotten out successfully! Throw the pdf with the rest of stash!
                        var content = new PdfDataHolder(new Uri(src), title);
                        models.Add(content);
                    }
                }
                //If theres any audio we can also snag that! It's a little less exciting but it's still a good find.
                if (href.Contains(".mp3") || href.Contains(".ogg") || href.Contains(".flac") || href.Contains(".wav") ||
                    href.Contains(".m4a"))
                {
                    var src = FormatSource(href);
                    //Let's get that audio's title
                    var title = RecursiveSpan(node);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = SearchForTitle(node);
                    }

                    //A good find! Put it in a Dataholder and with the rest of the stash!
                    if (!string.IsNullOrEmpty(src) && isTitleValid(title))
                    {
                        var content = new AudioDataHolder(new Uri(src), title);
                        models.Add(content);
                    }
                }
            }
            if (node.Name == "title")
            {
                //WE HAVE A TITLE BUT IDK WHERE TO PUT IT, I GUESS YOU CAN JUST STORE IT SOMEWHERE, I WAS GOING TO
                //PUT IT INTO A DATAHOLDER BUT THAT MAY BE uneccessary 
            }
        }

        /// <summary>
        /// This crawls through all of the children of a node and grabs all of the text that it finds
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string RecursiveSpan(HtmlNode node)
        {
            //We don't want any javascript for Christ's sake
            if (node.Name == "script")
            {
                return "";
            }
            Regex re = new Regex(@"[.#].*?\{");
            //If theres text we snag it and move along to each of the children to do the same
            var s = "";
            if (node.NodeType == HtmlNodeType.Text && !re.IsMatch(node.InnerText))
            {
                s += node.InnerText;
            }
            foreach (var child in node.ChildNodes)
            {
                // :o recursion
                s += RecursiveSpan(child) + "  ";
            }
            // Yay all of the text!
            return s;
        }

        /// <summary>
        /// This takes a url or url fragment and it makes it better for the Uri class
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static string FormatSource(string src)
        {
            try
            {
                var s = FormatSourcePrivate(src);
                if (s == null || !Uri.IsWellFormedUriString(s, UriKind.Absolute))
                {
                    return null;
                }
                //If this crashes one creation of a uri then it hasn't been formatted correctly and thus we return null
                var uri = new Uri(s);
                return s;
            }
            catch (Exception)
            {
                // :c
                return null;
            }
        }

        private static string FormatSourcePrivate(string src)
        {
            //Goes through a bunch of cases that could exist when dealing with urls and fixes them
            if (src == null)
            {
                return null;
            }
            if (src.StartsWith("//upload.wikimedia.org"))
            {
                Regex re = new Regex(@"thumb\/");
                src = re.Replace(src, "");
                re = new Regex(@"\/\d+px.*?$");
                src = re.Replace(src, "");
            }
            if (src.StartsWith("/~/"))
            {
                return "http:/" + src.Substring(2);
            }
            if (src.StartsWith("http://") || src.StartsWith("https://"))
            {
                return src;
            }
            if (src.StartsWith("//"))
            {
                return "http:" + src;
            }
            if (src.StartsWith("/wiki/"))
            {
                return "http://wikipedia.org" + src;
            }
            if (src.StartsWith("/"))
            {
                return "http:/" + src;
            }
            return "http://" + src;
        }


        /// <summary>
        /// This gets rid of all of the whitespace in a text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string StripText(string text)
        {
            //Just removing whitespace, don't mind me
            text = text.Trim();
            return text.Replace("\n", "").Replace("\t", "");
        }

        /// <summary>
        /// This recursively looks through nodes levels above it to try to find a caption
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string SearchForTitle(HtmlNode node)
        {
            //We keep check of the nodes we've visited so that we don't repeat nodes that we've aleady gone through (useful for huge documents)
            var visited = new HashSet<HtmlNode>();
            for (var i = 0; i < 4; i++) //depth to search set to 4
            {
                //If we hit the top node we can't keep going so we stop
                if (node == null)
                {
                    return null;
                }
                //This aint Christmas, we're only checking the list once
                visited.Add(node);
                // We must go further
                var title = RecursiveFindTitle(node, visited);
                //If we haven't found anything we keep going up
                if (title == null)
                {
                    node = node.ParentNode;
                }
                else
                {
                    return title;
                }
            }
            //no title :c
            return null;
        }

        /// <summary>
        /// This is the functional part of the title finder, it goes through each childnode and sees if there is a caption
        /// </summary>
        /// <param name="node"></param>
        /// <param name="visited"></param>
        /// <returns></returns>
        private string RecursiveFindTitle(HtmlNode node, HashSet<HtmlNode> visited)
        {
            // So we don't overlap ourselves
            visited.Add(node);
            //Sees if a title can be found in the current node
            var title = GetTitle(node);
            if (title != null)
            {
                return title;
            }
            //If theres no title then we keep looking down the generations
            if (node.ChildNodes.Count() > 10)
            {
                return null;
            }
            foreach (var child in node.ChildNodes)
            {
                if (child == null || visited.Contains(child)) continue;
                // :o recursion
                title = RecursiveFindTitle(child, visited);
                if (title != null)
                {
                    return title;
                }
            }
            // :c no title
            return null;
        }

        /// <summary>
        /// This sees if there is a valid caption the current node and if so then we return that
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetTitle(HtmlNode node)
        {
            if (!isTitleValid(node.InnerText)) return null;
            var text = node.InnerText;
            // Kinda arbitrary but if theres more than 325 characters then we can assume that it is much longer than a caption and is
            // instead a text block with information
            return text.Length > 325 ? null : StripText(text);
        }

        private static bool isTitleValid(string title)
        {
            return
                !(string.IsNullOrWhiteSpace(title) || (_titlesToRemove.IsMatch(title.ToLower().Trim()) && title.Length < 50) ||
                  _longTitlesToRemove.IsMatch(title.ToLower()));
        }

        private static bool isTextValid(string text)
        {
            return !(string.IsNullOrWhiteSpace(text) ||
                     _contentToRemove.IsMatch(text.ToLower().Trim()) && text.Length < 50 ||
                     _shortContentToRemove.IsMatch(text.ToLower().Trim()) && text.Length < 30);
        }

        /// <summary>
        /// Gets a url that is somewhere under the tree of node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string getUrl(HtmlNode node)
        {
            //The a tag is what stores the url so we check for these
            if (node.Name == "a")
            {
                return node.GetAttributeValue("href", null);
            }
            //Track all of the Urls that are in the tree
            var s = new List<string>();
            foreach (var child in node.ChildNodes)
            {
                //We recurse to find out
                var url = getUrl(child);
                if (string.IsNullOrEmpty(url)) continue;
                s.Add(url);
            }
            //This is more tailored towards Wikipedia, the last link generally has the actual url
            return s.Any() ? s.Last() : null;
        }

        public static async Task<List<List<DataHolder>>> RunWithSearch(string search)
        {

            var client = new HttpClient();
            //search += " wikipedia";
            // Add the subscription key to the request header
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "f3e590290fa54bf1865343c3bae6955c");
            string url = "https://api.cognitive.microsoft.com/bing/v5.0/search/?q=" + search +
                         "&count=25&offset=0&mkt=en-us&safesearch=Moderate";
            // Build the content of the post request

            //This is the request to the bing server
            var ret = await client.GetAsync(url);
            //We then have to read the string that is sent back
            var jsonString = await ret.Content.ReadAsStringAsync();
            //We then turn the json into our class structure 
            var json = JsonConvert.DeserializeObject<BingJson>(jsonString);
            //This takes only the usable urls for us to parse into
            var urls = json.webPages.value.Select(o => FormatSource(o.Url) ?? "").ToList();
            //This creates the models that we will then send back as a response 
            var models = new List<List<DataHolder>> {new List<DataHolder>()};
            int i = -1;
            //This is so that we can parse the data
            var htmlImporter = new HtmlImporter();
            var balance = 0;
            while (i < urls.Count - 1 && i < 4 + balance)
            {
                i++;
                bool isBad = false;
                //We check if the url is in the blacklist, if so we skip it
                foreach (var item in blacklist)
                {
                    if (json.webPages.value[i].displayUrl.Contains(item))
                    {
                        isBad = true;

                        break;
                    }
                        }
                        //We cannot parse pdfs or things on the blacklist
                        if (isBad || json.webPages.value[i].displayUrl.Contains(".pdf") || json.webPages.value[i].displayUrl.Contains(".ppt") || json.webPages.value[i].displayUrl.Contains(".doc"))
                        {
                            balance++;
                            continue;
                        }
                        var dataholders = await htmlImporter.Run(new Uri(urls[i]));
                        if (dataholders == null || dataholders.Count == 0)
                        {
                            balance++;
                            continue;
                        }
                        //Yay we have our models!
                        models.First().Add(new TextDataHolder(json.webPages.value[i].displayUrl, urls[i]));
                        models.Add(dataholders);
                    }
                    return models;
                }
            
        

        /// <summary>
        /// These classes are to deserialize the json that bing sends us back, just data containers
        /// </summary>
        private class
            BingJson
        {
            public BingPages webPages;
        }

        private class BingPages
        {
            public BingUrl[] value;
        }

        private class BingUrl
        {
            public string displayUrl;
            public string Url;
        }
    }
}

