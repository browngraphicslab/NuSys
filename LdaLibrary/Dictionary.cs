using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;

namespace LdaLibrary
{
    // TO DO: Test the correctness of the read and write methods!!
    public class Dictionary
    {
        public IDictionary<string, int> word2id; // replace map with idictionary
        public IDictionary<int, string> id2word;

        //--------------------------------------------------
        // constructors
        //--------------------------------------------------

        public Dictionary()
        {
            word2id = new System.Collections.Generic.Dictionary<string, int>(); // change hashmap to dictionary
            id2word = new System.Collections.Generic.Dictionary<int, string>(); // may need to make a list of strings
        }

        //---------------------------------------------------
        // get/set methods
        //---------------------------------------------------

        public String getWord(int id)
        {
            return id2word[id];
        }

        public int getID(String word)
        {
            return word2id[word];
        }

        //----------------------------------------------------
        // checking methods
        //----------------------------------------------------
        /**
         * check if this dictionary contains a specified word
         */

        public bool contains(String word)
        {
            return word2id.ContainsKey(word);
        }

        public bool contains(int id)
        {
            return id2word.ContainsKey(id);
        }

        //---------------------------------------------------
        // manupulating methods
        //---------------------------------------------------
        /**
         * add a word into this dictionary
         * return the corresponding id
         */

        public int addWord(String word)
        {
            if (!contains(word))
            {
                int id = word2id.Count; // switch size() with Count

                word2id.Add(word, id); // switch put with add
                id2word.Add(id, word);

                return id;
            }
            else return getID(word);
        }

        // STILL NEED TO TEST THIS FUNCTION
        public async Task<Boolean> readWordMap(String wordMapFile)
        {
            try
            {
                // create reader and have it read the wordMapFile line by line
                System.IO.StreamReader reader = System.IO.File.OpenText(wordMapFile);
                // for each line
                string s = "";
                string line;
                s = await reader.ReadLineAsync();
                int nwords = int.Parse(s);

                for (int i = 0; i < nwords; ++i)
                {
                    line = await reader.ReadLineAsync();
                    // now we need to tokenize, so i need to first write one...
                    string[] split = line.Split(new String[] {" ", "\n", "\r\n", "\t"},
                        StringSplitOptions.RemoveEmptyEntries); // need to test this!!

                    if (split.Length != 2) continue;

                    String word = split[0];
                    String id = split[1];
                    int intID = int.Parse(id);

                    id2word.Add(intID, word);
                    word2id.Add(word, intID);
                }

                reader.Dispose();
                return true; //STILL NEED TO DOUBLE CHECK ALL OF THIS CODE ABOVE

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while reading dictionary:" + e.GetBaseException());
                return false;
            }
        }

        // STILL NEED TO TEST THIS METHOD AS WELL
        public async Task<bool> writeWordMap(String wordMapFile)
        {

            var lines = new List<string>();
            lines.Add(word2id.Count.ToString());

            // iterator is replaced with enumerator WHICH IS THEN REPLACED WITH A LOOP
            //IEnumerator<string> it = word2id.Keys.GetEnumerator(); // JUST LOOP :D :D :D

            // for the first case
            // string curr = it.Current; // does this take the first one?
            string key = null;
            string value = null;
            foreach (string curr in word2id.Keys)
            {
                key = curr;
                value = word2id[key].ToString();
                lines.Add(key + " " + value);
            }

            //await FileIO.WriteLinesAsync(TagExtractor.WordmapFile, lines);

            // the above code should work

            return true;
        }
    }
}