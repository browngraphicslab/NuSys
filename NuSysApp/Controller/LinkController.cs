using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.FreeForm;

namespace NuSysApp
{ 
    public class LinkController
    {
        private ConcurrentDictionary<string, HashSet<string>> _links = new ConcurrentDictionary<string, HashSet<string>>();
 //       private ConcurrentDictionary<string, Color> _colors = new ConcurrentDictionary<string, Color>();
        public delegate void NewLinkEventHandler(LinkLibraryElementModel link);
        public event NewLinkEventHandler OnNewLink;

        public delegate void RemoveLinkEventHandler(LinkLibraryElementModel link);
        public event RemoveLinkEventHandler OnLinkRemoved;

        public HashSet<string> GetLinkedIds(string id)
        {
            if (_links.ContainsKey(id))
            {
                return _links[id];
            }
            _links[id] = new HashSet<string>();
            return _links[id];
        }

        public void AddLink(string id)
        {

            var link = SessionController.Instance.ContentController.GetContent(id) as LinkLibraryElementModel;
            if (!_links.ContainsKey(link.InAtomId))
            {
                _links[link.InAtomId] = new HashSet<string>();
            }
            foreach (var l in _links[link.InAtomId])
            {
                var temp = SessionController.Instance.ContentController.GetContent(l) as LinkLibraryElementModel;
                if ((temp.InAtomId == link.InAtomId && temp.OutAtomId == link.OutAtomId) ||
                    (temp.InAtomId == link.OutAtomId && temp.OutAtomId == link.InAtomId))
                {
                    return;
                }
            }

            _links[link.InAtomId].Add(id);

            if (!_links.ContainsKey(link.OutAtomId))
            {
                _links[link.OutAtomId] = new HashSet<string>();
            }
            _links[link.OutAtomId].Add(id);
            OnNewLink?.Invoke(link);
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
            var request = new NewLinkRequest(anotherId, otherId, SessionController.Instance.ContentController.GetContent(anotherId)?.Creator, 
                contentId, regionView, rectangle, inFGDictionary, outFGDictionary);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }

        public void ChangeLinkTitle(string linkLibraryElementId, string title)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(linkLibraryElementId);
            controller.SetTitle(title);
        }

        public void ChangeLinkTags(string linkLibraryElementId, HashSet<string> tags)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(linkLibraryElementId);

            foreach (var tag in tags)
            {
                if (!controller.LibraryElementModel.Keywords.Contains(new Keyword(tag)))
                {
                    controller.AddKeyword(new Keyword(tag));
                }
            }
            
        }
    }
}