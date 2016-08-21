using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LDAUser;

namespace LDAuser
{
    public class Parser
    {
        public Parser()
        {

        }

        /// <summary>
        /// Get all the Wikipedia articles associated with the topics.</summary>
        /// <param name="topics">List of topics.</param>
        /// <returns>
        /// Dictionary of (k,v) = (topic, text) pairs.</returns>
        public async Task<Dictionary<string,string>> GetAllWikiContent(List<string> topics)
        {
            Dictionary<string, string> allText = new Dictionary<string, string>();
            Dictionary<string, string> parsedTopics = new Dictionary<string, string>();
            foreach (string t in topics)
            {
                string formattedTopic = Regex.Replace(t, @"\s+", "%20");
                List<string> topTopicPage = await this.GetTopPages(formattedTopic, 1);
                string s = await GetWikiContent(topTopicPage[0]);
                string introText = GetParsedContent(GetIntroText(s)).ToLower();
                allText.Add(t,GetParsedContent(s));
                //string tFormat = Regex.Replace(t, @"\s+", "%20");
                List<string> links = await GetWikiLink(t, "https://en.wikipedia.org/w/api.php?format=json&action=query&titles=" + topTopicPage[0] + "&prop=links&pllimit=max");
                if (links.Count > 0)
                {
                    foreach (string link in links)
                    {
                        if (!introText.Contains(link.ToLower()) || introText.Length < 50)
                        {
                            continue;
                        }
                        string linkText = await GetWikiContent(link);
                        linkText = GetParsedContent(linkText);
                        if (allText.ContainsKey(link) || allText.ContainsKey(link.ToLower()))
                        {
                            continue;
                        }
                        allText.Add(link.ToLower(),linkText);
                    }
                }
            }
            Debug.WriteLine("done");
            return allText;
        }

        /// <summary>
        /// Get the most relevant page of a given topic.</summary>
        /// <param name="topic">Topic.</param>
        /// <param name="numPages">Number of most relevant pages.</param>
        /// <returns>
        /// Title of Wikipedia article(s) most relevant to the topic.</returns>
        public async Task<List<string>> GetTopPages(string topic, int numPages)
        {
            string formattedTopic = Regex.Replace(topic, @"\s+", "%20");
            string url = "https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=" + formattedTopic + "&utf8=&format=json&srlimit=" + numPages;
            string page; List<string> pageList = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                page = await client.GetStringAsync(url);
            }
            //var queriedPage = "";//JObject.Parse(page);
            var jo = new int[5];//queriedPage["query"]["search"];
            if (jo.Count() == 0)
            {
                return pageList;
            }
            for (int i = 0; i < jo.Count(); i++)
            {
                //pageList.Add(jo[i]["title"].ToString());
            }
            return pageList;
        }

        /// <summary>
        /// Do frequency counting of each topic in all the relevant articles.</summary>
        /// <param name="allTopics">List of all potentially relevant topics.</param>
        /// <param name="allWords">List of all relevant articles.</param>
        /// <returns>
        /// Returns (k,v) = (topic, number of occurrence) pair.</returns>
        public Dictionary<string,double> GetTopicCount(Dictionary<string,string> allTopics, List<string> allWords)
        {
            Dictionary<string, double> topicCount = new Dictionary<string, double>();
            foreach (string k in allTopics.Keys)
            {
                int count = 0;
                string text = allTopics[k].ToLower();
                int total = text.Split().Count();
                foreach (string word in allWords)
                {
                    count = count + Regex.Matches(text, word.ToLower()).Count;
                }
                topicCount.Add(k, count);
            }
            return topicCount;
        }

        /// <summary>
        /// Get the introduction text of a Wikipedia article (e.g. first paragraph)</summary>
        /// <param name="content">Wikipedia article in HTML format.</param>
        /// <returns>
        /// Returns the introduction text.</returns>
        public string GetIntroText(string content)
        {
            if (!content.Contains("<h2>"))
                return "";
            string intro = content.Substring(0, content.IndexOf("<h2>"));
            return intro;
        }
        public async Task<string> GetWikiContent(string topic)
        {
            string content;
            string url = "https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&titles=" + topic;
            using (HttpClient client = new HttpClient())
            {
                content = await client.GetStringAsync(url);
            }
            return content;
        }

        /// <summary>
        /// Get plain text format of a Wikipedia article.</summary>
        /// <param name="content">Wikipedia article in HTML format.</param>
        /// <returns>
        /// Returns parsed content.</returns>
        public string GetParsedContent(string content)
        {
            string noHTML = Regex.Replace(content, @"<[^>]+>|&nbsp;", "").Trim();
            noHTML = Regex.Replace(noHTML, @"([^a-zA-z0-9]|\\n|\\)", " ");
            noHTML = Regex.Replace(noHTML, @"\s+", " ");
            string[] splitText = noHTML.Split(new string[] { "extract" }, StringSplitOptions.None);
            if (splitText.Length < 2)
            {
                return "";
            }
            return splitText[1];
        }

        /// <summary>
        /// Get all possible pairs from a list of strings.</summary>
        /// <param name="input">List of words.</param>
        /// <returns>
        /// List of word pairs.</returns>
        public List<string> GetWordPairs(List<string> input)
        {
            List<string> output = new List<string>();
            foreach (string s1 in input)
            {
                foreach (string s2 in input)
                {
                    if (s1 != s2)
                    {
                        output.Add(s1 + " " + s2);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Get all Wikipedia links in a Wikipedia article.</summary>
        /// <param name="page">Title of the Wikipedia page.</param>
        /// <param name="url">Callback link if the links are more than one page.</param>
        /// <returns>
        /// Returns the list of all links.</returns>
        public async Task<List<string>> GetWikiLink(string page, string url)
        {
            string linkPage, id;
            List<string> linkList = new List<string>();
            List<string> continuedLink = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                linkPage = await client.GetStringAsync(url);
                var jo = new int[5];//JObject.Parse(linkPage);
                var notFound = (-1).ToString();
                var test = 5; //jo["query"]["pages"];
                //if (jo.SelectToken("continue") != null)
                if(true)
                {
                    //string contUrl = "https://en.wikipedia.org/w/api.php?format=json&action=query&titles=" + page + "&prop=links&pllimit=max&plcontinue="+jo["continue"]["plcontinue"];
                    //continuedLink = await GetWikiLink(page, contUrl);
                }
                //if (test.SelectToken("-1") == null)
                if(true)
                {
                    //id = jo["query"]["pages"].First.First["pageid"].ToString();
                } else
                {
                    return linkList;
                }
                var links = new int[5];// jo["query"]["pages"][id]["links"];
                for (int i = 0; i < links.Count(); i++)
                {
                    //linkList.Add(links[i]["title"].ToString());
                }
            }
            return linkList.Concat(continuedLink).ToList();
        }

        /// <summary>
        /// Get all Wikipedia back links in a Wikipedia article.</summary>
        /// <param name="page">Title of the Wikipedia page.</param>
        /// <returns>
        /// Returns the list of all back links.</returns>
        public async Task<List<string>> GetBackLinks(string page)
        {
            List<string> backLinkList = new List<string>();
            string formattedPage = Regex.Replace(page, @"\s+", "_");
            string linkPage;
            string url = "https://en.wikipedia.org/w/api.php?format=json&action=query&list=backlinks&bltitle=" + formattedPage + "&blfilterredir=redirects&bllimit=max";
            using (HttpClient client = new HttpClient())
            {
                linkPage = await client.GetStringAsync(url);
                //var jo = JObject.Parse(linkPage);
                var test = new int[4];// jo["query"]["backlinks"];
                if (test.Count() == 0)
                {
                    return backLinkList;
                }
                for (int i = 0; i < test.Count(); i++)
                {
          
                    //backLinkList.Add(test[i]["title"].ToString());
                }
            }
            return backLinkList;
        }

        private class Link
        {
            public string title { get; set; }
            public string name { get; set; }
            public Link()
            {
            }
        }
    }
}
