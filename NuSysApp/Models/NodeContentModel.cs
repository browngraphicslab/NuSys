using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public bool Loaded { get; set; }//TODO Add a loaded event
        //TODO add in 'MakeNewController' method that creates a new controller-model pair pointing to this and returns it

        public delegate void ContentChangedEventHandler(ElementViewModel originalSenderViewModel = null);
        public event ContentChangedEventHandler OnContentChanged;

        public NodeContentModel(string data, string id, ElementType elementType,string contentName = null)
        {
            Data = data;
            Id = id;
            ContentName = contentName;
            Type = elementType;
            Loaded = data != null;
        }
        public void FireContentChanged()
        {
            OnContentChanged?.Invoke();
        }

        public void SetContentData(ElementViewModel originalSenderViewModel, string data)
        {
            Data = data;

            Task.Run(async delegate
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(Id,data));
            });

            OnContentChanged?.Invoke(originalSenderViewModel);
        }

        public ElementType Type { get; set; }
        public string Data { get; set; }
        public string Id { get; set; }
        public string ContentName { get; set; }

    }
}
