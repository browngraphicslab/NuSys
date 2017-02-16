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
    public class ContentDataController : IInkController
    {
        
        /// <summary>
        /// Event fired whenever a region is added to this content.
        /// The passed string is the libraryElementModelId of the newly added region.
        /// </summary>
        public event EventHandler<string> OnRegionAdded;


        /// <summary>
        /// event first whenever a region is removed.
        /// This will pass to you the libraryElementId of the recently removed region
        /// </summary>
        public event EventHandler<string> OnRegionRemoved;


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

        /// <summary>
        /// event fired whenever an ink stroke is added.
        /// The passed inkModel is the newly added stroke.
        /// </summary>
        public event EventHandler<InkModel> InkAdded;

        /// <summary>
        /// Event fired whenever an ink stroke is removed.
        /// The string argument is the id of the ink stroke
        /// </summary>
        public event EventHandler<string> InkRemoved;

        /// <summary>
        /// The list of strokes on the content data model.
        /// Forwards directly from ContentDataModel.Strokes
        /// </summary>
        public IEnumerable<InkModel> Strokes
        {
            get { return ContentDataModel?.Strokes; }
        } 

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
            ContentDataModel.SetData(data);
            ContentDataUpdated?.Invoke(this, data);

            //if we are not already updating from the server
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.AddLatestContent(data);
            }
        }

        /// <summary>
        /// method to be called when adding an ink stroke from the server.  
        /// This is the same as calling AddInk(InkModel) except that it doesn't send off a server request
        /// </summary>
        /// <param name="model"></param>
        public void AddInkFromServer(InkModel model)
        {
            _blockServerInteractionCount++;
            AddInk(model);
            _blockServerInteractionCount--;
        }

        /// <summary>
        /// method to add an ink model to this controller's contentDataModel.
        /// This will add the stroke to the model, fire an InkAdded event
        /// and then send a server request unless we're currently being updated from the server.
        /// If you want to add initial ink strokes and not send new server requests, try AddInitialInkStrokes.
        /// </summary>
        /// <param name="inkModel"></param>
        public void AddInk(InkModel inkModel)
        {
            if (inkModel?.ContentId != null && inkModel?.InkStrokeId != null && inkModel?.InkPoints?.Any() == true)
            {
                ContentDataModel.Strokes.Add(inkModel);
                InkAdded?.Invoke(this,inkModel);
                if (!_blockServerInteraction)
                {
                    SendCreateInkRequest(inkModel);
                }
            }
        }

        /// <summary>
        /// This method will add all the ink strokes to the model and fire events for each.
        /// It will not, however, make any new server calls.
        /// To clear the ink strokes before adding these new ones, set the clearStrokes bool to 'true';
        /// </summary>
        /// <param name="inkModels"></param>
        public void AddInitialInkStrokes(IEnumerable<InkModel> inkModels, bool clearStrokes = true)
        {
            if (clearStrokes)
            {
                ContentDataModel.Strokes.Clear();
            }

            //block server interaction
            _blockServerInteractionCount++;
            foreach (var ink in inkModels)
            {
                AddInk(ink);
            }
            //free up server interaction
            _blockServerInteractionCount--;
        }

        /// <summary>
        /// creates and sends a request to make an ink stroke from the given ink model
        /// </summary>
        /// <param name="inkModel"></param>
        private void SendCreateInkRequest(InkModel inkModel)
        {
            Task.Run(async delegate
            {
                var args = new CreateInkStrokeRequestArgs();
                args.ContentId = inkModel.ContentId;
                args.InkPoints = inkModel.InkPoints;
                args.InkStrokeId = inkModel.InkStrokeId;
                args.Color = new ColorModel
                {
                    A = inkModel.Color.A,
                    B = inkModel.Color.B,
                    G = inkModel.Color.G,
                    R = inkModel.Color.R
                };
                args.Thickness = inkModel.Thickness;

                var request = new CreateInkStrokeRequest(args);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                Debug.Assert(request.WasSuccessful() == true);
                //TODO alert user somehow if ink didn't save
            });
        }

        /// <summary>
        /// method to remove ink from a server request.
        /// This is the same as calling RemoveInk(id) but this prevents a server request from going out as well.
        /// </summary>
        /// <param name="strokeId"></param>
        public void RemoveInkFromServer(string strokeId)
        {
            _blockServerInteractionCount++;
            RemoveInk(strokeId);
            _blockServerInteractionCount--;
        }

        /// <summary>
        /// method to remove an ink stroke via its id.
        /// This will remove the stroke, fire the InkRemoved event, and send an server request.
        /// </summary>
        /// <param name="strokeId"></param>
        public void RemoveInk(string strokeId)
        {
            var stroke = ContentDataModel.Strokes.Where(s => s.InkStrokeId == strokeId).FirstOrDefault();
            if (stroke != null)
            {
                ContentDataModel.Strokes.Remove(stroke);
                InkRemoved?.Invoke(this,strokeId);
                if (!_blockServerInteraction)
                {
                    SendRemoveInkRequest(strokeId);
                }
            }
        }

        /// <summary>
        /// private method to send a server request to remove an ink stroke by id.
        /// pass in the ink id
        /// </summary>
        /// <param name="strokeId"></param>
        private void SendRemoveInkRequest(string strokeId)
        {
            Debug.Assert(ContentDataModel?.ContentId != null);
            Task.Run(async delegate
            {
                var request = new DeleteInkStrokeRequest(strokeId, ContentDataModel.ContentId);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            });
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
        /// Invokes OnRegionAdded Event which updates the RectangleWrapper's region views
        /// </summary>
        /// <param name="regionLibraryElementModelId"></param>
        public void AddRegion(string regionLibraryElementModelId)
        {
            OnRegionAdded?.Invoke(this,regionLibraryElementModelId);
        }

        /// <summary>
        /// Invokes OnRegionRemoved Event which updates the RectangleWrapper's region views
        /// </summary>
        /// <param name="regionLibraryElementModelId"></param>
        public void RemoveRegion(string regionLibraryElementModelId)
        {
            OnRegionRemoved?.Invoke(this,regionLibraryElementModelId);
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
            var dict = new Dictionary<string, int> { { ContentDataModel.ContentType.ToString(), 1}}; //add the content data type string

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
