using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public static class QueryArgsBuilder
    {

        /// <summary>
        /// Fuzzy matching list for keywords category
        /// </summary>
        private static List<string> matchKeywords = new List<string> { "keywords", "keyword" };
        /// <summary>
        /// Fuzzy matching list for metadata category
        /// </summary>
        private static List<string> matchMetaData = new List<string> { "metadata", "metadatas" };
        /// <summary>
        /// Fuzzy matching list for creator category
        /// </summary>
        private static List<string> matchCreator = new List<string> { "creator", "creators", "author", "authors" };
        /// <summary>
        /// Fuzzy matching list for element type category
        /// </summary>
        private static List<string> matchType = new List<string> { "type", "element", "elementtype" };
        /// <summary>
        /// Concatenated list of all the fuzzy matches
        /// </summary>
        private static List<string> matchesAll =new List<string>(matchMetaData.Concat(matchKeywords).Concat(matchCreator).Concat(matchType));

        /// <summary>
        /// Takes in a query text string, processes the text, and returns a new QueryArgs
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public static QueryArgs GetQueryArgs(string searchString)
        {
            QueryArgs args = new QueryArgs();

            // contains substrings that belong to categories, used to determine which text is stand alone or not
            HashSet<string> categoryText = new HashSet<string>();

            // set the search string in the args to the entire query passed in
            args.SearchString = searchString.ToLowerInvariant().Trim();

            // split the search string into segments by semicolons and iterate over all of them
            var splitBySemiColon = searchString.Split(':').ToList();
            for (int i = 0; i < splitBySemiColon.Count; i++)
            {
                var searchSegment = splitBySemiColon[i].ToLowerInvariant().Trim();

                // if a segment ends with a fuzzy match for a category then the next segment contains the args for that category
                if (ContainsKeywordFuzzyMatch(searchSegment) && i + 1 < splitBySemiColon.Count)
                {
                    args.Keywords.AddRange(BuildArgsList(splitBySemiColon[i + 1]));
                    // each time we add a substring to an argument list we also add the substring to the hashset of categoryText
                    categoryText.Add(RemoveFuzzyMatchFromEnd(splitBySemiColon[i + 1]));
                }
                else if (ContainsMetadataFuzzyMatch(searchSegment) && i + 1 < splitBySemiColon.Count)
                {
                    args.Metadata.AddRange(BuildArgsList(splitBySemiColon[i + 1]));
                    categoryText.Add(RemoveFuzzyMatchFromEnd(splitBySemiColon[i + 1]));
                }
                else if (ContainsTypeFuzzyMatch(searchSegment) && i + 1 < splitBySemiColon.Count)
                {
                    args.ElementTypes.AddRange(BuildArgsList(splitBySemiColon[i + 1]));
                    categoryText.Add(RemoveFuzzyMatchFromEnd(splitBySemiColon[i + 1]));
                }
                else if (ContainsCreatorFuzzyMatch(searchSegment) && i + 1 < splitBySemiColon.Count)
                {
                    args.CreatorUserIds.AddRange(BuildArgsList(splitBySemiColon[i + 1]));
                    categoryText.Add(RemoveFuzzyMatchFromEnd(splitBySemiColon[i + 1]));
                }

                // if substring is not associated with category text, then add it to the args searchText list
                var searchText = RemoveFuzzyMatchFromEnd(searchSegment);
                if (!string.IsNullOrEmpty(searchText) && !categoryText.Contains(searchText))
                {
                    searchText = searchText.ToLowerInvariant().Trim();                 
                    args.SearchText.AddRange(CleanSplitByCommaAndWhiteSpace(searchText));
                }
            }

            return args;
        }

        /// <summary>
        /// Builds a list of search strings for searching over. 
        /// Removes any fuzzy matches from the end of the string and splits the list by commas
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<string> BuildArgsList(string str)
        {
            str = RemoveFuzzyMatchFromEnd(str);
            return CleanSplitByComma(str);
        }

        /// <summary>
        /// Removes any fuzzy matches from the end of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RemoveFuzzyMatchFromEnd(string str)
        {
            str = str.ToLowerInvariant().Trim();

            foreach (var fuzzyMatch in matchesAll)
            {
                if (str.EndsWith(fuzzyMatch))
                {
                    var index = str.IndexOf(fuzzyMatch, StringComparison.Ordinal);
                    str = str.Remove(index, fuzzyMatch.Length);
                    break;
                }
            }
            return str.Trim();
        }

        /// <summary>
        /// Splits a string by commas and removes any empty or null strings
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<string> CleanSplitByComma(string str)
        {
            var splitBycommas = str.Split(',');
            return splitBycommas.Select(item => item.Trim()).Where(item => !string.IsNullOrEmpty(item)).ToList();
        }

        /// <summary>
        /// Splits a string by commas and whitespace and removes any empty or null strings
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<string> CleanSplitByCommaAndWhiteSpace(string str)
        {
            var splitByCommaAndWhiteSpace = str.Split(',', ' ');
            return splitByCommaAndWhiteSpace.Select(item => item.Trim()).Where(item => !string.IsNullOrEmpty(item)).ToList();
        }

        #region contains checker
        /// <summary>
        /// Returns true if a string contains a fuzzy match for keyword
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <returns></returns>
        private static bool ContainsKeywordFuzzyMatch(string searchSegment)
        {
            // see if a segment ends with a fuzzy match for keywords
            foreach (var keyword in matchKeywords)
            {
                // if a segment ends with a fuzzy match for keyword then the next segment must be the keywords section
                if (searchSegment.EndsWith(keyword))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns true if a string contains a fuzzy match for type
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <returns></returns>
        private static bool ContainsTypeFuzzyMatch(string searchSegment)
        {
            // see if a segment ends with a fuzzy match for keywords
            foreach (var type in matchType)
            {
                // if a segment ends with a fuzzy match for keyword then the next segment must be the keywords section
                if (searchSegment.EndsWith(type))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a string contains a fuzzy match for metadata
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <returns></returns>
        private static bool ContainsMetadataFuzzyMatch(string searchSegment)
        {
            // see if a segment ends with a fuzzy match for metadata
            foreach (var metadata in matchMetaData)
            {
                // if a segment ends with a fuzzy match for metadata then the next segment must be the metadata section
                if (searchSegment.EndsWith(metadata))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a string contains a fuzzy match for creator
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <returns></returns>
        private static bool ContainsCreatorFuzzyMatch(string searchSegment)
        {
            // see if a segment ends with a fuzzy match for creator
            foreach (var creator in matchCreator)
            {
                // if a segment ends with a fuzzy match for creator then the next segment must be the creator section
                if (searchSegment.EndsWith(creator))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion contains checker
    }
}
