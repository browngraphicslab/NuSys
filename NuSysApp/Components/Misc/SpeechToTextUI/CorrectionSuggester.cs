using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuSysApp.Misc.SpeechToTextUI
{

    public class CorrectionSuggester
    {
        // the first string in the tuple is the word in the sentence indexed by wordcount
        // the hashset of strings in the tuple is the list of alternative suggestions for that word
        private List<HashSet<string>> _correctionsByIndex;

        public CorrectionSuggester()
        {
            this._correctionsByIndex = new List<HashSet<string>>();
        }

        internal List<string> GetAlternatesByIndex(int index)
        {
            // error checking for index out of bounds exceptions
            if (index < 0 || index >= _correctionsByIndex.Count)
            {
                throw new ArgumentOutOfRangeException("The given index is out of bounds.");
            }

            return new List<string>(_correctionsByIndex[index]);
        }

        /// <summary>
        /// Returns a list of correctionSuggestions indexed by the index of their word in the 
        /// sentence.
        /// </summary>
        /// <param name="hypothesesList"></param>
        /// <returns></returns>
        internal List<HashSet<string>> GetAlternates(List<string> hypothesesList)
        {

            // throw error if hypotheses list is null or result is null
            if (hypothesesList == null)
            {
                throw new NullReferenceException("The hypothesesLsit or result argument was null");
            }

            // show that the speech helper is starting
            Debug.WriteLine("======== Speech Helper ========");

            // create a list of HashSets to store unique words by index
            List<HashSet<string>> alternateWordsByIndex = new List<HashSet<string>>();

            // for each hypothisis from the list of hypotheses
            for (var i = 0; i < hypothesesList.Count; i++)
            {

                // split the sentence into an array
                string[] hypothesisWords = hypothesesList[i].Split(' ');

                // take care of two words becoming one word in the next sentence
                // if the next sentence has fewer words than this one, combine the extra words
                // in the current sentence into a single string
                if (i + 1 < hypothesesList.Count) // make sure hypothesesList has at least i + 1 elements
                {
                    var nextSentenceWordCount = hypothesesList[i + 1].Split(' ').Length;
                    var currWordCount = hypothesisWords.Length;
                    if (nextSentenceWordCount < currWordCount)
                    {
                        // combines the extra words in hypothesisWords into a single string
                        hypothesisWords = hypothesesList[i].Split(new char[] { ' ' }, nextSentenceWordCount);
                    }
                }

                // TODO take care of one word becoming two words

                // for each word in the hypothesis, modify the appropriate set containing their alternate words
                for (int index = 0; index < hypothesisWords.Length; index++)
                {
                    if (index >= alternateWordsByIndex.Count) // create a new hashset
                    {
                        alternateWordsByIndex.Add(new HashSet<string>());
                    }
                    // add the word to the correctly indexed hashset
                    alternateWordsByIndex[index].Add(hypothesisWords[index]);
                }
            }

            // remove the final words from the list of alternates
            var finalSentence = hypothesesList[hypothesesList.Count - 1].Split(' ');
            for (int i = 0; i < finalSentence.Length; i++)
            {
                alternateWordsByIndex[i].Remove(finalSentence[i]);
            }

            // print the output
            // printerHelper(alternateWordsByIndex);
            return alternateWordsByIndex;
        }


        /// <summary>
        /// The list contains hashsets which correspond to words organized by index + some other
        /// minimal logic. the final words by index is the final sentence split into an array
        /// </summary>
        /// <param name="list"></param>
        /// <param name="finalWordsByIndex"></param>
        private static void printerHelper(List<HashSet<string>> list)
        {
            var index = 0;
            foreach (var wordSet in list)
            {
                var output = "{" + index + ": ";
                foreach (var word in wordSet)
                {
                    output = output + word + ", ";
                }
                output = output + "}";
                Debug.WriteLine(output);
                index++;
            }
        }

        
    }
}