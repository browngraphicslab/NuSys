using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace LdaLibrary
{
    public class Model
    {
        //---------------------------------------------------------------
        //	Class Variables
        //---------------------------------------------------------------

        public static String tassignSuffix;	//suffix for topic assignment file
        public static String thetaSuffix;		//suffix for theta (topic - document distribution) file
        public static String phiSuffix;		//suffix for phi file (topic - word distribution) file
        public static String othersSuffix; 	//suffix for containing other parameters
        public static String twordsSuffix;		//suffix for file containing words-per-topics

        //---------------------------------------------------------------
        //	Model Parameters and Variables
        //---------------------------------------------------------------

        public String wordMapFile; 		//file that contain word to id map
        public String trainlogFile; 	//training log file	

        public String dir;
        public String dfile;
        public String modelName;
        public int modelStatus; 		//see Constants class for status of model
        public LDADataset data;			// link to a dataset

        public int M; //dataset size (i.e., number of docs)
        public int V; //vocabulary size
        public int K; //number of topics
        public double alpha, beta; //LDA  hyperparameters
        public int niters; //number of Gibbs sampling iteration
        public int liter; //the iteration at which the model was saved	
        public int savestep; //saving period
        public int twords; //print out top words per each topic
        public int withrawdata;

        // Estimated/Inferenced parameters
        public double[,] theta; //theta: document - topic distributions, size M x K
        public double[,] phi; // phi: topic-word distributions, size K x V

        // Temp variables while sampling
        public System.Collections.Generic.List<System.Collections.Generic.List<int>> z; //topic assignments for words, size M x doc.size() //note: originally vector before arraylist
        protected internal int[,] nw; //nw[i][j]: number of instances of word/term i assigned to topic j, size V x K
        protected internal int[,] nd; //nd[i][j]: number of words in document i assigned to topic j, size M x K
        protected internal int[] nwsum; //nwsum[j]: total number of words assigned to topic j, size K
        protected internal int[] ndsum; //ndsum[i]: total number of words in document i, size M

        // temp variables for sampling
        protected internal double[] p;

        //---------------------------------------------------------------
        //	Constructors
        //---------------------------------------------------------------	
        public Model()
        {
            setDefaultValues();
            wordList = new List<string>();
        }

        /**
         * Set default values for variables
         */
        public void setDefaultValues()
        {
            wordMapFile = "wordmap.txt";
            trainlogFile = "trainlog.txt";
            tassignSuffix = ".tassign.txt";
            thetaSuffix = ".theta.txt";
            phiSuffix = ".phi.txt";
            othersSuffix = ".others.txt";
            twordsSuffix = ".twords.txt";

            dir = "./";
            dfile = "trndocs.dat"; // do we need to add .txt to the end of these files as well?
            modelName = "model-final";
            modelStatus = Constants.MODEL_STATUS_UNKNOWN;

            M = 0;
            V = 0;
            K = 100;
            alpha = 50.0 / K;
            beta = 0.1;
            niters = 2000;
            liter = 0;

            z = null;
            nw = null;
            nd = null;
            nwsum = null;
            ndsum = null;
            theta = null;
            phi = null;
        }


        //---------------------------------------------------------------
        //	I/O Methods
        //---------------------------------------------------------------
        /**
         * read other file to get parameters
         */
        protected async Task<bool> readOthersFile(String otherFile)
        {
            // open file <model>.others to read:
            try
            {
                // create our reader 
                System.IO.StreamReader reader = System.IO.File.OpenText(otherFile);
                String line;
                while ((line = await reader.ReadLineAsync()) != null) {
                    // now we need to tokenize, so i need to first write one...
                    string[] split = line.Split(new String[] { "=", "", " ", "\n", "\r\n", "\t" }, StringSplitOptions.RemoveEmptyEntries); // need to test this!!

                    int count = split.Length; // not sure if this is proper, should also test
                    if (count != 2) continue; // we should check if the length is consistently 3 (pretty sure this is safe though)

                    string optstr = split[0];
                    string optval = split[1];
                    
                    if (optstr.Equals("alpha"))
                    {
                        alpha = Double.Parse(optval);
                    }
                    else if (optstr.Equals("beta"))
                    {
                        beta = Double.Parse(optval);
                    }
                    else if (optstr.Equals("ntopics"))
                    {
                        K = int.Parse(optval);
                    }
                    else if (optstr.Equals("liter"))
                    {
                        liter = int.Parse(optval);
                    }
                    else if (optstr.Equals("nwords"))
                    {
                        V = int.Parse(optval);
                    }
                    else if (optstr.Equals("ndocs"))
                    {
                        M = int.Parse(optval);
                    }
                    else
                    {
                        // any more?
                    }

                }
                reader.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while reading other file: " + e.GetBaseException());
                return false;
            }
            return true;
        }

        protected async Task<bool> readTAssignFile(string tassignFile)
        {
            try
            {
                int i, j;
                // create our reader 
                System.IO.StreamReader reader = System.IO.File.OpenText(tassignFile);

                string line;
                // make z a list of lists which hold ints
                z = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();

                data = new LDADataset(M);
                data.V = V;

                for (i = 0; i < M; i++)
                {
                    z.Add(new System.Collections.Generic.List<int>()); // i dink dis is wite
                    line = await reader.ReadLineAsync();
                    // now we need to tokenize, so i need to first write one...
                    string[] split = line.Split(new String[] { " ","\t","\r\n","\n" }, StringSplitOptions.RemoveEmptyEntries);
                    //split = line.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries); // need to test this!!
                    int length = split.Length;

                    System.Collections.Generic.List<int> words = new System.Collections.Generic.List<int>();
                    System.Collections.Generic.List<int> topics = new System.Collections.Generic.List<int>();

                    for (j = 0; j < length; j++)
                    {
                        string token = split[j];
                        // now we need to tokenize, so i need to first write one...
                        string[] split2 = token.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries); // need to test this!!
                        if (split2.Length != 2)
                        {
                            Debug.WriteLine("Invalid word-topic assignment line\n");
                            return false;
                        }
                        words.Add(int.Parse(split2[0]));
                        topics.Add(int.Parse(split2[1]));

                    } // end for each topic assignment

                    //allocate and add new document to the corpus
                    Document doc = new Document(words);
                    data.setDoc(doc, i);

                    //assign values for z
                    for (j = 0; j < topics.Count; j++)
                    {
                        z[i].Add(topics[j]); // i think this words, but if we get errors, take a look here
                    }
                } // end for each doc

                reader.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while loading model: " + e.GetBaseException());
                return false;
            }
            return true;
        }

        /**
         * load saved model
         */

        public async Task<bool> loadModel()
        {
            bool temp;
            temp = await readOthersFile(dir + System.IO.Path.DirectorySeparatorChar + modelName + othersSuffix);
            if (!temp)
            {
                return false;
            }
            temp = await readTAssignFile(dir + System.IO.Path.DirectorySeparatorChar + modelName + tassignSuffix);
            if (!temp)
            {
                return false;
            }

            // read dictionary
            Dictionary dict = new Dictionary();
            temp = await dict.readWordMap(dir + System.IO.Path.DirectorySeparatorChar + wordMapFile);
            if (!temp)
            {
                return false;
            }

            data.localDict = dict;
            return true;
        }

        /**
         * Save word-topic assignments for this model
         */
        public async Task<bool> saveModelTAssign(string filename)
        {
            int i, j;

            var lines = new List<string>();
            string line = "";

            //write docs with topic assignments for words
            for (i = 0; i < data.M; i++)
            {
                for (j = 0; j < data.docs[i].length; ++j)
                {
                    line = line + data.docs[i].words[j] + ":" + z[i][j] + " ";
                    
                }
                lines.Add(line);
                //byte[] end = System.Text.Encoding.UTF8.GetBytes("\n");

                // await writer.WriteAsync(end,0,end.Length); // in the future, after it is runnable, see the length of "\n"
                //lines.Add("");
            }
            //await FileIO.WriteLinesAsync(TagExtractor.Tassign, lines);
 
            

            return true;
        }

        /**
         * save theta for the model (topic distribution)
         */

        public async Task<bool> saveModelTheta(string filename)
        {

            var lines = new List<string>();
            string line = "";
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < K; j++)
                {
                    line = line + theta[i, j] + " ";
                    
                }
                lines.Add(line);
            }
            //await FileIO.WriteLinesAsync(TagExtractor.Theta, lines);
            return true;
        }

        public async Task<bool> saveModelPhi(string filename)
        {
            var lines = new List<string>();
            string line = "";

            for (int i = 0; i < K; i++){
                for (int j = 0; j < V; j++) {
                    line = line + phi[i, j] + " ";
                }
                lines.Add(line);
            }

            //await FileIO.WriteLinesAsync(TagExtractor.Phi, lines);

            return true;
        }

        /**
         * save other information of the model
         */
        public async Task<bool> saveModelOthers(string filename)
        {
        
            var lines = new List<string>();

            lines.Add("alpha=" + alpha);
            lines.Add("beta=" + beta);
            lines.Add("ntopics=" + K);
            lines.Add("ndocs=" + M);
            lines.Add("nwords=" + V);
            lines.Add("liters=" + liter);

           // await FileIO.WriteLinesAsync(TagExtractor.Others, lines);
           

            return true;
        }

        /**
         * save model the most likely words for each of the topics
         */

        // MAKE SURE WE DOUBLE CHECK THIS METHOD LATER TO SEE IF IT ACTS THE SAME WAY PAIR DOES , WE WILL BE USING TUPLE INSTEAD
        public async Task<bool> saveModelTwords(string filename)
        {

            if (twords > V)
            {
                twords = V;
            }

            var lines = new List<string>();
            for (int k = 0; k < K; k++)
            {   //switched it from a list of pairs (the one we make) to a list of keyvaluepairs, cuz SCREW JAVA CRAP. SAY HELLO TO CEEEEEE SHARPEEEEE
                List<KeyValuePair<int,double>> wordsProbsList = new System.Collections.Generic.List<KeyValuePair<int,double>>();//using a generic list over an array list
                for (int w = 0; w < V; w++)
                {   // i want to try using a keyvaluepair now...
                    KeyValuePair<int,double> p = new KeyValuePair<int,double>(w, phi[k,w]);
                    wordsProbsList.Add(p);
                }// end for each word

                // print the topic
             
                lines.Add("Topic " + k + "th: ");
                //wordsProbsList.Sort(); // double  check to make sure the most probably words show up first in the end
                // Gives intuition to the above's solution if it is incorrect : System.Collections.Generic.ICollection<Pair>.(wordsProbsList);
                //wordsProbsList.OrderBy();
                List<KeyValuePair<int,double>> temp = wordsProbsList;
                wordsProbsList = (from kv in wordsProbsList orderby kv.Value descending select kv).ToList();
                //so we want to sort the list such that the highest probability is on top!!!
                //int freezer = 0;
                for (int i = 0; i < twords; i++)
                {
                    if (data.localDict.contains((int)wordsProbsList[i].Key))
                    {
                        string word = data.localDict.getWord((int)wordsProbsList[i].Key);
                        lines.Add("\t" + word + " " + wordsProbsList[i].Value);
                        wordList.Add(word);
                    }
                }
            } // end for each topic
            //await FileIO.WriteLinesAsync(TagExtractor.Twords, lines);

            return true;
        }

        public List<string> wordList { get; set; }
        /**
         * Save ALL DAH MODELZ
         */
        public async Task<bool> saveModel(string modelName)
        {
            bool temp;
            temp = await saveModelTAssign(dir + System.IO.Path.DirectorySeparatorChar + modelName + tassignSuffix + ".txt");
            if (!temp)
            {
                return false;
            }
            temp = await saveModelOthers(dir + System.IO.Path.DirectorySeparatorChar + modelName + othersSuffix + ".txt");
            if (!temp)
            {
                return false;
            }
            temp = await saveModelTheta(dir + System.IO.Path.DirectorySeparatorChar + modelName + thetaSuffix + ".txt");
            if (!temp)
            {
                return false;
            }
            temp = await saveModelPhi(dir + System.IO.Path.DirectorySeparatorChar + modelName + phiSuffix + ".txt");
            if (!temp)
            {
                return false;
            }

            if (twords > 0)
            {
                temp = await saveModelTwords(dir + System.IO.Path.DirectorySeparatorChar + modelName + twordsSuffix + ".txt");
                if (!temp)
                    return false;
            }
            return true;
        }



        //---------------------------------------------------------------
        //	Init Methods
        //---------------------------------------------------------------
        /**
         * initialize the model
         */
        protected bool init(Option option)
        {
            //be careful about the option's constants
           
            // i think that the option values are fine.
            modelName = option.model;
            K = option.ntopics;

            alpha = option.alpha;
            if (alpha < 0.0)
                alpha = 50.0 / K;

            if (option.beta >= 0)
                beta = option.beta;

            niters = option.niters;

            dir = option.dir;
            if (dir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))// switched e with E, SECOND EDIT: added tostring
                dir = dir.Substring(0, dir.Length - 1);// switched s with S

            dfile = option.dfile;
            twords = option.twords;
            wordMapFile = option.wordmap;

            return true;
        }

        /**
         * Init parameters for estimation
         */
        public async Task<bool> initNewModel(Option option, List<string> documents ){

		    if (!init(option))
			    return false;
		
		    int m, n, w, k;		
		    p = new double[K];
            
		    data = LDADataset.ReadDataSet(documents);
		    if (data == null){
			    Debug.WriteLine("Fail to read training data!\n");
			    return false;
		    }
		
		    //+ allocate memory and assign values for variables		
		    M = data.M;
		    V = data.V;
		    dir = option.dir;
		    savestep = option.savestep;
		
		    // K: from command line or default value
	        // alpha, beta: from command line or default values
	        // niters, savestep: from command line or default values

		    nw = new int[V,K];
		    for (w = 0; w < V; w++){
			    for (k = 0; k < K; k++){
				    nw[w,k] = 0;
			    }
		    }
		
		    nd = new int[M,K];
		    for (m = 0; m < M; m++){
			    for (k = 0; k < K; k++){
				    nd[m,k] = 0;
			    }
		    }
		
		    nwsum = new int[K];
		    for (k = 0; k < K; k++){
			    nwsum[k] = 0;
		    }
		
		    ndsum = new int[M];
		    for (m = 0; m < M; m++){
			    ndsum[m] = 0;
		    }
		
		    z = new System.Collections.Generic.List<List<int>>();//this has length M
		    for (m = 0; m < data.M; m++){
			    int N = data.docs[m].length;
                z.Add(new System.Collections.Generic.List<int>()); // i think this makes it proper! :D
			    z[m] = new System.Collections.Generic.List<int>(); // not sure if this line is necessary...
			
			    //initilize for z
                Random random = new Random();
			    for (n = 0; n < N; n++){
                    // so for some reason, we believe that the topic assignment of words is distributed evenly
				    int topic = (int)Math.Floor(random.NextDouble() * K);//should be the equivalent to java's math.random
				    z[m].Add(topic);//replace a with A
				
				    // number of instances of word assigned to topic j
				    nw[data.docs[m].words[n],topic] += 1;
				    // number of words in document i assigned to topic j
				    nd[m,topic] += 1;
				    // total number of words assigned to topic j
				    nwsum[topic] += 1;
			    }
			    // total number of words in document i
			    ndsum[m] = N;
		    }
		
            //need to reset these values
            //THIS WILL PROBABLY BLOW UP. Need to replace [][] with [,];
		    theta = new double[M,K];//originally new double[M]{K}		
		    phi = new double[K,V];//why do we not have to specify these values all the way up top?

		
		    return true;
        }


        /**
         * Init parameters for inference
         * @param newData DataSet for which we do inference
         */
        public async Task<bool> initNewModel(Option option, LDADataset newData, Model trnModel){
		    if (!init(option))
			    return false;
		
		    int m, n, w, k;
		
		    K = trnModel.K;
		    alpha = trnModel.alpha;
		    beta = trnModel.beta;		
		
		    p = new double[K];
		    Debug.WriteLine("K:" + K);
		
		    data = newData;
		
		    //+ allocate memory and assign values for variables		
		    M = data.M;
		    V = data.V;
		    dir = option.dir;
		    savestep = option.savestep;
		    Debug.WriteLine("M:" + M);
		    Debug.WriteLine("V:" + V);
		
		    // K: from command line or default value
	        // alpha, beta: from command line or default values
	        // niters, savestep: from command line or default values

		    nw = new int[V,K];
		    for (w = 0; w < V; w++){
			    for (k = 0; k < K; k++){
				    nw[w,k] = 0;
			    }
		    }
		
		    nd = new int[M,K];
		    for (m = 0; m < M; m++){
			    for (k = 0; k < K; k++){
				    nd[m,k] = 0;
			    }
		    }
		
		    nwsum = new int[K];
		    for (k = 0; k < K; k++){
			    nwsum[k] = 0;
		    }
		
		    ndsum = new int[M];
		    for (m = 0; m < M; m++){
			    ndsum[m] = 0;
		    }

            z = new System.Collections.Generic.List<List<int>>(); // switch with a list of size m
		    for (m = 0; m < data.M; m++){
			    int N = data.docs[m].length;
                z[m] = new System.Collections.Generic.List<int>(); // switch with a list
			
			    //initilize for z
                Random random = new Random();
			    for (n = 0; n < N; n++){
				    int topic = (int)Math.Floor((random.NextDouble() * K)); //hopefully casting this is okay
				    z[m].Add(topic);
				
				    // number of instances of word assigned to topic j
				    nw[data.docs[m].words[n],topic] += 1;
				    // number of words in document i assigned to topic j
				    nd[m,topic] += 1;
				    // total number of words assigned to topic j
				    nwsum[topic] += 1;
			    }
			    // total number of words in document i
			    ndsum[m] = N;
		    }
		
		    theta = new double[M,K];		
		    phi = new double[K,V];
		
		    return true;
	    }


        /**
         * Init parameters for inference
         * reading new dataset from file
         */
     


        /**
	 * init parameter for continue estimating or for later inference
	 */
        public async Task<bool> initEstimatedModel(Option option){
		    if (!init(option))
			    return false;
		
		    int m, n, w, k;
		
		    p = new double[K];

            bool temp = await loadModel();
            // load model, i.e., read z and trndata
            if (!temp){
			    Debug.WriteLine("Fail to load word-topic assignment file of the model!\n");
			    return false;
		    }
		
		    Debug.WriteLine("Model loaded:");
		    Debug.WriteLine("\talpha:" + alpha);
		    Debug.WriteLine("\tbeta:" + beta);
		    Debug.WriteLine("\tM:" + M);
		    Debug.WriteLine("\tV:" + V);		
		
		    nw = new int[V,K];
		    for (w = 0; w < V; w++){
			    for (k = 0; k < K; k++){
				    nw[w,k] = 0;
			    }
		    }
		
		    nd = new int[M,K];
		    for (m = 0; m < M; m++){
			    for (k = 0; k < K; k++){
				    nd[m,k] = 0;
			    }
		    }
		
		    nwsum = new int[K];
	        for (k = 0; k < K; k++) {
		    nwsum[k] = 0;
	        }
	    
	        ndsum = new int[M];
	        for (m = 0; m < M; m++) {
		    ndsum[m] = 0;
	        }
	    
	        for (m = 0; m < data.M; m++){
	    	    int N = data.docs[m].length;
	    	
	    	    // assign values for nw, nd, nwsum, and ndsum
	    	    for (n = 0; n < N; n++){
	    		    w = data.docs[m].words[n];
	    		    int topic = z[m][n]; //note: should return an int
	    		
	    		    // number of instances of word i assigned to topic j
	    		    nw[w,topic] += 1;
	    		    // number of words in document i assigned to topic j
	    		    nd[m,topic] += 1;
	    		    // total number of words assigned to topic j
	    		    nwsum[topic] += 1;	    		
	    	    }
	    	    // total number of words in document i
	    	    ndsum[m] = N;
	        }
	    
	        theta = new double[M,K];
	        phi = new double[K,V];
	        dir = option.dir;
		    savestep = option.savestep;
	    
		    return true;
        }
	

	}

}
