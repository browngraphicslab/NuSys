using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LdaLibrary
{
    public class LDADataset
    {
        //---------------------------------------------------------------
        // Instance Variables
        //---------------------------------------------------------------

        public Dictionary localDict;	// local dictionary	
        public Document[] docs; 		// a list of documents	
        public int M; 			 		// number of documents
        public int V;			 		// number of words

        // map from local coordinates (id) to global ones 
        // null if the global dictionary is not set
        public IDictionary<int, int> lid2gid;//switch map with idictionary

        //link to a global dictionary (optional), null for train data, not null for test data
        public Dictionary globalDict;

        //--------------------------------------------------------------
        // Constructor
        //--------------------------------------------------------------
        public LDADataset()
        {
            localDict = new Dictionary();
            M = 0;
            V = 0;
            docs = null;

            globalDict = null;
            lid2gid = null;
        }

        public LDADataset(int M)
        {
            localDict = new Dictionary();
            this.M = M;
            this.V = 0;
            docs = new Document[M];

            globalDict = null;
            lid2gid = null;
        }

        public LDADataset(int M, Dictionary globalDict)
        {
            localDict = new Dictionary();
            this.M = M;
            this.V = 0;
            docs = new Document[M];

            this.globalDict = globalDict;
            lid2gid = new System.Collections.Generic.Dictionary<int, int>(); // replace hashmap with dictionary
        }

        //-------------------------------------------------------------
        //Public Instance Methods
        //-------------------------------------------------------------
        /**
	    * set the document at the index idx if idx is greater than 0 and less than M
	    * @param doc document to be set
	    * @param idx index in the document array
	    */
        public void setDoc(Document doc, int idx)
        {
            if (0 <= idx && idx < M)
            {
                docs[idx] = doc;
            }
        }
        /**
         * set the document at the index idx if idx is greater than 0 and less than M
         * @param str string contains doc
         * @param idx index in the document array
         */
        public void setDoc(String str, int idx){
		if (0 <= idx && idx < M){
            char[] delimChars = {' ','\t', '\n'}; // should we get rid of periods as well?
			string [] words = str.Split(delimChars); //slightly modified this line
			
			System.Collections.Generic.List<int> ids = new System.Collections.Generic.List<int>(); // replace vector with arraylist
			
			foreach (String word in words){
				int _id = localDict.word2id.Count;
				
				if (localDict.contains(word))		
					_id = localDict.getID(word);
								
				if (globalDict != null){
					//get the global id					
					int id = globalDict.getID(word); // we're getting an error here because we have no removed stop words from the original document.
					//Debug.WriteLine(id);
					
					if (id != null){
						localDict.addWord(word);
						
						lid2gid.Add(_id, id);// changed put to Add
						ids.Add(_id);
					}
					else { //not in global dictionary
						//do nothing currently
					}
				}
				else {
					localDict.addWord(word);
					ids.Add(_id);
				}
			}
			
			Document doc = new Document(ids, str);
			docs[idx] = doc;
			V = localDict.word2id.Count; // size is replaced with Count	
		}
	}
        //---------------------------------------------------------------
        // I/O methods
        //---------------------------------------------------------------
        /**
         *  read a dataset from a stream, create new dictionary
         *  @return dataset if success and null otherwise
         */
        public async static Task<LDADataset> ReadDataSet(String filename){
            //try {
            // create our reader 
            // we can't read the filename. so what the hell is the filename!!
            LDADataset data = null;
            System.IO.StreamReader reader = System.IO.File.OpenText(filename);
            data = ReadDataSet(reader).Result;
            reader.Dispose();
            return data;
            //}
            /*catch (Exception e){
			    Debug.WriteLine("Read Dataset Error: " + e.GetBaseException());
                return null;
		    }*/
        }


	    /**
	     *  read a dataset from a stream, create new dictionary
	     *  @return dataset if success and null otherwise
	     */
        public async static Task<LDADataset> ReadDataSet(System.IO.StreamReader reader) { // keep in mind that System.IO.StreamReader replaces bufferedreader from java
            try {
                //read number of document
			    String line;
			    line = await reader.ReadLineAsync(); //readline is replaced with ReadLine
			    int M = int.Parse(line);
                DieStopWords deleter = new DieStopWords();
			    LDADataset data = new LDADataset(M);
			    for (int i = 0; i < M; ++i){
				    line = await reader.ReadLineAsync();
                    // here we can get rid of the periods in the line.
                    line = line.Replace(".","");//may need to add more values to trim in the future
                    // here I should implement stemming

                    // now to get rid of the stop words
                    line = await deleter.removeStopWords(line);
				    data.setDoc(line, i);
			    }
			
			    return data;
            } catch (Exception e) {
                Debug.WriteLine("Read Dataset Error: " + e.GetBaseException());
                return null;
            }
        }
	    /**
	     * read a dataset from a stream with respect to a specified dictionary
	     * @param reader stream from which we read dataset
	     * @param dict the dictionary
	     * @return dataset if success and null otherwise
	     */
	    public static LDADataset ReadDataSet(List<string> documents, Dictionary dict){
	        var data = new LDADataset(documents.Count, dict);
		    for (var index = 1; index <= documents.Count; index++)
		    {
		        var document = documents[index];
		        data.setDoc(document, index);
		    }
		    return data;
	    }
	
	    /**
	     * read a dataset from a string, create new dictionary
	     * @param str String from which we get the dataset, documents are seperated by newline character 
	     * @return dataset if success and null otherwise
	     */
	    public static LDADataset ReadDataSet(List<string> strs) {
		    LDADataset data = new LDADataset(strs.Count);
		
		    for (int i = 0 ; i < strs.Count; ++i){
			    data.setDoc(strs[i], i);
		    }
		    return data;
	    }
	
	    /**
	     * read a dataset from a string with respect to a specified dictionary
	     * @param str String from which we get the dataset, documents are seperated by newline character	
	     * @param dict the dictionary
	     * @return dataset if success and null otherwise
	     */
	    public static LDADataset ReadDataSet(String [] strs, Dictionary dict){
		    //Debug.WriteLine("readDataset...");
		    LDADataset data = new LDADataset(strs.Length, dict);
		
		    for (int i = 0 ; i < strs.Length; ++i){
			    //Debug.WriteLine("set doc " + i);
			    data.setDoc(strs[i], i);
		    }
		    return data;
	    }
    }
}

