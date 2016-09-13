using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;
using Windows.UI.Xaml;

namespace NuSysApp
{ 
    public class LinksController
    {
        /// <summary>
        /// Just to map a linkable id to the linkable itself
        /// </summary>
        private ConcurrentDictionary<string,ILinkable> _linkableIdToLinkableController = new ConcurrentDictionary<string, ILinkable>();

        //A content ID to a list of the ids of linkables that are instances of that id
        //aka returns all aliases of a content
        private ConcurrentDictionary<string,HashSet<string>> _contentIdToLinkableIds = new ConcurrentDictionary<string, HashSet<string>>();

        //LibraryElementModel ID to List of LinkLibraryElementModel ID's for links that are attached to it
        private ConcurrentDictionary<string, HashSet<string>> _contentIdToLinkContentIds = new ConcurrentDictionary<string, HashSet<string>>();

        //Get all link Ids attached to a linkable
        private ConcurrentDictionary<string, HashSet<string>> _linkableIdToLinkIds = new ConcurrentDictionary<string, HashSet<string>>();

        private ConcurrentDictionary<string, HashSet<LinkViewModel>> _collectionLibraryIdToLinkViewModels = new ConcurrentDictionary<string, HashSet<LinkViewModel>>();
        
        public HashSet<LinkViewModel> GetLinkViewModel(string collectionLibraryId)
        {
            if (_collectionLibraryIdToLinkViewModels.ContainsKey(collectionLibraryId))
            {
                return _collectionLibraryIdToLinkViewModels[collectionLibraryId];
            }
            return new HashSet<LinkViewModel>(); 
        }

        public void Clear()
        {
            _collectionLibraryIdToLinkViewModels.Clear();
        }

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
        /// When set to true we render all of the visual links that connect the nodes in the current workspace
        /// When set to false we only render the circle links that exist between contents and not the bezier links
        /// </summary>
        public bool AreBezierLinksVisible = true;

        /// <summary>
        /// Adds the linkable to the appropriate linkables dictionaries
        ///  should be called whenever a new linkable is made
        ///  currently in constructors of link controller and element controller
        /// </summary>
        /// <param name="linkable"></param>
        public void AddLinkable(ILinkable linkable)
        {
            Debug.Assert(linkable != null && linkable.Id != null && linkable.ContentId != null);
            var linkableContentId = linkable.ContentId;
            _linkableIdToLinkableController.TryAdd(linkable.Id, linkable);
            if (!_contentIdToLinkableIds.ContainsKey(linkableContentId))
            {
                _contentIdToLinkableIds[linkableContentId] = new HashSet<string>();
            }
            _contentIdToLinkableIds[linkableContentId].Add(linkable.Id);
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

                controller.OutElement.UpdateCircleLinks();
                controller.InElement.UpdateCircleLinks();
            }
            if(!_linkableIdToLinkIds.ContainsKey(linkable.Id))
            {
                _linkableIdToLinkIds[linkable.Id] = new HashSet<string>();
            }
            
            // Get the list of links attached to the current linkable's library element model
            var linkLibraryElementModelIds = GetLinkedIds(linkableContentId);
            // Get the list of library element controllers that are attached to this linkable's library elment model
            var oppositeLibraryElementControllers =
                linkLibraryElementModelIds.Select(linkId =>
                GetOppositeLibraryElementModel(linkableContentId, GetLinkLibraryElementControllerFromLibraryElementId(linkId)));

            // create the bezier lnks
            foreach (var libraryElementController in oppositeLibraryElementControllers ?? new List<LibraryElementController>())
            {
                if (libraryElementController == null)
                {
                    continue;
                }
                var linkables = GetInstancesOfLibraryElement(libraryElementController.LibraryElementModel.LibraryElementId);
                foreach (var toLinkTo in linkables)
                {
                    var linkLibElemController = GetLinkLibraryElementControllerBetweenLinkables(linkable, toLinkTo);
                    Debug.Assert(linkLibElemController != null);
                    Debug.Assert(linkable.Id != toLinkTo.Id);

                    if (linkLibElemController.LinkLibraryElementModel.InAtomId.Equals(linkable.ContentId))
                    {
                        CreateBezierLinkBetween(linkable, toLinkTo);
                    }
                    else
                    {
                        CreateBezierLinkBetween(toLinkTo, linkable);
                    }
                }
            }
        }
        /// <summary>
        /// When the global function that changes the visibility of the bezier links is changed in a global setting this fucntion changes the value and also redraws the 
        /// visual links so that the settings are applied in real time.
        /// </summary>
        /// <param name="visibility"></param>
        public void ChangeVisualLinkVisibility(bool visibility)
        {
            AreBezierLinksVisible = visibility;
            if (visibility)
            {
                foreach (var linkId in SessionController.Instance.ContentController.IdList.Where(e => SessionController.Instance.ContentController.GetLibraryElementModel(e) is LinkLibraryElementModel))
                {
                    var linkController = GetLinkLibraryElementControllerFromLibraryElementId(linkId);
                    CreateVisualLinks(linkController);
                }
            }
            else
            {
                foreach(var linkAtom in new HashSet<FrameworkElement>(SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(e => e is BezierLinkView)))
                {
                    SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(linkAtom);
                }
            }
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
        /// To remove all the visual links from a libraryElementController
        /// </summary>
        /// <param name="content"></param>
        public void RemoveContent(LibraryElementController libraryElementController)
        {
            var libraryElementId = libraryElementController?.LibraryElementModel?.LibraryElementId;
            Debug.Assert(libraryElementId != null);
            Debug.Assert(libraryElementController?.LibraryElementModel != null);
            if (libraryElementController.LibraryElementModel.Type == NusysConstants.ElementType.Link)
            {
                Debug.Assert(libraryElementController.LibraryElementModel is LinkLibraryElementModel);
                var linkLibraryElementModel = libraryElementController.LibraryElementModel as LinkLibraryElementModel;

                //Debug.Assert(_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.InAtomId));
                //Debug.Assert(_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.OutAtomId));
                //Debug.Assert(_contentIdToLinkContentIds[linkLibraryElementModel.InAtomId].Contains(libraryElementId));
                //Debug.Assert(_contentIdToLinkContentIds[linkLibraryElementModel.OutAtomId].Contains(libraryElementId));
                if (_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.InAtomId))
                {
                    _contentIdToLinkContentIds[linkLibraryElementModel.InAtomId].Remove(libraryElementId);
                }
                if (_contentIdToLinkContentIds.ContainsKey(linkLibraryElementModel.OutAtomId))
                {
                    _contentIdToLinkContentIds[linkLibraryElementModel.OutAtomId].Remove(libraryElementId);
                }

                foreach (var collectionLibraryIdToLinkViewModel in _collectionLibraryIdToLinkViewModels)
                {
                    foreach (var linkViewModel in collectionLibraryIdToLinkViewModel.Value.ToArray())
                    {

                        if (linkViewModel.Controller.ContentId == linkLibraryElementModel.OutAtomId || linkViewModel.Controller.ContentId == linkLibraryElementModel.InAtomId)
                        {
                            collectionLibraryIdToLinkViewModel.Value.Remove(linkViewModel);
                        }
                    }
                }
            }

            var linkedIds = GetLinkedIds(libraryElementController.LibraryElementModel.LibraryElementId);
            DisposeLibraryElement(libraryElementController.LibraryElementModel.LibraryElementId);
            foreach (var linkId in linkedIds)
            {
                var linkController = GetLinkLibraryElementControllerFromLibraryElementId(linkId);
                var inelementid = linkController.LinkLibraryElementModel.InAtomId;
                var outelementid = linkController.LinkLibraryElementModel.OutAtomId;

     


                if (_contentIdToLinkableIds.ContainsKey(inelementid))
                {
                    foreach (var visualId in _contentIdToLinkableIds[inelementid])
                    {
                        // We add the circle links even so that even if one of them is not on the current workspace we can still see that a link exists
                        GetLinkable(visualId).UpdateCircleLinks();
                    }
                }
                if (_contentIdToLinkableIds.ContainsKey(outelementid))
                {
                    foreach (var visualId in _contentIdToLinkableIds[outelementid])
                    {
                        // We add the circle links even so that even if one of them id not on the current workspace we can still see that a link exists
                        GetLinkable(visualId).UpdateCircleLinks();
                    }
                }
            }
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

            if (!AreBezierLinksVisible)
            {
                return; // if we do not want to see the visual links then this should stop the links from being created
            }
            var oneParentCollectionId = one.GetParentCollectionId();
            var twoParentCollectionId = two.GetParentCollectionId();

            if (oneParentCollectionId != twoParentCollectionId || oneParentCollectionId == null)
            {
                return;
            }

            var linkLibElemController = GetLinkLibraryElementControllerBetweenLinkables(one, two);
            Debug.Assert(linkLibElemController != null);
            Debug.Assert(one.Id != two.Id);
            var model = new LinkModel();
            model.InAtomId = one.Id;
            model.OutAtomId = two.Id;
            var controller = new LinkController(model, linkLibElemController);
            //    var allContent = SessionController.Instance.ActiveFreeFormViewer.AllContent;
            //   var collectionViewModel = allContent.FirstOrDefault(item => ((item as GroupNodeViewModel)?.LibraryElementId == oneParentCollectionId)) as GroupNodeViewModel;

            var vm = new LinkViewModel(controller);
            if (!_collectionLibraryIdToLinkViewModels.ContainsKey(oneParentCollectionId)) { 
                _collectionLibraryIdToLinkViewModels[oneParentCollectionId] = new HashSet<LinkViewModel>();
            }
            _collectionLibraryIdToLinkViewModels[oneParentCollectionId].Add(vm);

            var parentCollectionController = SessionController.Instance.ContentController.GetLibraryElementController(oneParentCollectionId);
            parentCollectionController.AddLink(new LinkViewModel(controller));

            return;
            /*
            if (collectionViewModel != null)
            {
             //   collectionViewModel.Links.Add(vm);
            }
            else if (SessionController.Instance.ActiveFreeFormViewer.LibraryElementId == oneParentCollectionId)
            {
                SessionController.Instance.SessionView.FreeFormViewer.InitialCollection.ViewModel.Links.Add(vm);
            }
            */
            controller.OutElement.UpdateCircleLinks();
            controller.InElement.UpdateCircleLinks();
        }


         /// <summary>
         /// Creates the visual links for a given link library element controller
         /// </summary>
         /// <param name="linkController"></param>
         public void CreateVisualLinks(LinkLibraryElementController linkController)
         {
             var contentId1 = linkController?.LinkLibraryElementModel?.InAtomId;
             var contentId2 = linkController?.LinkLibraryElementModel?.OutAtomId;
             Debug.Assert(contentId1 != null);
             Debug.Assert(contentId2 != null);
             if (_contentIdToLinkableIds.ContainsKey(contentId1) && _contentIdToLinkableIds.ContainsKey(contentId2) && AreBezierLinksVisible) // the AreBezierLinksVisible is just to clean up runtime
             {
                 foreach (var visualId1 in _contentIdToLinkableIds[contentId1])
                 {
                     foreach (var visualId2 in _contentIdToLinkableIds[contentId2])
                     {
                        CreateBezierLinkBetween(visualId1, visualId2);
                     }
                 }
 
             }
            if (_contentIdToLinkableIds.ContainsKey(contentId1)) {
                foreach (var visualId in _contentIdToLinkableIds[contentId1])
                {
                    // We add the circle links even so that even if one of them is not on the current workspace we can still see that a link exists
                    GetLinkable(visualId).UpdateCircleLinks();
                }
            }
            if (_contentIdToLinkableIds.ContainsKey(contentId2)){
                foreach (var visualId in _contentIdToLinkableIds[contentId2])
                {
                    // We add the circle links even so that even if one of them id not on the current workspace we can still see that a link exists
                    GetLinkable(visualId).UpdateCircleLinks();
                }
            }

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


        /// <summary>
        /// returns the LinkLibraryElementController for two libraryElementModels passed in via their Id's
        /// null if no link existed between them
        /// returns null if the link doesn't exist
        /// </summary>
        /// <param name="libElemId1"></param>
        /// <param name="libElemId2"></param>
        /// <returns></returns>
        public LinkLibraryElementController GetLinkLibraryElementControllerBetweenContent(string libElemId1, string libElemId2)
        {


            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementController(libElemId1) != null);
            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementController(libElemId2) != null);

            if (!_contentIdToLinkContentIds.ContainsKey(libElemId1) || !_contentIdToLinkContentIds.ContainsKey(libElemId2))
            {
                return null;
            }
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
        /// Gets the library element opposite the id passed in
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <param name="linkController"></param>
        /// <returns></returns>
        public LibraryElementController GetOppositeLibraryElementModel(string libraryElementId,
            LinkLibraryElementController linkController)
        {
            Debug.Assert(libraryElementId != null);
            Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementModel(libraryElementId) != null);

            var inId = linkController?.LinkLibraryElementModel?.InAtomId;
            var outId = linkController?.LinkLibraryElementModel?.OutAtomId;
            Debug.Assert(inId != null);
            Debug.Assert(outId != null);

            string oppositeModelIdToReturn = null;

            if (libraryElementId == inId)
            {
                oppositeModelIdToReturn = outId;
            }
            else if (libraryElementId == outId)
            {
                oppositeModelIdToReturn = inId;
            }
            Debug.Assert(oppositeModelIdToReturn != null);
            var controllerToReturn = SessionController.Instance.ContentController.GetLibraryElementController(oppositeModelIdToReturn);
            //Debug.Assert(controllerToReturn != null);
            return controllerToReturn;
        }

        /// <summary>
        /// Return all the instances of a link of libraryelementModel as iLinkable controllers
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public IEnumerable<ILinkable> GetInstancesOfLibraryElement(string contentId)
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
            Debug.Assert(linkableId != null);

            ILinkable outLinkable;
            _linkableIdToLinkableController.TryRemove(linkableId, out outLinkable);

            Debug.Assert(outLinkable != null);
            Debug.Assert(outLinkable.ContentId != null);

            if (_contentIdToLinkableIds.ContainsKey(outLinkable.ContentId) &&
                _contentIdToLinkableIds[outLinkable.ContentId].Contains(linkableId))
            {

                _contentIdToLinkableIds[outLinkable.ContentId].Remove(linkableId);
            }

            HashSet<string> outObj;
            _linkableIdToLinkIds.TryRemove(linkableId, out outObj);
        }

        /// <summary>
        /// takes care of mapping when a LibraryElement is removed
        /// </summary>
        /// <param name="libraryElementId"></param>
        private void DisposeLibraryElement(string libraryElementId)
        {
            HashSet<string> outObj;
            if (_contentIdToLinkContentIds.ContainsKey(libraryElementId))
            {
                foreach (var linkId in _contentIdToLinkContentIds[libraryElementId])
                {
                    RemoveLink(linkId);
                }
            }
            _contentIdToLinkableIds.TryRemove(libraryElementId, out outObj);
            _contentIdToLinkContentIds.TryRemove(libraryElementId, out outObj);           
        }

        /// <summary>
        /// Takes in a link id, and removes the link. takes care of firing linkremoved on both end points
        /// </summary>
        /// <param name="linkLibraryElementId"></param>
        /// <returns>returns true if link was deleted successfully, false otherwise</returns>
        public async Task<bool> RemoveLink(string linkLibraryElementId)
        {
            // get the linkLibraryElementController from the link id
            var linkLibraryElementController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(linkLibraryElementId);

            var request = new DeleteLibraryElementRequest(linkLibraryElementId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            // if the request was performed successfully
            if (request.DeleteLocally())
            {
                // get the library element controllers for each side of the link and fire the link removed events on them
                var inLibraryElementId = linkLibraryElementController.LinkLibraryElementModel.InAtomId;
                var inLibElemController =
                    SessionController.Instance.ContentController.GetLibraryElementController(inLibraryElementId);
                if (inLibElemController != null) {
                    inLibElemController.InvokeLinkRemoved(linkLibraryElementId);
                }
                var outLibraryElementId = linkLibraryElementController.LinkLibraryElementModel.OutAtomId;
                var outLibElemController =
                    SessionController.Instance.ContentController.GetLibraryElementController(outLibraryElementId);
                if (outLibElemController != null)
                {
                    outLibElemController.InvokeLinkRemoved(linkLibraryElementId);
                }

                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.RemoveLink(linkLibraryElementId);

                // return true because request was performed succesfully
                return true;
            }

            // return false if the request was performed unsuccesfully
            return false;
        }


        /// <summary>
        /// This is a essentially static method that adds a presentation link to the library when given a presentation link model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> AddPresentationLinkToLibrary(PresentationLinkModel model)
        {
            // If there exists a presentation link between two element models, return and do not create a new one
            if (PresentationLinkViewModel.Models.FirstOrDefault(item => item.InElementId == model.InElementId && item.OutElementId == model.OutElementId) != null ||
                PresentationLinkViewModel.Models.FirstOrDefault(item => item.OutElementId == model.InElementId && item.InElementId == model.OutElementId) != null)
            {
                return false;
            }

            var isSuccess = false;
  
            Debug.Assert(PresentationLinkViewModel.Models != null, "this hashset of presentationlinkmodels should be statically instantiated");


            // create a new presentation link view
            //   var view = new PresentationLinkView(vm); //todo remove add to atom view list from presentation link view constructor
            //TODO use this collectionController stuff, check if the collection exists
            //var collectionController = SessionController.Instance.IdToControllers[model.ParentCollectionId] as ElementCollectionController;
            // Debug.Assert(collectionController != null, "the collectionController is not an element collection controller, check that parent collection id is being set correctly for presentation link models");
            //collectionController.AddChild(view);

            // Add the model to the list of models
            // create a new presentation link view model
            var vm = new PresentationLinkViewModel(model);
            PresentationLinkViewModel.Models.Add(vm.Model);

            var allContent = SessionController.Instance.ActiveFreeFormViewer.AllContent;
            var collectionViewModel = allContent.FirstOrDefault(item => (item.LibraryElementId == vm.Model.ParentCollectionId)) as GroupNodeViewModel;
            if (collectionViewModel != null)
            {
                collectionViewModel.Trails.Add(vm);
            }
            else
            {
             //   SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.ViewModel.Trails.Add(vm);
            }
            isSuccess = true;

            return isSuccess;
        }
    }
}