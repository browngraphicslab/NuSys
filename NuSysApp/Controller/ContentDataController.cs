using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    /// <summary>
    /// Controller class for all contentDataModels.  
    /// This class will be instantiated for every ContentDataModel in the content Controller.  
    /// This class should allow for updating the data string of the content data model. 
    /// </summary>
    public class ContentDataController
    {
        /// <summary>
        /// the private bool used to indicate when the controller is being updated form the server. 
        /// It should be set true whenerver we are in the process of updating the controller from a server request.
        /// </summary>
        private bool _blockServerInteraction
        {
            get
            {
                return _blockServerInteractionCount != 0;
            }
        }

        /// <summary>
        /// count to represent how many unpacks are currently running.
        /// This is being used to replace the boolean. If this number is greater than 0, then an unpack is currently happening
        /// </summary>
        private int _blockServerInteractionCount;

        /// <summary>
        /// the event that will fire whenever the content data string changes.  
        /// This should only be getting fired for Text ContentDataModels.  
        /// The string is the new ContentData String.
        /// </summary>
        public event EventHandler<string> ContentDataUpdated;

        /// <summary>
        /// the public instance of the contentDataModel.
        /// This is the content data model that this controller will interact with.
        /// All the setters of the content data model data should be coming from this controller class.
        /// </summary>
        public ContentDataModel ContentDataModel { get; private set; }

        /// <summary>
        /// the protected debouncing dictionary that will be used to update the properties and content data string for this content data model;
        /// As of 8/23/16 should only be used to update the data string for the text node contents
        /// </summary>
        protected ContentDebouncingDictionary _debouncingDictionary;

        public delegate void InkAddEventHandler(InkModel inkModel);
        public delegate void InkRemoveEventHandler(string strokeId);
        public event InkAddEventHandler InkAdded;
        public event InkRemoveEventHandler InkRemoved;

        /// <summary>
        /// The constructor of the controller only takes in a Content Data Model.  
        /// </summary>
        /// <param name="contentDataModel"></param>
        public ContentDataController(ContentDataModel contentDataModel)
        {
            ContentDataModel = contentDataModel;
            Debug.Assert(contentDataModel?.ContentId != null);
            _debouncingDictionary = new ContentDebouncingDictionary(contentDataModel.ContentId, contentDataModel.ContentType);//instantiate the deboucning dictionary
        }
        

        /// <summary>
        /// this Method should be where all the updates of the content String go through.  
        /// </summary>
        /// <param name="data"></param>
        public void SetData(string data)
        {
            ContentDataModel.Data = data;
            ContentDataUpdated?.Invoke(this, data);

            //if we are not already updating from the server
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.AddLatestContent(data);
            }
        }

        public void AddInk(InkModel inkModel)
        {
            if (inkModel?.ContentId != null && inkModel?.InkStrokeId != null && inkModel?.InkPoints?.Any() == true)
            {
                ContentDataModel.Strokes.Add(inkModel);
                InkAdded?.Invoke(inkModel);
            }
        }

        public void RemoveInk(string strokeId)
        {
            var stroke = ContentDataModel.Strokes.Where(s => s.InkStrokeId == strokeId).First();
            ContentDataModel.Strokes.Remove(stroke);
            InkRemoved?.Invoke(strokeId);
        }

        /// <summary>
        /// this method serves the same purpose as the LibraryElementController's UnPack method.  
        /// This will be called when another client has changed the content data string.  
        /// If we ever have more properties that could change about contentDataModels, this should be changed to take in a Message class adn this should be a full 'UnPack' method
        /// </summary>
        /// <param name="newData"></param>
        public void UpdateFromServer(string newData)
        {
            _blockServerInteractionCount++;
            SetData(newData);
            _blockServerInteractionCount--;
        }

        /// <summary>
        /// This virtual method can be used to get the suggested tags of this content data controller's model.  
        /// It will return a dictionary of string to int, with the string being the lowercased, suggested tag and the int being the weight it is given.
        /// A higher weight is more suggested.
        /// 
        /// This method takes in a bool that will indicate whether this method can also mkae servre calls to get the most information possible.
        /// To make this method faster but MUCH MUCH less helpful, pass in false.
        /// 
        /// To Merge two of these dictionaries together via adding their values for each key, use the linq statement:
        /// 
        /// To merge a INTO dict
        ///  a.ForEach(kvp => dict[kvp.Key] = (dict.ContainsKey(kvp.Key) ? dict[kvp.Key] : 0) + kvp.Value);
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Dictionary<string, int>> GetSuggestedTagsAsync(bool makeServerCallsIfNeeded = true)
        {
            var dict = new Dictionary<string, int>() { { ContentDataModel.ContentType.ToString(), 1}}; //add the content data type string

            if (SessionController.Instance.ContentController.HasAnalysisModel(ContentDataModel.ContentId) || makeServerCallsIfNeeded)
            {
                if (!SessionController.Instance.ContentController.HasAnalysisModel(ContentDataModel.ContentId))
                {
                    await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(ContentDataModel.ContentId);
                }
                var analysisDict = await SessionController.Instance.ContentController.GetAnalysisModel(ContentDataModel.ContentId).GetSuggestedTagsAsync(makeServerCallsIfNeeded);
                analysisDict.ForEach(kvp => dict[kvp.Key] = (dict.ContainsKey(kvp.Key) ? dict[kvp.Key] : 0) + kvp.Value);
            }
            
            return dict;
        }
    }
}
