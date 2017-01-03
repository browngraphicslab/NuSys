using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Parser
{
    public class HTMLImporter
    {
        public HTMLImporter() {}

        public async Task<IEnumerable<DataHolder>> Run(Uri url)
        {
            var doc = await GetDocumentFromUri(url);
            if (doc == null)
            {
                return null;
            }

            var models = new HashSet<DataHolder>();
            await RecursiveAdd(doc.DocumentNode, models);
            foreach(var paragraphs in paragraphCollections)
            {
                var text = "";
                var title = "";
                var hasTitle = false;
                foreach(var node in paragraphs)
                {
                    if (!hasTitle)
                    {
                        title = RecursiveSpan(node);
                        hasTitle = true;
                        continue;
                    }
                        text += RecursiveSpan(node) + "\n";
                }
                if (text == "")
                    continue;
                    var content = new TextDataHolder(text,title??"");
                    models.Add(content);
            }
            return models;
        }
        private async Task<HtmlDocument> GetDocumentFromUri(Uri url)
        {
            try
            {
                url = url ?? new Uri("https://en.wikipedia.org/wiki/Computer_science");
                var doc = new HtmlDocument();
                var webRequest = HttpWebRequest.Create(url.AbsoluteUri);
                HttpWebResponse response = (HttpWebResponse) (await webRequest.GetResponseAsync());
                Stream stream = response.GetResponseStream();
                doc.Load(stream);
                stream.Dispose();
                return doc;
            }
            catch (Exception e)
            {
                Debug.Write("couldn't get document");
                return null;
            }
        }

        private HashSet<HashSet<HtmlNode>> paragraphCollections = new HashSet<HashSet<HtmlNode>>();
        private async Task RecursiveAdd(HtmlNode node, HashSet<DataHolder> models,  bool ignoreTexts = false)
        {
            if (node.Name.ToLower() == "script")
            {
                return;
            }
            Regex reh = new Regex(@"^(?:h\d|strong)");//Matches if there is a header tag (h1,h2...) or a bold tag and uses that as a title
            if (!ignoreTexts && reh.IsMatch(node.Name) && !RecursiveSpan(node).ToLower().Contains("credit"))
            {
                paragraphCollections.Add(new HashSet<HtmlNode>());
                paragraphCollections.Last().Add(node);
            }
            if (!ignoreTexts && (node.Name == "p" && paragraphCollections.Any()))//node.ChildNodes.Count(c => c.NodeType == HtmlNodeType.Text) > node.ChildNodes.Count() / 2)
            {
                paragraphCollections.Last().Add(node);
               // ignoreTexts = true;
            }
            foreach (var child in node.ChildNodes)
            {
                if (child.Name.ToLower() != "script")
                {
                    await RecursiveAdd(child, models, ignoreTexts);
                }
            }
            if (node.Name == "video")
            {
                var uristring = FormatSource(node.GetAttributeValue("src", null));
                if (uristring != null && uristring != "")
                {
                    var uri = new Uri(uristring);
                    var title = node.GetAttributeValue("alt", null);
                    if (title == null || title == "")
                    {
                        title = SearchForTitle(node);
                    }
                    if (uri != null)
                    {
                        var content = new VideoDataHolder(uri, title);
                        models.Add(content);
                    }
                }
            }
            if (node.Name == "img")
            {
                var src = FormatSource(node.GetAttributeValue("src", null));
                Regex re = new Regex(@"svg$");
                if (src == null || re.IsMatch(src)) 
                {
                    return;
                }
                if (src !=null && (src.Contains(".mp4") || src.Contains(".webm") || src.Contains(".ogv") || src.Contains(".ogg")))
                {
                    //TODO Implement Video Code
                    return;
                }
                var height = node.GetAttributeValue("height", 1000);
                var width = node.GetAttributeValue("width", 1000);
                if (height > 75 && width > 75)
                {
                    var title = node.GetAttributeValue("alt",null);
                    if (title == null || title == "")
                    {
                        title = SearchForTitle(node);
                    }

                    var content = new ImageDataHolder(new Uri(src), title);
                    models.Add(content);
                }
            }
            if (node.Name == "a")
            {
                var href = node.GetAttributeValue("href", null);
                if (href == null)
                {
                    return;
                }
                if (href.Contains(".pdf"))
                {
                    var src = FormatSource(href);
                    var title = RecursiveSpan(node);
                    if (title == null || title == "")
                    {
                        title = SearchForTitle(node);
                    }

                    var content = new PdfDataHolder(new Uri(src), title);
                    models.Add(content);
                }
                if (href.Contains(".mp3") || href.Contains(".ogg") ||href.Contains(".flac") ||href.Contains(".wav") ||href.Contains(".m4a"))
                {
                    var src = FormatSource(href);
                    var title = RecursiveSpan(node);
                    if (title == null || title == "")
                    {
                        title = SearchForTitle(node);
                    }

                    var content = new AudioDataHolder(new Uri(src), title);
                    models.Add(content);
                }
            }
        }


        private string RecursiveSpan(HtmlNode node)
        {
            var s = "";
            if ( node.NodeType == HtmlNodeType.Text)
            {
                s += node.InnerText;
            }
            foreach (var child in node.ChildNodes)
            {
                s += RecursiveSpan(child) + "  ";
            }
            return s;
        }

        private bool IsJS(string text)
        {
            var list = text.ToCharArray().ToList();
            if (list.Count(c => c == '{' || c == '}' || c == '$' || c == '&' || c == '.' || c == ';' || c == '!' || c == '=') > list.Count*.025)
            {
                return true;;
            }
            return false;
        }
        private string FormatSource(string src)
        {
            try
            {
                var s = FormatSourcePrivate(src);
                var uri = new Uri(s);
                return s;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private string FormatSourcePrivate(string src)
        {
            if (src == null)
            {
                return null;
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
            if (src.StartsWith("/"))
            {
                return "http:/" + src;
            }
            return "http://" + src;
        }

        private bool IsValidText(string text)
        {
            return text.Length > 50;
        }

        private string StripText(string text)
        {
            while (text.StartsWith(" "))
            {
                text = text.Substring(1);
            }
            while (text.EndsWith(" "))
            {
                text = text.Remove(text.Length - 1);
            }
            return text.Replace("\n","").Replace("\t","");
        }
        private string SearchForHeader(HtmlNode node)
        {
            var visited = new HashSet<HtmlNode>(); 
            for (int i = 0; i < 4; i++) //depth to search set to 4
            {
                if(node == null)
                {
                    return null;
                }
                visited.Add(node);
                var title = RecursiveFindHeader(node, visited);
                if (title == null)
                {
                    node = node.ParentNode;
                }
                else
                {
                    return title;
                }
            }
            return null;
        }
        private string SearchForTitle(HtmlNode node)
        {
            var visited = new HashSet<HtmlNode>(); 
            for (int i = 0; i < 4; i++) //depth to search set to 4
            {
                if(node == null)
                {
                    return null;
                }
                visited.Add(node);
                var title = RecursiveFindTitle(node, visited);
                if (title == null)
                {
                    node = node.ParentNode;
                }
                else
                {
                    return title;
                }
            }
            return null;
        }

        private string RecursiveFindHeader(HtmlNode node, HashSet<HtmlNode> visited)
        {
            visited.Add(node);
            Regex re = new Regex(@"^h\d");
            if (re.IsMatch(node.Name))
            {
                return RecursiveSpan(node);
            }
            /*if (node.ChildNodes.Count() > 10)
            {
                return null;
            }*/
            foreach (var child in node.ChildNodes)
            {
                if (child != null && !visited.Contains(child))
                {
                    var title = RecursiveFindHeader(child, visited);
                    if (title != null)
                    {
                        return title;
                    }
                }
            }
            return null;
        }
        private string RecursiveFindTitle(HtmlNode node, HashSet<HtmlNode> visited)
        {
            visited.Add(node);
            var title = GetTitle(node);
            if (title != null)
            {
                return title;
            }
            if (node.ChildNodes.Count() > 10)
            {
                return null;
            }
            foreach (var child in node.ChildNodes)
            {
                if (child != null && !visited.Contains(child))
                {
                    title = RecursiveFindTitle(child, visited);
                    if (title != null)
                    {
                        return title;
                    }
                }
            }
            return null;
        }

        private string GetTitle(HtmlNode node)
        {
            if (IsValidText(node.InnerText))
            {
                var text = node.InnerText;
                if (text.Length > 325)
                {
                    return null;
                }
                return StripText(text);
            }
            return null;
        }
    }
}
