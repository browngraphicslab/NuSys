using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.FreeForm;

namespace NuSysApp
{ 
    public class LinkController
    {
        private ConcurrentDictionary<string, HashSet<string>> _links = new ConcurrentDictionary<string, HashSet<string>>();

        public HashSet<string> GetLinkedIds(string id)
        {
            return _links[id];
        }

        public void AddLink(string id)
        {
            
            var link =  SessionController.Instance.ContentController.GetContent(id) as LinkLibraryElementModel;
            if (!_links.ContainsKey(link.InAtomId))
            {
                _links[link.InAtomId] = new HashSet<string>();
            }
            _links[link.InAtomId].Add(id);
            if (!_links.ContainsKey(link.OutAtomId))
            {
                _links[link.OutAtomId] = new HashSet<string>();
            }
            _links[link.OutAtomId].Add(id);
        }

        public void RemoveLink(string id)
        {
            var link = SessionController.Instance.ContentController.GetContent(id) as LinkLibraryElementModel;
            if (_links.ContainsKey(link.InAtomId))
            {
                _links[link.InAtomId].Remove(link.InAtomId);
            }
            if (_links.ContainsKey(link.OutAtomId))
            {
                _links[link.OutAtomId].Remove(link.OutAtomId);
            }
        }

        public virtual async Task RequestLink(string otherId, string anotherId, RectangleView rectangle = null, UserControl regionView = null, Dictionary<string, object> inFGDictionary = null, Dictionary<string, object> outFGDictionary = null)
        {
            var contentId = SessionController.Instance.GenerateId();
            var libraryElementRequest = new CreateNewLibraryElementRequest(contentId, null, ElementType.Link, "NEW LINK");
            var request = new NewLinkRequest(anotherId, otherId, SessionController.Instance.ContentController.GetContent(anotherId)?.Creator, 
                contentId, regionView, rectangle, inFGDictionary, outFGDictionary);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
    }
}