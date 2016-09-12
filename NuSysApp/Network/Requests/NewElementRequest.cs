using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;
using NuSysApp.Controller;

namespace NuSysApp
{
    /// <summary>
    /// the request that should be used to create all the elements (nodes) in nusys.
    /// Suggested usage is to create a NewElementRequestArgs class (or a subclass) and pass that in the constructor.  
    /// After a succesful reuqest has been executed and awaited, call AddReturnedElementToSession() to add the element locally.
    /// </summary>
    public class NewElementRequest : Request
    {
        /// <summary>
        ///  the required constructor which should only be used when deserializing from the server as a json.  
        /// </summary>
        /// <param name="message"></param>
        public NewElementRequest(Message message) : base(NusysConstants.RequestType.NewElementRequest, message)
        {
        }

        /// <summary>
        /// Preferred constructor.  
        /// Create and populate an args class.  
        /// Check the args properties to see what is and isn't required.
        /// This constructor will add the arg's properties to the message of the request.  
        /// </summary>
        /// <param name="args"></param>
        public NewElementRequest(NewElementRequestArgs args) : base(args, NusysConstants.RequestType.NewElementRequest) {}

        /// <summary>
        /// this checker just debug.asserts() the required keys.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            var m = _message;
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY));
        }


        /// <summary>
        /// If the request was successful this will get the returned model and add it to the Session.
        /// This will throw an exception if the request hasn't returned or wasn't successful.
        /// </summary>
        public void AddReturnedElementToSession()
        {
            CheckWasSuccessfull();
            //get and add the requested element model.
            var model = GetReturnedElementModel();
            Task.Run(async delegate {
                var success = await SessionController.Instance.AddElementAsync(model);
                Debug.Assert(success == true);
            });
        }

        /// <summary>
        /// Adds the returned elelementModel to the current session.  
        /// This will throw an exception if the request hasn't returned or wasn't successful. 
        /// This async method will not return until the element has succesfully been added or been rejected.
        /// Returns true if the element was added succesfully. 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddReturnedElementToSessionAsync()
        {
            CheckWasSuccessfull();
            //get and add the requested element model.
            var model = GetReturnedElementModel();
            var success = await SessionController.Instance.AddElementAsync(model);
            return success;
        }

        /// <summary>
        /// This will return the Request-returned elementModel if the request was sucessful.  
        /// </summary>
        /// <returns></returns>
        public ElementModel GetReturnedElementModel()
        {
            //call the base class's checking method
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));
            try
            {
                //get, deserialize, and return the model
                var model = ElementModelFactory.DeserializeFromString(_returnMessage.GetString(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));
                return model;
            }
            catch (JsonException e)
            {
                throw new Exception("The deserialization of an element model failed;");
            }
            
        }
       
        /// <summary>
        /// This execute reuqest function is now VERY SIMPLE. 
        /// It now gets the deserialized model from the message, and addits it to the session controller. 
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));
            var model = ElementModelFactory.DeserializeFromString(_message.GetString(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));

            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryId);

            //make sure the controller exists. If it doesnt exist this probably means that someone added a public library element
            //to a private workspace so you cant see it.
            if (libraryElementController == null)
            {
                return;//this could happen if we get a element that is private to someone else.  return and do nothing
            }
            Debug.Assert(libraryElementController != null); //make sure the controller exists

            if (!SessionController.Instance.CollectionIdsInUse.Contains(model.ParentCollectionId)) //if we don't need the collection that this element lives in
            {
                return;
            }

            if (libraryElementController.LibraryElementModel.Type == NusysConstants.ElementType.Collection && //if we have a collection
                !SessionController.Instance.CollectionIdsInUse.Contains(libraryElementController.LibraryElementModel.LibraryElementId))//and the content isn't loaded
            {//send a request to fetch the entire workspace
                var workspaceRequest = new GetEntireWorkspaceRequest(libraryElementController.LibraryElementModel.LibraryElementId);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(workspaceRequest);
                await workspaceRequest.AddReturnedDataToSessionAsync();
                await workspaceRequest.MakeCollectionFromReturnedElementsAsync();
            }

            var success = await SessionController.Instance.AddElementAsync(model);
            //Debug.Assert(success == true);
        }
    }
}
