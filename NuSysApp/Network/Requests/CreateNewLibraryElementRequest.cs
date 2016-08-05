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
        /// pack the message here for creating a new library element. 
        /// In readlity, this request should probably only be used for creating regions
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
        public override async Task CheckOutgoingRequest()
        {
            var time = DateTime.UtcNow.ToString();
            _message["library_element_creation_timestamp"] = time;
            _message["library_element_last_edited_timestamp"] = time;
            string url = null;
            if (_message.ContainsKey("server_url"))
            {
                url = _message["server_url"].ToString();
            }

            NusysConstants.ElementType type = (NusysConstants.ElementType) Enum.Parse(typeof(NusysConstants.ElementType), (string) _message["type"], true);
            /*
            //LibraryElementModel libraryElement = SessionController.Instance.ContentController.CreateAndAddModelFromMessage(new Message(_message.GetSerialized()));
            if (libraryElement != null && false)
            {
                Debug.Fail("just wanted to make sure this code isn't being used.  If it is, tell trent");
                SessionController.Instance.ContentController.Add(libraryElement);
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        libraryElement.LibraryElementId);
                libraryElement.Timestamp = time;
                var loadEventArgs = new LoadContentEventArgs(_message["data"]?.ToString());
                if (_message.ContainsKey("data") && _message["data"] != null)
                {
                    if (libraryElement.Type != NusysConstants.ElementType.Word)
                    {
                        controller.Load(loadEventArgs);
                    }
                }
                libraryElement.ServerUrl = url;
            }*/
        }
    }
}
