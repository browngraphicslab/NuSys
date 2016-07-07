using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateNewLibraryElementRequest : Request
    {
        public CreateNewLibraryElementRequest(Message m) : base(RequestType.CreateNewLibrayElementRequest, m)
        {
            SetServerSettings();
        }

        public CreateNewLibraryElementRequest(string id, string data, ElementType type, string title = "")
            : base(RequestType.CreateNewLibrayElementRequest)
        {
            _message["id"] = id;
            _message["data"] = data;
            _message["type"] = type.ToString();
            if (title != null)
            {
                _message["title"] = title;
            }
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.None);
            SetServerItemType(ServerItemType.Content);
            SetServerRequestType(ServerRequestType.Add);
        }

        public override async Task<bool> CheckOutgoingRequest()
        {
            SetServerSettings();
            var time = DateTime.UtcNow.ToString();
            _message["library_element_creation_timestamp"] = time;
            string url = null;
            if (_message.ContainsKey("server_url"))
            {
                url = _message["server_url"].ToString();
            }

            ElementType type = (ElementType) Enum.Parse(typeof(ElementType), (string) _message["type"], true);

            LibraryElementModel libraryElement = LibraryElementModelFactory.CreateFromMessage(_message);
            if (libraryElement != null)
            {
                SessionController.Instance.ContentController.Add(libraryElement);
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        libraryElement.LibraryElementId);
                libraryElement.Timestamp = time;
                var loadEventArgs = new LoadContentEventArgs(_message["data"]?.ToString());
                if (_message.ContainsKey("data") && _message["data"] != null)
                {
                    if (libraryElement.Type != ElementType.Word)
                    {
                        controller.Load(loadEventArgs);
                    }
                }
                libraryElement.ServerUrl = url;
            }
            return true;
        }
    }
}
