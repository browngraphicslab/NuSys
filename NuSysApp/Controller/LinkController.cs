using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using NuSysApp;
using NuSysApp.Components.Viewers.FreeForm;

namespace NuSysApp
{ 
    public class LinkController
    {
        private ConcurrentDictionary<LinkId, HashSet<string>> _links = new ConcurrentDictionary<LinkId, HashSet<string>>();
        private ConcurrentDictionary<string, string> _endPointsToLinks = new ConcurrentDictionary<string, string>();

        public delegate void NewLinkEventHandler(LinkLibraryElementController link);
        public event NewLinkEventHandler OnNewLink;

        public delegate void RemoveLinkEventHandler(LinkLibraryElementController link);
        public event RemoveLinkEventHandler OnLinkRemoved;

        public HashSet<string> GetLinkedIds(LinkId id)
        {
            if (_links.ContainsKey(id))
            {
                return _links[id];
            }
            _links[id] = new HashSet<string>();
            return _links[id];
        }
        //id is the id of a LinkLibraryElementModel
        public void AddLink(string id)
        {
            var link = SessionController.Instance.ContentController.GetContent(id) as LinkLibraryElementModel;
            LinkId inAtomId = link.InAtomId;
            LinkId outAtomId = link.OutAtomId;

            if (!_links.ContainsKey(inAtomId))
            {
                _links[inAtomId] = new HashSet<string>();
            }
            foreach (var l in _links[inAtomId])
            {
                var temp = SessionController.Instance.ContentController.GetContent(l) as LinkLibraryElementModel;
                if ((temp.InAtomId == inAtomId && temp.OutAtomId == outAtomId) ||
                    (temp.InAtomId == outAtomId && temp.OutAtomId == inAtomId))
                {
                    return;
                }
            }
            if (!_links[inAtomId].Contains(id))
            {
                _links[inAtomId].Add(id);
            }

            if (!_links.ContainsKey(outAtomId))
            {
                _links[outAtomId] = new HashSet<string>();
            }
            if (!_links[outAtomId].Contains(id))
            {
                _links[outAtomId].Add(id);
            }

            AddToEndPointsToLink(inAtomId, outAtomId, id);
            OnNewLink?.Invoke(SessionController.Instance.ContentController.GetLibraryElementController(id) as LinkLibraryElementController);
        }

        private void AddToEndPointsToLink(LinkId inAtomId, LinkId outAtomId, string idOfLink)
        {
            string id1 = inAtomId.RegionId == null ? inAtomId.LibraryElementId : inAtomId.RegionId;
            string id2 = outAtomId.RegionId == null ? outAtomId.LibraryElementId : outAtomId.RegionId;
            _endPointsToLinks[id1 + id2] = idOfLink;
            _endPointsToLinks[id2 + id1] = idOfLink;
        }

        public string GetLinkIdBetween(LinkId inAtomId, LinkId outAtomId)
        {
            string id1 = inAtomId.RegionId == null ? inAtomId.LibraryElementId : inAtomId.RegionId;
            string id2 = outAtomId.RegionId == null ? outAtomId.LibraryElementId : outAtomId.RegionId;
            if (_endPointsToLinks.ContainsKey(id1 + id2) &&
                _endPointsToLinks.ContainsKey(id1 + id2) != null)
            {
                return _endPointsToLinks[id1 + id2];
            }
            throw new ArgumentException("end points have no link associated with them");
        }

        public HashSet<LinkLibraryElementController> IdHashSetToControllers(IEnumerable<string> ids) { 
            return new HashSet<LinkLibraryElementController>(ids.Select(item => GetLinkLibraryElementController(item)));
        }

        public LinkLibraryElementController GetLinkLibraryElementController(string id)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
            Debug.Assert(controller is LinkLibraryElementController);
            return controller as LinkLibraryElementController;
        }   

        public void RemoveLink(string id)
        {
            var link = SessionController.Instance.ContentController.GetContent(id) as LinkLibraryElementModel;
            if (link == null)
            {
                return;
            }
            if (_links.ContainsKey(link.InAtomId))
            {
                _links[link.InAtomId].Remove(id);
            }
            if (_links.ContainsKey(link.OutAtomId))
            {
                _links[link.OutAtomId].Remove(id);
            }
            RemoveFromEndPointsToLink(link.InAtomId, link.OutAtomId);
            OnLinkRemoved?.Invoke(SessionController.Instance.ContentController.GetLibraryElementController(id) as LinkLibraryElementController);
        }
        //removes a link from endPointsToLink
        private void RemoveFromEndPointsToLink(LinkId inAtomId, LinkId outAtomId)
        {
            string id1 = inAtomId.RegionId == null ? inAtomId.LibraryElementId : inAtomId.RegionId;
            string id2 = outAtomId.RegionId == null ? outAtomId.LibraryElementId : outAtomId.RegionId;
            if (_endPointsToLinks.ContainsKey(id1 + id2))
            {
                _endPointsToLinks[id1 + id2] = null;
            }
            if (_endPointsToLinks.ContainsKey(id2 + id1))
            {
                _endPointsToLinks[id2 + id1] = null;
            }
        }

        public virtual async Task RequestLink(LinkId otherId, LinkId anotherId, RectangleView rectangle = null, UserControl regionView = null, Dictionary<string, object> inFGDictionary = null, Dictionary<string, object> outFGDictionary = null)
        {
            var contentId = SessionController.Instance.GenerateId();
            var request = new NewLinkRequest(anotherId, otherId, SessionController.Instance.ContentController.GetContent(anotherId.LibraryElementId)?.Creator, 
                contentId, regionView, rectangle, inFGDictionary, outFGDictionary);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            SessionController.Instance.ActiveFreeFormViewer.AllContent.First().Controller.RequestVisualLinkTo(contentId);
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