using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/*using LAIR.Extensions;
using LAIR.Collections.Generic;*/
/*using LAIR.IO;*/

namespace WordNetUnivApp
{
    /// <summary>
    /// Provides access to the WordNet resource via two alternative methods, in-memory and disk-based. The latter is blazingly
    /// fast but also hugely inefficient in terms of memory consumption. The latter uses essentially zero memory but is slow
    /// because all searches have to be conducted on-disk.
    /// </summary>
    public class WordNetEngine
    {
        #region static members
        /// <summary>
        /// SynSet relations
        /// </summary>
        public enum SynSetRelation
        {
            None,
            AlsoSee,
            Antonym,
            Attribute,
            Cause,
            DerivationallyRelated,
            DerivedFromAdjective,
            Entailment,
            Hypernym,
            Hyponym,
            InstanceHypernym,
            InstanceHyponym,
            MemberHolonym,
            MemberMeronym,
            PartHolonym,
            ParticipleOfVerb,
            PartMeronym,
            Pertainym,
            RegionDomain,
            RegionDomainMember,
            SimilarTo,
            SubstanceHolonym,
            SubstanceMeronym,
            TopicDomain,
            TopicDomainMember,
            UsageDomain,
            UsageDomainMember,
            VerbGroup,
        }

        /// <summary>
        /// WordNet parts-of-speech
        /// </summary>
        public enum POS
        {
            None,
            Noun,
            Verb,
            Adjective,
            Adverb
        }

        /// <summary>
        /// Lexicographer file names
        /// </summary>
        public enum LexicographerFileName
        {
            None,
            AdjAll,
            AdjPert,
            AdvAll,
            NounTops,
            NounAct,
            NounAnimal,
            NounArtifact,
            NounAttribute,
            NounBody,
            NounCognition,
            NounCommunication,
            NounEvent,
            NounFeeling,
            NounFood,
            NounGroup,
            NounLocation,
            NounMotive,
            NounObject,
            NounPerson,
            NounPhenomenon,
            NounPlant,
            NounPossession,
            NounProcess,
            NounQuantity,
            NounRelation,
            NounShape,
            NounState,
            NounSubstance,
            NounTime,
            VerbBody,
            VerbChange,
            VerbCognition,
            VerbCommunication,
            VerbCompetition,
            VerbConsumption,
            VerbContact,
            VerbCreation,
            VerbEmotion,
            VerbMotion,
            VerbPerception,
            VerbPossession,
            VerbSocial,
            VerbStative,
            VerbWeather,
            AdjPpl
        }

        /// <summary>
        /// SynSet relation symbols that are available for each POS
        /// </summary>
        private static Dictionary<POS, Dictionary<string, SynSetRelation>> _posSymbolRelation;

        /// <summary>
        /// Static constructor
        /// </summary>
        static WordNetEngine()
        {
            _posSymbolRelation = new Dictionary<POS, Dictionary<string, SynSetRelation>>();

            // noun relations
            Dictionary<string, SynSetRelation> nounSymbolRelation = new Dictionary<string, SynSetRelation>();
            nounSymbolRelation.Add("!", SynSetRelation.Antonym);
            nounSymbolRelation.Add("@", SynSetRelation.Hypernym);
            nounSymbolRelation.Add("@i", SynSetRelation.InstanceHypernym);
            nounSymbolRelation.Add("~", SynSetRelation.Hyponym);
            nounSymbolRelation.Add("~i", SynSetRelation.InstanceHyponym);
            nounSymbolRelation.Add("#m", SynSetRelation.MemberHolonym);
            nounSymbolRelation.Add("#s", SynSetRelation.SubstanceHolonym);
            nounSymbolRelation.Add("#p", SynSetRelation.PartHolonym);
            nounSymbolRelation.Add("%m", SynSetRelation.MemberMeronym);
            nounSymbolRelation.Add("%s", SynSetRelation.SubstanceMeronym);
            nounSymbolRelation.Add("%p", SynSetRelation.PartMeronym);
            nounSymbolRelation.Add("=", SynSetRelation.Attribute);
            nounSymbolRelation.Add("+", SynSetRelation.DerivationallyRelated);
            nounSymbolRelation.Add(";c", SynSetRelation.TopicDomain);
            nounSymbolRelation.Add("-c", SynSetRelation.TopicDomainMember);
            nounSymbolRelation.Add(";r", SynSetRelation.RegionDomain);
            nounSymbolRelation.Add("-r", SynSetRelation.RegionDomainMember);
            nounSymbolRelation.Add(";u", SynSetRelation.UsageDomain);
            nounSymbolRelation.Add("-u", SynSetRelation.UsageDomainMember);
            nounSymbolRelation.Add(@"\", SynSetRelation.DerivedFromAdjective);  // appears in WordNet 3.1
            _posSymbolRelation.Add(POS.Noun, nounSymbolRelation);

            // verb relations
            Dictionary<string, SynSetRelation> verbSymbolRelation = new Dictionary<string, SynSetRelation>();
            verbSymbolRelation.Add("!", SynSetRelation.Antonym);
            verbSymbolRelation.Add("@", SynSetRelation.Hypernym);
            verbSymbolRelation.Add("~", SynSetRelation.Hyponym);
            verbSymbolRelation.Add("*", SynSetRelation.Entailment);
            verbSymbolRelation.Add(">", SynSetRelation.Cause);
            verbSymbolRelation.Add("^", SynSetRelation.AlsoSee);
            verbSymbolRelation.Add("$", SynSetRelation.VerbGroup);
            verbSymbolRelation.Add("+", SynSetRelation.DerivationallyRelated);
            verbSymbolRelation.Add(";c", SynSetRelation.TopicDomain);
            verbSymbolRelation.Add(";r", SynSetRelation.RegionDomain);
            verbSymbolRelation.Add(";u", SynSetRelation.UsageDomain);
            _posSymbolRelation.Add(POS.Verb, verbSymbolRelation);

            // adjective relations
            Dictionary<string, SynSetRelation> adjectiveSymbolRelation = new Dictionary<string, SynSetRelation>();
            adjectiveSymbolRelation.Add("!", SynSetRelation.Antonym);
            adjectiveSymbolRelation.Add("&", SynSetRelation.SimilarTo);
            adjectiveSymbolRelation.Add("<", SynSetRelation.ParticipleOfVerb);
            adjectiveSymbolRelation.Add(@"\", SynSetRelation.Pertainym);
            adjectiveSymbolRelation.Add("=", SynSetRelation.Attribute);
            adjectiveSymbolRelation.Add("^", SynSetRelation.AlsoSee);
            adjectiveSymbolRelation.Add(";c", SynSetRelation.TopicDomain);
            adjectiveSymbolRelation.Add(";r", SynSetRelation.RegionDomain);
            adjectiveSymbolRelation.Add(";u", SynSetRelation.UsageDomain);
            adjectiveSymbolRelation.Add("+", SynSetRelation.DerivationallyRelated);  // not in documentation
            _posSymbolRelation.Add(POS.Adjective, adjectiveSymbolRelation);

            // adverb relations
            Dictionary<string, SynSetRelation> adverbSymbolRelation = new Dictionary<string, SynSetRelation>();
            adverbSymbolRelation.Add("!", SynSetRelation.Antonym);
            adverbSymbolRelation.Add(@"\", SynSetRelation.DerivedFromAdjective);
            adverbSymbolRelation.Add(";c", SynSetRelation.TopicDomain);
            adverbSymbolRelation.Add(";r", SynSetRelation.RegionDomain);
            adverbSymbolRelation.Add(";u", SynSetRelation.UsageDomain);
            adverbSymbolRelation.Add("+", SynSetRelation.DerivationallyRelated);  // not in documentation
            _posSymbolRelation.Add(POS.Adverb, adverbSymbolRelation);
        }

        /// <summary>
        /// Gets the relation for a given POS and symbol
        /// </summary>
        /// <param name="pos">POS to get relation for</param>
        /// <param name="symbol">Symbol to get relation for</param>
        /// <returns>SynSet relation</returns>
        internal static SynSetRelation GetSynSetRelation(POS pos, string symbol)
        {
            return _posSymbolRelation[pos][symbol];
        }

        /// <summary>
        /// Gets the part-of-speech associated with a file path
        /// </summary>
        /// <param name="path">Path to get POS for</param>
        /// <returns>POS</returns>
        private static POS GetFilePOS(string path)
        {
            POS pos;
            string extension = Path.GetExtension(path).Trim('.');
            if (extension == "adj")
                pos = POS.Adjective;
            else if (extension == "adv")
                pos = POS.Adverb;
            else if (extension == "noun")
                pos = POS.Noun;
            else if (extension == "verb")
                pos = POS.Verb;
            else
                throw new Exception("Unrecognized data file extension:  " + extension);

            return pos;
        }

        /// <summary>
        /// Gets synset shells from a word index line. A synset shell is an instance of SynSet with only the POS and Offset
        /// members initialized. These members are enough to look up the full synset within the corresponding data file. This
        /// method is static to prevent inadvertent references to a current WordNetEngine, which should be passed via the 
        /// corresponding parameter.
        /// </summary>
        /// <param name="wordIndexLine">Word index line from which to get synset shells</param>
        /// <param name="pos">POS of the given index line</param>
        /// <param name="mostCommonSynSet">Returns the most common synset for the word</param>
        /// <param name="wordNetEngine">WordNetEngine to pass to the constructor of each synset shell</param>
        /// <returns>Synset shells for the given index line</returns>
        private static HashSet<SynSet> GetSynSetShells(string wordIndexLine, POS pos, out SynSet mostCommonSynSet, WordNetEngine wordNetEngine)
        {
            HashSet<SynSet> synsets = new HashSet<SynSet>();
            mostCommonSynSet = null;

            // get number of synsets
            string[] parts = wordIndexLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int numSynSets = int.Parse(parts[2]);

            // grab each synset shell, from last to first
            int firstOffsetIndex = parts.Length - numSynSets;
            for (int i = parts.Length - 1; i >= firstOffsetIndex; --i)
            {
                // create synset
                int offset = int.Parse(parts[i]);

                // add synset to collection                        
                SynSet synset = new SynSet(pos, offset, wordNetEngine);
                synsets.Add(synset);

                // if this is the last synset offset to get (since we grabbed them in reverse order), record it as the most common synset
                if (i == firstOffsetIndex)
                    mostCommonSynSet = synset;
            }

            if (mostCommonSynSet == null)
                throw new Exception("Failed to get most common synset");

            return synsets;
        }
        #endregion

        private string _wordNetDirectory;
        private bool _inMemory;
        //private Dictionary<POS, BinarySearchTextStream> _posIndexWordSearchStream;  // disk-based search streams to get words from the index files
        private Dictionary<POS, StreamReader> _posSynSetDataFile;                   // disk-based synset data files
        private Dictionary<POS, Dictionary<string, HashSet<SynSet>>> _posWordSynSets;   // in-memory pos-word synsets lookup
        private Dictionary<string, SynSet> _idSynset;                               // in-memory id-synset lookup where id is POS:Offset
        private Dictionary<POS, string> _posIndexWordSearchPath;
        /// <summary>
        /// Gets whether or not the data in this WordNetEngine is stored in memory
        /// </summary>
        public bool InMemory
        {
            get { return _inMemory; }
        }

        /// <summary>
        /// Gets the WordNet data distribution directory
        /// </summary>
        public string WordNetDirectory
        {
            get { return _wordNetDirectory; }
        }

        /// <summary>
        /// Gets all words in WordNet, organized by POS.
        /// </summary>

        public Dictionary<POS, HashSet<string>> AllWords
        {
            get
            {
                Dictionary<POS, HashSet<string>> posWords = new Dictionary<POS, HashSet<string>>();

                if (_inMemory)
                {
                    // grab words from in-memory index
                    foreach (POS pos in _posWordSynSets.Keys)
                        posWords.Add(pos, new HashSet<string>(_posWordSynSets[pos].Keys));
                }
                else
                {
                    /*foreach(POS pos in _posIndexWordSearchPath.Keys)
                    {

                    }*/

                    // read index file for each pos
                    /*foreach (POS pos in _posIndexWordSearchStream.Keys)
                    {
                        // reset index file to start
                        StreamReader indexFile = _posIndexWordSearchStream[pos].Stream;
                        //indexFile.SetPosition(0);
                        indexFile.DiscardBufferedData();
                        indexFile.BaseStream.Position = 0;
                        // read words, skipping header lines
                        HashSet<string> words = new HashSet<string>();
                        string line;
                        while ((line = indexFile.ReadLine()) != null)
                            if (!line.StartsWith(" "))
                                words.Add(line.Substring(0, line.IndexOf(' ')));

                        posWords.Add(pos, words);
                    }*/
                }

                return posWords;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wordNetDirectory">Path to WorNet directory (the one with the data and index files in it)</param>
        /// <param name="inMemory">Whether or not to store all data in memory. In-memory storage requires quite a bit of space
        /// but it is also very quick. The alternative (false) will cause the data to be searched on-disk with an efficient
        /// binary search algorithm.</param>
        public WordNetEngine(string wordNetDirectory, bool inMemory)
        {
            _wordNetDirectory = wordNetDirectory;
            _inMemory = inMemory;
            //_posIndexWordSearchStream = null;
            _posSynSetDataFile = null;

            if (!System.IO.Directory.Exists(_wordNetDirectory))
                throw new DirectoryNotFoundException("Non-existent WordNet directory:  " + _wordNetDirectory);

            // get data and index paths
            string[] dataPaths = new string[]
            {
                Path.Combine(_wordNetDirectory, "data.adj"),
                Path.Combine(_wordNetDirectory, "data.adv"),
                Path.Combine(_wordNetDirectory, "data.noun"),
                Path.Combine(_wordNetDirectory, "data.verb")
            };

            string[] indexPaths = new string[]
            {
                Path.Combine(_wordNetDirectory, "index.adj"),
                Path.Combine(_wordNetDirectory, "index.adv"),
                Path.Combine(_wordNetDirectory, "index.noun"),
                Path.Combine(_wordNetDirectory, "index.verb")
            };

            // make sure all files exist
            //foreach (string path in dataPaths.Union(indexPaths))
             //   if (!System.IO.File.Exists(path))
               //     throw new FileNotFoundException("Failed to find WordNet file:  " + path);

            #region index file sorting
            string sortFlagPath = Path.Combine(_wordNetDirectory, ".sorted_for_dot_net");
            if (!System.IO.File.Exists(sortFlagPath))
            {
                /* make sure the index files are sorted according to the current sort order. the index files in the
                 * wordnet distribution are sorted in the order needed for (presumably) the java api, which uses
                 * a different sort order than the .net runtime. thus, unless we resort the lines in the index 
                 * files, we won't be able to do a proper binary search over the data. */
                foreach (string indexPath in indexPaths)
                {
                    // create temporary file for sorted lines
                    string tempPath = Path.GetTempFileName();
                    FileStream fs = new FileStream(tempPath, FileMode.OpenOrCreate);
                    StreamWriter tempFile = new StreamWriter(fs);

                    // get number of words (lines) in file
                    int numWords = 0;
                    fs = new FileStream(indexPath, FileMode.Open);
                    StreamReader indexFile = new StreamReader(fs);
                    string line;
                    while ((line = indexFile.ReadLine()) != null)
                        //while (indexFile.TryReadLine(out line))
                        if (!line.StartsWith(" "))
                            ++numWords;

                    // get lines in file, sorted by first column (i.e., the word)
                    Dictionary<string, string> wordLine = new Dictionary<string, string>(numWords);
                    indexFile = new StreamReader(fs);

                    //while (indexFile.TryReadLine(out line))
                    while (indexFile.ReadLine() != null)
                        // write header lines to temp file immediately
                        if (line.StartsWith(" "))
                            tempFile.WriteLine(line);
                        else
                        {
                            // trim useless blank spaces from line and map line to first column
                            line = line.Trim();
                            wordLine.Add(line.Substring(0, line.IndexOf(' ')), line);
                        }

                    // get sorted words
                    List<string> sortedWords = new List<string>(wordLine.Count);
                    sortedWords.AddRange(wordLine.Keys);
                    sortedWords.Sort();

                    // write lines sorted by word
                    foreach (string word in sortedWords)
                        tempFile.WriteLine(wordLine[word]);

                    tempFile.Dispose();

                    // replace original index file with properly sorted one
                    System.IO.File.Delete(indexPath);
                    System.IO.File.Move(tempPath, indexPath);
                }

                // create flag file, indicating that we've sorted the data
                FileStream sort_fs = new FileStream(sortFlagPath, FileMode.OpenOrCreate);
                StreamWriter sortFlagFile = new StreamWriter(sort_fs);
                sortFlagFile.WriteLine("This file serves no purpose other than to indicate that the WordNet distribution data in the current directory has been sorted for use by the .NET API.");
                sortFlagFile.Dispose();
            }
            #endregion

            #region engine init
            if (inMemory)
            {
                // pass 1:  get total number of synsets
                int totalSynsets = 0;
                foreach (string dataPath in dataPaths)
                {
                    // scan synset data file for lines that don't start with a space...these are synset definition lines
                    StreamReader dataFile = new StreamReader(new FileStream(dataPath, FileMode.Open));
                    string line;
                    //while (dataFile.TryReadLine(out line))
                    while ((line = dataFile.ReadLine()) != null)
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                            ++totalSynsets;
                    }
                }

                // pass 2:  create synset shells (pos and offset only)
                _idSynset = new Dictionary<string, SynSet>(totalSynsets);
                foreach (string dataPath in dataPaths)
                {
                    POS pos = GetFilePOS(dataPath);

                    // scan synset data file
                    StreamReader dataFile = new StreamReader(new FileStream(dataPath, FileMode.Open));
                    string line;
                    //while (dataFile.TryReadLine(out line))
                    while ((line = dataFile.ReadLine()) != null)
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                        {
                            // get offset and create synset shell
                            int offset = int.Parse(line.Substring(0, firstSpace));
                            SynSet synset = new SynSet(pos, offset, null);

                            _idSynset.Add(synset.ID, synset);
                        }
                    }
                }

                // pass 3:  instantiate synsets (hooks up relations, set glosses, etc.)
                foreach (string dataPath in dataPaths)
                {
                    POS pos = GetFilePOS(dataPath);

                    // scan synset data file
                    StreamReader dataFile = new StreamReader(new FileStream(dataPath, FileMode.Open));
                    string line;
                    while ((line = dataFile.ReadLine()) != null)
                    //while (dataFile.TryReadLine(out line))
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                            // instantiate synset defined on current line, using the instantiated synsets for all references
                            _idSynset[pos + ":" + int.Parse(line.Substring(0, firstSpace))].Instantiate(line, _idSynset);
                    }
                }

                // organize synsets by pos and words...also set most common synset for word-pos pairs that have multiple synsets
                _posWordSynSets = new Dictionary<POS, Dictionary<string, HashSet<SynSet>>>();
                foreach (string indexPath in indexPaths)
                {
                    POS pos = GetFilePOS(indexPath);

                    //_posWordSynSets.EnsureContainsKey(pos, typeof(Dictionary<string, HashSet<SynSet>>));

                    // scan word index file, skipping header lines
                    StreamReader indexFile = new StreamReader(new FileStream(indexPath, FileMode.Open));
                    string line;
                    while ((line = indexFile.ReadLine()) != null)
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                        {
                            // grab word and synset shells, along with the most common synset
                            string word = line.Substring(0, firstSpace);
                            SynSet mostCommonSynSet;
                            HashSet<SynSet> synsets = GetSynSetShells(line, pos, out mostCommonSynSet, null);

                            // set flag on most common synset if it's ambiguous
                            if (synsets.Count > 1)
                                _idSynset[mostCommonSynSet.ID].SetAsMostCommonSynsetFor(word);

                            // use reference to the synsets that we instantiated in our three-pass routine above
                            _posWordSynSets[pos].Add(word, new HashSet<SynSet>());
                            foreach (SynSet synset in synsets)
                                _posWordSynSets[pos][word].Add(_idSynset[synset.ID]);
                        }
                    }
                }
            }
            else
            {
                _posIndexWordSearchPath = new Dictionary<POS, string>();
                foreach (string indexPath in indexPaths)
                {
                    _posIndexWordSearchPath.Add(GetFilePOS(indexPath), indexPath);
                }

                // open readers for synset data files
                _posSynSetDataFile = new Dictionary<POS, StreamReader>();
                foreach (string dataPath in dataPaths)
                    _posSynSetDataFile.Add(GetFilePOS(dataPath), new StreamReader(new FileStream(dataPath, FileMode.Open)));
            }
            #endregion
        }

        #region synset retrieval
        /// <summary>
        /// Gets a synset
        /// </summary>
        /// <param name="synsetID">ID of synset in the format returned by SynSet.ID (i.e., POS:Offset)</param>
        /// <returns>SynSet</returns>
        public SynSet GetSynSet(string synsetID)
        {
            SynSet synset;
            if (_inMemory)
                synset = _idSynset[synsetID];
            else
            {
                // get POS and offset
                int colonLoc = synsetID.IndexOf(':');
                POS pos = (POS)Enum.Parse(typeof(POS), synsetID.Substring(0, colonLoc));
                int offset = int.Parse(synsetID.Substring(colonLoc + 1));

                // create shell and then instantiate
                synset = new SynSet(pos, offset, this);
                synset.Instantiate();
            }

            return synset;
        }

        /// <summary>
        /// Gets all synsets for a word, optionally restricting the returned synsets to one or more parts of speech. This
        /// method does not perform any morphological analysis to match up the given word. It does, however, replace all 
        /// spaces with underscores and call String.ToLower to normalize case.
        /// </summary>
        /// <param name="word">Word to get SynSets for. This method will replace all spaces with underscores and
        /// call ToLower() to normalize the word's case.</param>
        /// <param name="posRestriction">POSs to search. Cannot contain POS.None. Will search all POSs if no restriction
        /// is given.</param>
        /// <returns>Set of SynSets that contain word</returns>
        public HashSet<SynSet> GetSynSets(string word, params POS[] posRestriction)
        {
            // use all POSs if none are supplied
            if (posRestriction == null || posRestriction.Length == 0)
                posRestriction = new POS[] { POS.Adjective, POS.Adverb, POS.Noun, POS.Verb };

            HashSet<POS> posSet = new HashSet<POS>(posRestriction);
            if (posSet.Contains(POS.None))
                throw new Exception("Invalid SynSet POS request:  " + POS.None);

            // all words are lower case and space-replaced
            word = word.ToLower().Replace(' ', '_');

            // gather synsets for each POS
            HashSet<SynSet> allSynsets = new HashSet<SynSet>();
            foreach (POS pos in posSet)
                if (_inMemory)
                {
                    // read instantiated synsets from memory
                    HashSet<SynSet> synsets;
                    if (_posWordSynSets[pos].TryGetValue(word, out synsets))
                        // optimization:  if there are no more parts of speech to check, we have all the synsets - so set the return collection and make it read-only. this is faster than calling AddRange on a set.
                        if (posSet.Count == 1)
                        {
                            allSynsets = synsets;
                            //allSynsets.IsReadOnly = true;
                        }
                        else
                            allSynsets.UnionWith(synsets);
                }
                else
                {
                    // get index line for word
                    long indexNo = FastSearch(word, _posIndexWordSearchPath[pos]);
                    string indexLine = ReadRecord(indexNo, _posIndexWordSearchPath[pos]);
                    
                    // if index line exists, get synset shells and instantiate them
                    if (indexLine != null)
                    {
                        // get synset shells and instantiate them
                        SynSet mostCommonSynset;
                        HashSet<SynSet> synsets = GetSynSetShells(indexLine, pos, out mostCommonSynset, this);
                        foreach (SynSet synset in synsets)
                        {
                            synset.Instantiate();
                            allSynsets.Add(synset);
                        }

                        // we only need to set this flag if there is more than one synset for the word-pos pair
                        if (synsets.Count > 1)
                            mostCommonSynset.SetAsMostCommonSynsetFor(word);
                    }
                }

            return allSynsets;
        }

        /// <summary>
        /// Gets the most common synset for a given word/pos pair. This is only available for memory-based
        /// engines (see constructor).
        /// </summary>
        /// <param name="word">Word to get SynSets for. This method will replace all spaces with underscores and
        /// will call String.ToLower to normalize case.</param>
        /// <param name="pos">Part of speech to find</param>
        /// <returns>Most common synset for given word/pos pair</returns>
        public SynSet GetMostCommonSynSet(string word, POS pos)
        {
            // all words are lower case and space-replaced...we need to do this here, even though it gets done in GetSynSets (we use it below)
            word = word.ToLower().Replace(' ', '_');

            // get synsets for word-pos pair
            HashSet<SynSet> synsets = GetSynSets(word, pos);

            // get most common synset
            SynSet mostCommon = null;
            if (synsets.Count == 1)
                return synsets.First();
            else if (synsets.Count > 1)
            {
                // one (and only one) of the synsets should be flagged as most common
                foreach (SynSet synset in synsets)
                    if (synset.IsMostCommonSynsetFor(word))
                        if (mostCommon == null)
                            mostCommon = synset;
                        else
                            throw new Exception("Multiple most common synsets found");

                if (mostCommon == null)
                    throw new NullReferenceException("Failed to find most common synset");
            }

            return mostCommon;
        }

        /// <summary>
        /// Gets definition line for synset from data file
        /// </summary>
        /// <param name="pos">POS to get definition for</param>
        /// <param name="offset">Offset into data file</param>
        internal string GetSynSetDefinition(POS pos, int offset)
        {
            // set data file to synset location
            StreamReader dataFile = _posSynSetDataFile[pos];
            dataFile.DiscardBufferedData();
            dataFile.BaseStream.Position = offset;

            // read synset definition
            string synSetDefinition = dataFile.ReadLine();

            // make sure file positions line up
            if (int.Parse(synSetDefinition.Substring(0, synSetDefinition.IndexOf(' '))) != offset)
                throw new Exception("Position mismatch:  passed " + offset + " and got definition line \"" + synSetDefinition + "\"");

            return synSetDefinition;
        }
        #endregion

        /// <summary>
        /// Closes all files and releases any resources assocated with this WordNet engine
        /// </summary>
        public void Close()
        {
            if (_inMemory)
            {
                // release all in-memory resources
                _posWordSynSets = null;
                _idSynset = null;
            }
            else
            {
                // close all data files
                foreach (StreamReader dataFile in _posSynSetDataFile.Values)
                    dataFile.Dispose();

                _posSynSetDataFile = null;
            }
        }

        #region FastSearch
        /// <summary>
        /// Searches the specified file for the specified keyword
        /// </summary>
        /// <param name="keyword">The keyword to find</param>
        /// <param name="dbFileName">The full path to the file to search in</param>
        /// <returns>The offset in the file at which the word was found; otherwise 0</returns>
        internal static long FastSearch(string keyword, string dbFileName)
        {
            long retVal = 0L;
            string key = string.Empty;
            Encoding enc = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(new FileStream(dbFileName, FileMode.Open), true))
            {
                enc = reader.CurrentEncoding;
                reader.Dispose();
            }

            using (FileStream fs = System.IO.File.OpenRead(dbFileName))
            {
                long diff = 666;
                string line = string.Empty;

                fs.Seek(0, SeekOrigin.End);
                long top = 0;
                long bottom = fs.Position;
                long mid = (bottom - top) / 2;

                do
                {
                    fs.Seek(mid - 1, SeekOrigin.Begin);
                    if (mid != 1)
                    {
                        while (fs.ReadByte() != '\n' && fs.Position < fs.Length)
                        {
                            retVal = fs.Position;
                        }
                    }

                    byte[] btData = new byte[1024];
                    int count = fs.Read(btData, 0, btData.Length);
                    fs.Seek(fs.Position - count, SeekOrigin.Begin);

                    string readData = enc.GetString(btData);
                    key = readData.Split(new char[] { ' ', '\n', '\r' })[0];

                    if (string.Compare(key, keyword) != 0)
                    {
                        if (string.Compare(key, keyword) < 0)
                        {
                            top = mid;
                            diff = (bottom - top) / 2;
                            mid = top + diff;
                        }

                        if (string.Compare(key, keyword) > 0)
                        {
                            bottom = mid;
                            diff = (bottom - top) / 2;
                            mid = top + diff;
                        }
                    }
                }
                while (string.Compare(key, keyword) != 0 && diff != 0);
            }

            if (string.Compare(key, keyword) != 0)
                retVal = 0L;
            else
                retVal++;

            return retVal;
        }
        #endregion FastSearch

        #region ReadIndex
        /// <summary>
        /// Reads the record at the specified offset from the specified file
        /// </summary>
        /// <param name="offset">The offset (record id) to read</param>
        /// <param name="dbFileName">The full path of the file to read from</param>
        /// <returns>The record as a string is successfull; otherwise an empty string</returns>
        internal static string ReadRecord(long offset, string dbFileName)
        {
            string retVal = string.Empty;

            using (StreamReader reader = new StreamReader(new FileStream(dbFileName, FileMode.Open), true))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                retVal = reader.ReadLine();
                reader.Dispose();
            }

            return retVal;
        }
        #endregion ReadIndex
    }
}