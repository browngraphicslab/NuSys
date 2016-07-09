using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using NuSysApp;
using NuSysApp.Components.Viewers.FreeForm;

namespace NuSysApp
{ 
    public class LinksController
    {
        //Just to map a linkable id to the linkable itself
        private ConcurrentDictionary<string,ILinkable> _linkableIdToLinkableController = new ConcurrentDictionary<string, ILinkable>();

        //A content ID to a list of the ids of linkables that are instances of that id
        //aka returns all aliases of a content
        private ConcurrentDictionary<string,HashSet<string>> _contentIdToLinkableIds = new ConcurrentDictionary<string, HashSet<string>>();

        //LibraryElementModel ID to List of LinkLibraryElementModel ID's for links that are attached to it
        private ConcurrentDictionary<string, HashSet<string>> _contentIdToLinkContentIds = new ConcurrentDictionary<string, HashSet<string>>();

        //Get all link Ids attached to a linkable
        private ConcurrentDictionary<string, HashSet<string>> _linkableIdToLinkIds = new ConcurrentDictionary<string, HashSet<string>>();

        /// <summary>
        /// Gets a Linkable (Link controller or ElementController) from it's id.  Aka elementId
        /// </summary>
        /// <param name="linkId"></param>
        /// <returns></returns>
        public ILinkable GetLinkable(string linkId)
        {
            if (_linkableIdToLinkableController.ContainsKey(linkId))
            {
                return _linkableIdToLinkableController[linkId];
            }
            Debug.Fail("we shouldnt ever really fail to find ilinkable for a certain id");
            return null;
        }

        /// <summary>
        /// Adds the linkable to the appropriate linkables dictionaries
        ///  should be called whenever a new linkable is made
        ///  currently in constructors of link controller and element controller
        /// </summary>
        /// <param name="linkable"></param>
        public void AddLinkable(ILinkable linkable)
        {
            Debug.Assert(linkable != null && linkable.Id != null && linkable.ContentId != null);
            _linkableIdToLinkableController.TryAdd(linkable.Id, linkable);
            if (!_contentIdToLinkableIds.ContainsKey(linkable.ContentId))
            {
                _contentIdToLinkableIds[linkable.ContentId] = new HashSet<string>();
            }
            _contentIdToLinkableIds[linkable.ContentId].Add(linkable.Id);
            linkable.Disposed += LinkableOnDisposed;

            //if linkable is LinkController
            var controller = linkable as LinkController;
            if (controller != null)
            {
                var outId = controller?.OutElement?.Id;
                var inId = controller?.InElement?.Id;

                Debug.Assert(inId != null);
                if (!_linkableIdToLinkIds.ContainsKey(inId))
                {
                    _linkableIdToLinkIds[inId] = new HashSet<string>();
                }

                Debug.Assert(outId != null);
                if (!_linkableIdToLinkIds.ContainsKey(outId))
                {
                    _linkableIdToLinkIds[outId] = new HashSet<string>();
                }

                _linkableIdToLinkIds[outId].Add(controller.Id);
                _linkableIdToLinkIds[inId].Add(controller.Id);
            }
            if(!_linkableIdToLinkIds.ContainsKey(linkable.Id))
            {
                _linkableIdToLinkIds[linkable.Id] = new HashSet<string>();
            }
        }

        public bool IsContentId(string id)
        {
            return id != null && SessionController.Instance.ContentController.GetContent(id) != null;
        }

        /// <summary>
        /// Add the link library element controller's library ContentId to the hashset for both endpoint ids' keys
        /// </summary>
        /// <param name="controller"></param>
        public void AddLinkLibraryElementController(LinkLibraryElementController controller)
        {
            var libraryId = controller?.LinkLibraryElementModel?.LibraryElementId;
            Debug.Assert(libraryId != null);
            Debug.Assert(!_contentIdToLinkContentIds.ContainsKey(libraryId));

            var inId = controller?.LinkLibraryElementModel?.InAtomId;
            var outId = controller?.LinkLibraryElementModel?.OutAtomId;
            Debug.Assert(inId != null);
            Debug.Assert(outId != null);

            if (!_contentIdToLinkContentIds.ContainsKey(inId))
            {
                _contentIdToLinkContentIds[inId] = new HashSet<string>();
            }
            if (!_contentIdToLinkContentIds.ContainsKey(outId))
            {
                _contentIdToLinkContentIds[outId] = new HashSet<string>();
            }
            _contentIdToLinkContentIds[inId].Add(libraryId);
            _contentIdToLinkContentIds[outId].Add(libraryId);
        }

        /// <summary>
        /// Gets the list of linkable ids that correspond to LINKS OR NODES that are instances of the given contetn
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public HashSet<string> GetLinkableIdsOfContentIdInstances(string contentId)
        {
            Debug.Assert(contentId != null);
            return _contentIdToLinkableIds.ContainsKey(contentId)
                ? _contentIdToLinkableIds[contentId]
                : new HashSet<string>();
        }

        /// <summary>
        /// converts hash set of library element ids to linklibraryelementcontrollers
        /// CAN CRASH THO
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public HashSet<LinkLibraryElementController> IdHashSetToControllers(IEnumerable<string> ids)
        {
            return new HashSet<LinkLibraryElementController>(ids.Select(item => GetLinkLibraryElementControllerFromLibraryElementId(item)));
        }

        /// <summary>
        /// Gets a link library element controller from it's libraryId
        /// Safe, wont crash.  fuck yeah
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LinkLibraryElementController GetLinkLibraryElementControllerFromLibraryElementId(string id)
        {
            Debug.Assert(id != null);
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
            Debug.Assert(controller is LinkLibraryElementController);
            return controller as LinkLibraryElementController;
        }

        /// <summary>
        /// Returns the list of LinkLibraryElement ID's for links attached to the LibraryElementModel of the passed in id
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public HashSet<string> GetLinkedIds(string contentId)
        {
            if (_contentIdToLinkContentIds.ContainsKey(contentId))
            {
                return _contentIdToLinkContentIds[contentId];
            }
            _contentIdToLinkContentIds[contentId] = new HashSet<string>();
            return _contentIdToLinkContentIds[contentId];
        }

        /// <summary>
        /// To be API called whenever an alias is created
        /// RECURSIVE
        /// </summary>
        /// <param name="linkableController"></param>
        public void AddAlias(ILinkable linkableController)
        {
            var linkableContentId = linkableController.ContentId;
            Debug.Assert(linkableContentId != null);
            var linkLibraryElementModelIds = GetLinkedIds(linkableContentId);
            var oppositeLibraryElementControllers =
                linkLibraryElementModelIds.Select(linkId => 
                GetOppositeLibraryElementModel(linkableContentId, GetLinkLibraryElementControllerFromLibraryElementId(linkId)));

            foreach (var libraryElementController in oppositeLibraryElementControllers)
            {
                var linkables = GetInstancesOfContent(libraryElementController.ContentId);
                foreach (var link in linkables)
                {
                    CreateBezierLinkBetween(linkableController, link);
                }
            }
        }

        /// <summary>
        /// To Be API called whenever an Alias is Removed
        /// Removes all the links attached to that alias
        /// </summary>
        /// <param name="linkableController"></param>
        public void RemoveAlias(string linkableControllerId)
        {
            //Debug.WriteLine($"RemoveAlias: {linkableControllerId}");
            //Debug.Assert(linkableControllerId != null && _linkableIdToLinkIds.ContainsKey(linkableControllerId));
        }

        /// <summary>
        /// Creates the visual links for a given link library element controller
        /// </summary>
        /// <param name="linkController"></param>
        public void CreateVisualLinks(LinkLibraryElementController linkController)
        {
            var contentId1 = linkController.LinkLibraryElementModel.InAtomId;
            var contentId2 = linkController.LinkLibraryElementModel.OutAtomId;
            Debug.Assert(contentId1 != null);
            Debug.Assert(contentId2 != null);
            if (_contentIdToLinkableIds.ContainsKey(contentId1) && _contentIdToLinkableIds.ContainsKey(contentId2))
            {            
                foreach (var visualId1 in _contentIdToLinkableIds[contentId1])
                {
                    foreach (var visualId2 in _contentIdToLinkableIds[contentId2])
                    {
                        CreateBezierLinkBetween(visualId1, visualId2);   
                    }
                }
                
            }
        }

        public async Task RequestLink(Message m)
        {
            Debug.Assert(m.ContainsKey("id1"));
            Debug.Assert(m.ContainsKey("id2"));
            // don't create a link between two library element models, if there is already a link
            // element controller between them
            if(GetLinkLibraryElementControllerBetweenContent(m.GetString("id1"), m.GetString("id2")) != null)
            {
                return;
            }

            var contentId = SessionController.Instance.GenerateId();
            m["contentId"] = contentId;
            var request = new NewLinkRequest(m);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            //SessionController.Instance.ActiveFreeFormViewer.AllContent.First().Controller.RequestVisualLinkTo(contentId);
        }

        /// <summary>
        /// To remove all the visual links from a content
        /// </summary>
        /// <param name="content"></param>
        public void RemoveContent(LibraryElementController content)
        {
            Debug.WriteLine($"RemoveContent {content.ContentId}");
            var libraryElementId = content?.LibraryElementModel?.LibraryElementId;
            Debug.Assert(libraryElementId != null);
            Debug.Assert(content?.LibraryElementModel != null);
            if (content.LibraryElementModel.Type == ElementType.Link)
            {
                Debug.Assert(content.LibraryElementModel is LinkLibraryElementModel);
                var linkLibraryElementModel = content.LibraryElementModel as LinkLibraryElementModel;
                
                Debug.Assert(_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.InAtomId));
                Debug.Assert(_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.OutAtomId));
                Debug.Assert(_contentIdToLinkContentIds[linkLibraryElementModel.InAtomId].Contains(libraryElementId));
                Debug.Assert(_contentIdToLinkContentIds[linkLibraryElementModel.OutAtomId].Contains(libraryElementId));

                _contentIdToLinkableIds[linkLibraryElementModel.InAtomId].Remove(libraryElementId);
                _contentIdToLinkableIds[linkLibraryElementModel.OutAtomId].Remove(libraryElementId);
            }

            var linkableAliases = GetInstancesOfContent(libraryElementId);
            foreach (var linkableAlias in linkableAliases)
            {
                Debug.Assert(_contentIdToLinkableIds.ContainsKey(linkableAlias.ContentId));
                RemoveAlias(linkableAlias.Id);
            }
            DisposeContent(content.ContentId);
        }

        /// <summary>
        /// pass in the id to convert to other ids and then we will convert those ids to objects
        /// we will return the objects from the original id you gave us. 
        /// make sure it is an id, otherwise we wont find it in the id list
        /// 
        /// parameter: id
        /// 
        /// -Sahil
        /// 
        /// Finds the LibraryElementControllers that are the endpoints of the passed in LinkController Id
        /// -Trent
        /// </summary>
        /// <param name="linkControllerId"></param>
        /// <returns></returns>
        public Tuple<LibraryElementController, LibraryElementController> GetEndControllersOfLinkControllerId(
            string linkControllerId)
        {
            Debug.Assert(linkControllerId != null);
            var linkController = GetLinkable(linkControllerId) as LinkController;

            var contentId1 = linkController?.InElement?.ContentId;
            Debug.Assert(contentId1 != null);

            var contentId2 = linkController?.OutElement?.ContentId;
            Debug.Assert(contentId2 != null);

            var linkable1 = SessionController.Instance.ContentController.GetLibraryElementController(contentId1);
            Debug.Assert(linkable1 != null);
            var linkable2 = SessionController.Instance.ContentController.GetLibraryElementController(contentId2);
            Debug.Assert(linkable2 != null);

            return new Tuple<LibraryElementController, LibraryElementController>(linkable1, linkable2);

        }

        /// <summary>
        /// Creates a bezier link between the two linkables passed in
        /// Their content Ids must be populated correctly
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        private void CreateBezierLinkBetween(ILinkable one, ILinkable two)
        {
            var linkLibElemController = GetLinkLibraryElementControllerBetweenLinkables(one, two);
            Debug.Assert(linkLibElemController != null);
            Debug.Assert(one.Id != two.Id);
            var model = new LinkModel();
            model.InAtomId = one.Id;
            model.OutAtomId = two.Id;
            var controller = new LinkController(model, linkLibElemController);
            var vm = new LinkViewModel(controller);
           
            UITask.Run(async delegate
                        {
                            var view = new BezierLinkView(vm);
                            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Add(view);
                        });
        }

        /// <summary>
        /// Gets the ILinkable for each LinkableId and calls createbezierlinkbetween
        /// </summary>
        /// <param name="visualId1"></param>
        /// <param name="visualId2"></param>
        private void CreateBezierLinkBetween(string LinkableId1, string LinkableId2)
        {
            Debug.Assert(LinkableId1 != null && _linkableIdToLinkableController.ContainsKey(LinkableId1));
            Debug.Assert(LinkableId2 != null && _linkableIdToLinkableController.ContainsKey(LinkableId2));

            var linkable1 = GetLinkable(LinkableId1);
            var linkable2 = GetLinkable(LinkableId2);

            CreateBezierLinkBetween(linkable1, linkable2);
        }

        /// <summary>
        /// Gets the LinkLibraryElementController for the link that exists between ILinkables
        /// Fails if the link LinkLibraryElementController does not exist
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        private LinkLibraryElementController GetLinkLibraryElementControllerBetweenLinkables(ILinkable one, ILinkable two)
        {

            Debug.Assert(one != null && one.ContentId != null);
            Debug.Assert(two != null && two.ContentId != null);

            var oneLinkLEMId = _contentIdToLinkContentIds[one.ContentId];
            var twoLinkLEMId = _contentIdToLinkContentIds[two.ContentId];
            var intersect = oneLinkLEMId.Intersect(twoLinkLEMId).ToList();
            Debug.Assert(intersect.Count < 2, "There can be zero or one link library element controllers between any two linkables");
            // if there is no link LEC
            if (intersect.Count == 0)
            {
                return null;
            }
            var controller = GetLinkLibraryElementControllerFromLibraryElementId(intersect.FirstOrDefault());
            Debug.Assert(controller != null);
            return controller;
        }

        private LinkLibraryElementController GetLinkLibraryElementControllerBetweenContent(string libElemId1, string libElemId2)
        {

            Debug.Assert(libElemId1 != null && _contentIdToLinkContentIds.ContainsKey(libElemId1));
            Debug.Assert(libElemId2 != null && _contentIdToLinkContentIds.ContainsKey(libElemId2));
            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementController(libElemId1) != null);
            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementController(libElemId2) != null);


            var linkLibElemControllersIds1 = _contentIdToLinkContentIds[libElemId1];
            var linkLibElemControllersIds2 = _contentIdToLinkContentIds[libElemId2];
            var intersect = linkLibElemControllersIds1.Intersect(linkLibElemControllersIds2).ToList();
            Debug.Assert(intersect.Count < 2, "There can be zero or one link library element controllers between any two library elements");
            // if there is no link LEC
            if (intersect.Count == 0)
            {
                return null;
            }
            var controller = GetLinkLibraryElementControllerFromLibraryElementId(intersect.FirstOrDefault());
            Debug.Assert(controller != null);
            return controller;
        }

        /// <summary>
        /// Return all the instances of a link of libraryelementModel as iLinkable controllers
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        private IEnumerable<ILinkable> GetInstancesOfContent(string contentId)
        {
            Debug.Assert(contentId != null);
            if (_contentIdToLinkableIds.ContainsKey(contentId))
            {
                var linkIds = _contentIdToLinkableIds[contentId];
                IEnumerable<ILinkable> linkables;
                try
                {
                    linkables = linkIds.Select(GetLinkable);
                    return linkables;
                }
                catch (KeyNotFoundException e)
                {
                    Debug.Fail("We shouldn't have a linkable id that doesn't map to a linkable");
                }
                catch (ArgumentNullException e)
                {
                    Debug.Fail("We shouldn't have a linkable id that is null!");
                }
                return new HashSet<ILinkable>();
            }
            else
            {
                return new HashSet<ILinkable>();
            }
        }

        /// <summary>
        /// Gets the library element opposite the id passed in
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <param name="linkController"></param>
        /// <returns></returns>
        private LibraryElementController GetOppositeLibraryElementModel(string libraryElementId,
            LinkLibraryElementController linkController)
        {
            Debug.Assert(libraryElementId != null);
            Debug.Assert(SessionController.Instance.ContentController.GetContent(libraryElementId) != null);

            var inId = linkController?.LinkLibraryElementModel?.InAtomId;
            var outId = linkController?.LinkLibraryElementModel?.OutAtomId;
            Debug.Assert(inId != null);
            Debug.Assert(outId != null);

            string oppositeModelIdToReturn = null;

            if (libraryElementId == inId)
            {
                oppositeModelIdToReturn = outId;
            }
            else if(libraryElementId == outId)
            {
                oppositeModelIdToReturn = inId;
            }
            Debug.Assert(oppositeModelIdToReturn != null);
            var controllerToReturn = SessionController.Instance.ContentController.GetLibraryElementController(oppositeModelIdToReturn);
            Debug.Assert(controllerToReturn != null);
            return controllerToReturn;
        }

        private void LinkableOnDisposed(object sender, object args)
        {
            var linkable = sender as ILinkable;
            Debug.Assert(linkable != null);
            linkable.Disposed -= LinkableOnDisposed;
            DisposeLinkable(linkable.Id);
        }

        /// <summary>
        /// takes care of mapping when a Linkable is removed
        /// </summary>
        /// <param name="linkableId"></param>
        private void DisposeLinkable(string linkableId)
        {
            Debug.WriteLine($"DisposeLinkable: {linkableId}");
            Debug.Assert(linkableId != null);

            ILinkable outLinkable;
            _linkableIdToLinkableController.TryRemove(linkableId, out outLinkable);

            Debug.Assert(outLinkable != null);
            Debug.Assert(outLinkable.ContentId != null);
            Debug.Assert(_contentIdToLinkableIds.ContainsKey(outLinkable.ContentId));
            Debug.Assert(_contentIdToLinkableIds[outLinkable.ContentId].Contains(linkableId));

            _contentIdToLinkableIds[outLinkable.ContentId].Remove(linkableId);

            HashSet<string> outObj;
            _linkableIdToLinkIds.TryRemove(linkableId, out outObj);
        }

        /// <summary>
        /// takes care of mapping when a content is removed
        /// </summary>
        /// <param name="contentId"></param>
        private void DisposeContent(string contentId)
        {

            Debug.WriteLine($"DisposeContent {contentId}");
            HashSet<string> outObj;
            _contentIdToLinkableIds.TryRemove(contentId, out outObj);
            _contentIdToLinkContentIds.TryRemove(contentId, out outObj);

        }

        //private ILinkable GetLinkableBetweenLinkables(ILinkable one, ILinkable two)
        //{
        //    Debug.Assert(one != null && _linkableIdToLinkIds.ContainsKey(one.Id));
        //    var oneLinkables = _linkableIdToLinkIds[one];

        //    Debug.Assert(one != null && _linkableIdToLinkIds.ContainsKey(one.Id));
        //    var oneLinkables = _linkableIdToLinkIds[one];
        //    _linkableIdToLinkIds
        //}

    }
}