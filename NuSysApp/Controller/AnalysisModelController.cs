using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    /// <summary>
    /// class that acts as a very simple controller for analysis models.
    /// Its only job is to extract tags
    /// </summary>
    public class AnalysisModelController
    {
        /// <summary>
        /// the analysis model that this controller encapsulates
        /// </summary>
        public AnalysisModel Model { get; private set; }

        /// <summary>
        /// the constructor just takes in the analysis model that this controller will encapsulate. 
        /// It can be null.
        /// </summary>
        /// <param name="model"></param>
        public AnalysisModelController(AnalysisModel model)
        {
            Model = model;
        }

        /// <summary>
        /// This virtual method can be used to get the suggested tags of this analysis model controller's model.  
        /// It will return a dictionary of string to int, with the string being the lowercased, suggested tag and the int being the weight it is given.
        /// A higher weight is more suggested.
        /// 
        /// This method takes in a bool that will indicate whether this method can make server calls.
        /// 
        /// To Merge two of these dictionaries together via adding their values for each key, use the linq statement:
        /// 
        /// To merge a INTO dict
        ///  a.ForEach(kvp => dict[kvp.Key] = (dict.ContainsKey(kvp.Key) ? dict[kvp.Key] : 0) + kvp.Value);
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, int>> GetSuggestedTagsAsync(bool makeServerCallsIfNeeded = true)
        {
            var dict = new Dictionary<string,int>();

            if (Model == null)
            {
                return dict;
            }

            if (Model.Type == NusysConstants.ContentType.PDF)//switch on the type.  Later, this should be put into inheritted controllers, not type switches
            {
                var analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(Model.ContentDataModelId) as NusysPdfAnalysisModel;
                analysisModel?.SuggestedTopics?.Select(s => s.ToLower().Replace("_", ""))?.Where(s => !string.IsNullOrEmpty(s))?.ForEach(s => dict[s] = (dict.ContainsKey(s) ? dict[s] : 0) + 3); //add the suggested topics with triple weight
                analysisModel?.DocumentAnalysisModel?.Segments?.SelectMany(s => s?.KeyPhrases?.Select(kp => kp?.ToLower()))?.Where(s => !string.IsNullOrEmpty(s))?.ForEach(s => dict[s] = (dict.ContainsKey(s) ? dict[s] : 0) + 2); // add key phrases with double weight
            }
            else if (Model.Type == NusysConstants.ContentType.Image)
            {
                var analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(Model.ContentDataModelId) as NusysImageAnalysisModel;
                analysisModel?.Categories?.Select(s => s?.Name?.ToLower().Replace("_",""))?.Where(s => !string.IsNullOrEmpty(s))?.ForEach(s => dict[s] = (dict.ContainsKey(s) ? dict[s] : 0) + 3);//add the categories
                analysisModel?.Tags?.Select(s => s?.Name?.ToLower().Replace("_", ""))?.Where(s => !string.IsNullOrEmpty(s))?.ForEach(s => dict[s] = (dict.ContainsKey(s) ? dict[s] : 0) + 3);//add the tags
            }
            return dict;
        }
    }
}
