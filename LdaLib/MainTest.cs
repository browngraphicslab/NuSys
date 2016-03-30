using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace LdaLibrary
{
    public class TagExtractor
    {
        public static StorageFile WordmapFile;
        public static StorageFile Tassign;
        public static StorageFile Others;
        public static StorageFile Phi;
        public static StorageFile Twords;
        public static StorageFile Theta;
        public static Estimator estimator;

        public async static Task<List<string>> launch(List<string> args, List<string> documents)
        {
            await Init(args, documents);
            return ListWordsOfTopic();
        }

        public async static Task Init(List<string> args, List<string> documents )
        {

            var tmp = ApplicationData.Current.LocalFolder;

            WordmapFile = await CreateFileIfNotExists(tmp, "wordmap.txt");
            Tassign = await CreateFileIfNotExists(tmp, "tassign.txt");
            Others = await CreateFileIfNotExists(tmp, "others.txt");
            Phi = await CreateFileIfNotExists(tmp, "phi.txt");
            Twords = await CreateFileIfNotExists(tmp, "twords.txt");
            Theta = await CreateFileIfNotExists(tmp, "theta.txt");

            estimator = new Estimator();

            //string test = await GetWikiContent("The Beatles");
            /*
            if (true)
            {
                PDFReader test = new PDFReader();
                test.test(args[0]);
                return;
            }*/

            Option option = new Option();
            /*
            if (true) // just here for testing for now
            {
                DocumentReformatter reformatter = new DocumentReformatter();
                reformatter.reformat();
                return;
            }*/
            try
            {
                if (args.Count == 0)
                {
                    Debug.WriteLine("Please specify the document to be analyzed");
                    return;
                }
                if (args.Count > 0)
                {
                    // set default values  for args here
                    option.ntopics = 100;
                    option.alpha = 50/(double)option.ntopics; //if you do int/int you lose precision !! SO KEEP ONE OF THEM A DOUBLE
                    option.beta = .1;
                    option.niters = 2000;
                    option.savestep = 200;
                    option.twords = 0;
                    option.est = false;
                    option.estc = false;
                    option.inf = false;
                    option.dir = "";
                    option.dfile = "";
                    option.withrawdata = false;
                    option.wordmap = "wordmap.txt";
                    option.model = "";

                    // there is more than 1 argument, so we need to parse the input
                    for (int i = 1; i < args.Count; i++)
                    {
                        string[] currArg = args[i].Split(' '); // we assume that the arguments are coming in the form of "[parameter type] [parameter value]"
       

                        switch (currArg[0])
                        {
                            case "alpha":
                                //set assigned value
                                option.alpha = Convert.ToDouble(currArg[1]); 
                                break;
                            case "beta":
                                option.beta = Convert.ToDouble(currArg[1]); 
                                break;
                            case "dir":
                                //TODO: figure out how the directory is used
                                option.dir = currArg[1];
                                break;
                            case "est":
                                if (currArg[1] == "true")
                                {
                                    option.est = true;
                                } else if (currArg[1] == "false") {
                                    option.est = false;
                                }
                                else
                                {
                                    Debug.WriteLine("Error, please review argument formatting for -est");
                                    return;
                                }
                                break;
                            case "estc":
                                if (currArg[1] == "true")
                                {
                                    option.estc = true;
                                }
                                else if (currArg[1] == "false")
                                {
                                    option.estc = false;
                                }
                                else
                                {
                                    Debug.WriteLine("Error, please review argument formatting for -estc");
                                    return;
                                }
                                break;
                            case "inf":
                                if (currArg[1] == "true")
                                {
                                    option.inf = true;
                                }
                                else if (currArg[1] == "false")
                                {
                                    option.inf = false;
                                }
                                else
                                {
                                    Debug.WriteLine("Error, please review argument formatting for -inf");
                                    return;
                                }
                                break;
                            case "model":
                                //set new values...
                                option.model = currArg[1];
                                break;
                            case "niters":
                                option.niters = Convert.ToInt32(currArg[1]);
                                break;
                            case "ntopics":
                                option.ntopics = Convert.ToInt32(currArg[1]);
                                break;
                            case "savestep":
                                option.savestep = Convert.ToInt32(currArg[1]);
                                break;
                            case "twords":
                                option.twords = Convert.ToInt32(currArg[1]);
                                break;
                            case "withrawdata":
                                if (currArg[1] == "true")
                                {
                                    option.withrawdata = true;
                                } else if (currArg[1] == "false") {
                                    option.withrawdata = false;
                                }
                                else
                                {
                                    Debug.WriteLine("Error, please review argument formatting for -withrawdata");
                                    return;
                                }
                                break;
                            case "wordmap":
                                option.wordmap = currArg[1];
                                break;
                            default:
                                //for now, do nothing
                                break;
                        }
                    }
                }
                // now we deal with the first argument (dfile)
                option.dfile = args[0]; //remember that we assume the first argument is just the file we wish to analyze

                // from this point, we are done with parsing the arguments
                Debug.WriteLine("We have just finished parsing all of the arguments\n");
                Debug.WriteLine("this is the directory we will be looking at: " + option.dir);

                //List<String> wikiDoc = new List<String> { test };
                if (option.est || option.estc)
                {
                    Debug.WriteLine("made it into the if statement!!");
                    // now we need to make an estimator
                    Debug.WriteLine("if we make it here after the error, then it's caused by the estimator.");
                    await estimator.Init(option, documents); // so it's caused by the estimator
                    Debug.WriteLine("so it's caused by init?"); // for some reason we can get here if we just step in...........
                    await estimator.estimate();
                }
                else if (option.inf)
                {
                    /*
                    Inferencer inferencer = new Inferencer();
                    await inferencer.init(option);

                    Model newModel = await inferencer.inference();

                    for (int i = 0; i < newModel.phi.Length; ++i)
                    {
                        //phi: K*V
                        Debug.WriteLine("------------------------------\ntopic" + i + " : ");
                        for (int j = 0; j < 10; ++j)
                        {
                            Debug.WriteLine(inferencer.globalDict.id2word[j] + "\t" + newModel.phi[i,j]);
                        }
                    }
                    */
                }
                else
                {
                    Debug.WriteLine("made it into the else, ALL THE SAD FACES");
                }
            }
            //missing a catch block here. BUT DEY NOT IMPORTANT
            catch (Exception e)
            {
                Debug.WriteLine("Error in main: " + e.GetBaseException());
                return;
            }


        }

        public static List<string> ListWordsOfTopic()
        {
            return estimator.GetWordListOfTopic();
        }

        public async static Task<StorageFile> CreateFileIfNotExists(StorageFolder parent, string fileName)
        {
            return await parent.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).AsTask();
        }


        // also skipped a show help method! :D (Don't worry, it's unimportant).

        public async static Task<string> GetWikiContent(string topic)
        {
            string content;
            string formattedTopic = Regex.Replace(topic, @"\s+", "%20");
            /*string url = "https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles=" + formattedTopic */;
            string url = "https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&titles=" + formattedTopic;
            using (HttpClient client = new HttpClient())
            {
                content = await client.GetStringAsync(url);
            }
            string noHTML = Regex.Replace(content, @"<[^>]+>|&nbsp;", "").Trim();
            DieStopWords ds = new DieStopWords();
            string ret = await ds.removeStopWords(noHTML);
            return ret;
        }
    }
}
