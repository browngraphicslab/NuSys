using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LdaLibrary
{
    public class DieStopWords
    {   // actually ended up using this class to filter in general

        //  TIME TO KILL THE STOP WORDS >:D
        // we should also get rid of all of the periods DONE
        // now that i think about it, i hope it's not case sensitive... not done :( HA, DONE NOW! :D

        private HashSet<string> stopWords = new HashSet<string>(){"i", "getting", "going", "having", "result", "usually", "a", "about", "above", "above", "across", "after", "afterwards", "again", "against", "all", "almost", "alone", "along", "already", "also","although","always","am","among", "amongst", "amoungst", "amount",  "an", "and", "another", "any","anyhow","anyone","anything","anyway", "anywhere", "are", "around", "as",  "at", "back","be","became", "because","become","becomes", "becoming", "been", "before", "beforehand", "behind", "being", "below", "beside", "besides", "between", "beyond", "bill", "both", "bottom","but", "by", "call", "can", "cannot", "cant", "co", "con", "could", "couldnt", "cry", "de", "describe", "detail", "do", "does", "done", "don't", "down", "due", "during", "each", "e.g.", "eg", "eight", "either", "eleven","else", "elsewhere", "empty", "enough", "etc", "even", "ever", "every", "everyone", "everything", "everywhere", "except", "few", "fifteen", "fify", "fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", "four", "from", "front", "full", "further", "get", "give", "go", "had", "has", "hasnt", "have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", "indeed", "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", "latterly", "least", "less", "likes", "like", "liked", "ltd", "made", "many", "may", "me", "meanwhile", "might", "mill", "mine", "more", "moreover", "most", "mostly", "move", "much", "must", "my", "myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", "nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", "often", "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", "ours", "ourselves", "out", "over", "own","part", "per", "perhaps", "please", "put", "rather", "rather", "re", "same", "said", "says", "say", "see", "seem", "seemed", "seeming", "seems", "serious", "several", "she", "should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", "somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such", "system", "take", "ten", "than", "that", "the", "their", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", "they", "thickv", "thin", "third", "this", "those", "though", "three", "through", "throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", "twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via", "was", "we", "well", "were", "what", "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will", "with", "within", "without", "would", "yet", "you", "your", "yours", "yourself", "yourselves", "the", "goes", "likes", "does", "known", "really"};
       
        public async Task<string> removeStopWords(string line)
        {
            var finalString = "";
            line = line.ToLower();
            var noSpaces = line.Split(new String[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in noSpaces)
            {
                if (!stopWords.Contains(word))
                {
                    finalString += word + " ";
                }
            }

            // string cleaning
            finalString = finalString.TrimEnd(' ');
            finalString = Regex.Replace(finalString, "[’']", " "); // replace apostrophe with white space
            finalString = Regex.Replace(finalString, "\\p{P}+", ""); // remove punctuation
            finalString = Regex.Replace(finalString, @"\b\w{1,3}\b", ""); // remove words that have three letters or fewer
            finalString = Regex.Replace(finalString, @"\s+", " ");  // remove extra whitespace

            byte[] statement = System.Text.Encoding.UTF8.GetBytes(finalString);
            return finalString;
        }

    }

}