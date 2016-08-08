using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateNewLibraryElementRequest : Request
    {
        public CreateNewLibraryElementRequest(Message m) : base(NusysConstants.RequestType.CreateNewLibraryElementRequest, m)
        {
        }

        /// <summary>
        /// DEPRECATED, SHOULD REMOVE
        /// pack the message here for creating a new library element. 
        /// In reality, this request should probably only be used for creating regions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        public CreateNewLibraryElementRequest(string id, string data, NusysConstants.ElementType type, string title = "")
            : base(NusysConstants.RequestType.CreateNewLibraryElementRequest)
        {
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = id;
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = data;
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = type.ToString();
            if (title != null)
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;
            }
        }

        /// <summary>
        /// preffered constructor for the CreateNewLibraryElementRequest.
        /// Takes in an arguments class.  Check the class properties to see which are required.  
        /// </summary>
        /// <returns></returns>
        public CreateNewLibraryElementRequest(CreateNewLibraryElementRequestArgs args) :  base(NusysConstants.RequestType.CreateNewLibraryElementRequest)
        {
            //debug.asserts for required types
            Debug.Assert(args.LibraryElementType != null);

            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = args.LibraryElementType.ToString();

            //set the default library element's content ID
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = args.ContentId ?? SessionController.Instance.GenerateId();

            //set the library element's library Id
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = args.LibraryElementId ?? SessionController.Instance.GenerateId();

            //set the keywords
            if (args.Keywords != null)
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY] = args.Keywords;
            }

            //set the title
            if (args.Title != null)
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = args.Title;
            }

            //set the favorited boolean
            if (args.Favorited != null)
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_FAVORITED_KEY] = args.Favorited.Value;
            }

            //TODO add in metadata
        }


        public override async Task CheckOutgoingRequest()
        {
            var time = DateTime.UtcNow.ToString();
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = time;
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LAST_EDITED_TIMESTAMP_KEY] = time;
        }
    }
}
