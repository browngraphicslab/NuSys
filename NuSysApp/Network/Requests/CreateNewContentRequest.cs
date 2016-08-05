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
    /// this request is used to create a new content.  
    /// IT ALSO CREATES A NEW LIBRARY ELEMENT. 
    /// This should be used when uploading new things to nusys.  
    /// </summary>
    public class CreateNewContentRequest : Request
    {
        /// <summary>
        /// default constructor, for from-server deserilization
        /// </summary>
        /// <param name="message"></param>
        public CreateNewContentRequest(Message message) : base(NusysConstants.RequestType.CreateNewContentRequest,message){}

        /// <summary>
        /// This constructor should be used when sending this request to the server.  
        /// This constructor's message should be well populated.  
        /// Since this message also creates a new Library Element, all the keys of the CreateNewLibraryElementRequest should be used when adding properties to the future library element;
        /// For example, use NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY when adding a title to this new content.  
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentBase64String"></param>
        /// <param name="otherProperties"></param>
        public CreateNewContentRequest(NusysConstants.ContentType contentType, string contentBase64String, Message otherProperties) : base (NusysConstants.RequestType.CreateNewContentRequest, otherProperties)
        {
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = contentBase64String;
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = contentType.ToString();

            //if theres no content Id, add one
            if (!_message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY))
            {
                _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = SessionController.Instance.GenerateId();
            }
            
            // if the library element contentId is not set, set it to the contentId
            if (!_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY))
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY];
            }

            //make sure the new library element will have an id
            if (!_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY))
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = SessionController.Instance.GenerateId();
            }
        }

        /// <summary>
        /// this is the preferred constructor to use.  Create and populate the request args before adding.
        /// Check the arguments comments before popluating so you know which properties are required
        /// </summary>
        /// <param name="requestArgs"></param>
        public CreateNewContentRequest(CreateNewContentRequestArgs requestArgs)
            : base(NusysConstants.RequestType.CreateNewContentRequest)
        {
            //debug.asserts for required types
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ElementTypeToContentType(requestArgs.LibraryElementType);
        }

        /// <summary>
        /// This check will make sure that the minimum requirements are met before sending out the request.
        /// In other words, the associated handler server-side should have similar logic such that any reuqest that passes through this method
        /// will also pass through the server request handler and be successful.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY));

            //make sure that a file extension is present if needed
            switch (_message.GetEnum<NusysConstants.ContentType>(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY))
            {
                case NusysConstants.ContentType.Audio:
                case NusysConstants.ContentType.Image:
                case NusysConstants.ContentType.Video:
                    Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_FILE_EXTENTION));
                    break;
            }

            //assert the ids are present
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));

            //make sure the new library element will have an element type
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY));
        }
    }
}
