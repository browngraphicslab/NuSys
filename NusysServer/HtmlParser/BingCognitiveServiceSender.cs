using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class BingCognitiveServiceSender
    {
        public static async Task<Dictionary<string,List<Keyword>>> RunTextAnalysis(List<DataHolder> input)
        {
            var textDataholders = input.Where(e => e is TextDataHolder);
            var docs = new List<CognitiveApiDocument>();
            foreach (var dh in textDataholders)
            {
                var doc = new CognitiveApiDocument(dh.LibraryElement.LibraryElementId,(dh as TextDataHolder)?.Text);
            }
            int i = 0;
            if (docs.Count == 0)
            {
                return null;
            }
            while (docs.Count <= 100)
            {
                docs.Add(new CognitiveApiDocument(docs[i].id+"1",docs[i].text));
                i++;
            }
            var results = await TextProcessor.GetTextTopicsAsync(docs);
            if (results == null)
            {
                return null;
            }
           
            var libraryElementIdsToKeywords = new Dictionary<string,List<Keyword>>();
            foreach (var a in results.operationProcessingResult.topicAssignments)
            {
                if (!libraryElementIdsToKeywords.ContainsKey(a.documentId))
                {
                    libraryElementIdsToKeywords.Add(a.documentId,new List<Keyword>());
                }
                var keyword = new Keyword(results.operationProcessingResult.topics.First(e => e.id==a.topicId).keyPhrase,Keyword.KeywordSource.TopicModeling);
                libraryElementIdsToKeywords[a.documentId].Add(keyword);
            }
            return libraryElementIdsToKeywords;
        }
    }
}