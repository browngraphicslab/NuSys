using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

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
        private bool _blockServerInteraction = false;

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
        /// The constructor of the controller only takes in a Content Data Model.  
        /// </summary>
        /// <param name="contentDataModel"></param>
        public ContentDataController(ContentDataModel contentDataModel)
        {
            ContentDataModel = contentDataModel;
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
                //update the server
                Task.Run(async delegate
                {
                    var args = new UpdateContentRequestArgs();
                    args.ContentId = ContentDataModel.ContentId;
                    args.ContentType = ContentDataModel.ContentType = ContentDataModel.ContentType;
                    args.UpdatedContent = data;

                    //create the update request
                    var updateRequest =  new UpdateContentRequest(args);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(updateRequest);
                    Debug.Assert(updateRequest.WasSuccessful() == true);
                });
            }
        }

        /// <summary>
        /// this method serves the same purpose as the LibraryElementController's UnPack method.  
        /// This will be called when another client has changed the content data string.  
        /// If we ever have more properties that could change about contentDataModels, this should be changed to take in a Message class adn this should be a full 'UnPack' method
        /// </summary>
        /// <param name="newData"></param>
        public void UpdateFromServer(string newData)
        {
            _blockServerInteraction = true;
            SetData(newData);
            _blockServerInteraction = false;
        }
    }
}
