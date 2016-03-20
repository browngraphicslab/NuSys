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

        public CreateNewLibraryElementRequest(string id, string data, ElementType type, string title = null)
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

        public override async Task CheckOutgoingRequest()
        {
            _message["library_element_creation_timestamp"] = DateTime.UtcNow;

            ElementType type = (ElementType) Enum.Parse(typeof (ElementType), (string) _message["type"], true);
            if (type == ElementType.Collection)
            {
                SessionController.Instance.ContentController.Add(
                    new CollectionLibraryElementModel((string) _message["id"]));
            }
            else
            {
                SessionController.Instance.ContentController.Add(new LibraryElementModel((string) _message["id"], type));
            }
        }
    }
}
