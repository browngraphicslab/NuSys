using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using NusysIntermediate;

namespace NuSysApp
{
    public class LinkEditorTabViewModel
    {
        public ObservableCollection<LinkTemplate> LinkTemplates { get; }
        public ObservableCollection<LibraryItemTemplate> LibraryElements { get; } 

        /// <summary>
        /// The library element id of the library element the detail viewer is currently open to
        /// </summary>
        public string CurrentLibraryElementId;

        public LinkEditorTabViewModel()
        {
            LinkTemplates = new ObservableCollection<LinkTemplate>();
            LibraryElements = new ObservableCollection<LibraryItemTemplate>();


            // populates the to link to list with all the library element models
            var idList = SessionController.Instance.ContentController.IdList;
            foreach (var id in idList)
            {
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
                if (controller.LibraryElementModel.Type != NusysConstants.ElementType.Collection)
                {
                    var libraryElementTemplate = new LibraryItemTemplate(controller);
                    LibraryElements.Add(libraryElementTemplate);
                }
                
            }
            
            SessionController.Instance.ContentController.OnNewLibraryElement += ContentController_OnNewContent;
            SessionController.Instance.ContentController.OnLibraryElementDelete += ContentControllerOnLibraryElementDelete;

        }

        private void ContentControllerOnLibraryElementDelete(LibraryElementModel element)
        {
            foreach (var item in new HashSet<LibraryItemTemplate>(LibraryElements))
            {
                if (item.LibraryElementId == element.LibraryElementId)
                {
                    LibraryElements.Remove(item);
                }
            }
        }

        private void ContentController_OnNewContent(LibraryElementModel element)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId);
            if (controller.LibraryElementModel.Type != NusysConstants.ElementType.Collection)
            {
                var libraryElementTemplate = new LibraryItemTemplate(controller);
                UITask.Run(delegate {
                    LibraryElements.Add(libraryElementTemplate);
                });
            }
        }

        /// <summary>
        /// Deletes the link associated with the passed in linkLibraryElementId
        /// </summary>
        /// <param name="linkLibraryElementId"></param>
        /// <returns>true if the link was deleted succesfully false otherwise</returns>
        internal async Task<bool> DeleteLink(string linkLibraryElementId)
        {
            // delete the link using the SessionController
            var success = await SessionController.Instance.LinksController.RemoveLink(linkLibraryElementId);

            // the link was deleted succesfully
            if (success)
            {
                //Remove all instances of the link from the list view 
                foreach (var template in LinkTemplates)
                {
                    if (template.ID == linkLibraryElementId)
                    {
                        LinkTemplates.Remove(template);
                        break; // break because there shoudl only be one instance of the link
                    }
                }

                // return true to show that the link was deleted succesfully
                return true;
            }

            // return false to show that the link was not deleted successfully
            return false;
        }

        public void ChangeLinkTemplates(string newLibraryElementModelId)
        {
            if (newLibraryElementModelId == null)
            {
                return;
            }
            LinkTemplates.Clear();
            CurrentLibraryElementId = newLibraryElementModelId;
            var linkController = SessionController.Instance.ContentController.GetLibraryElementController(CurrentLibraryElementId);
            foreach (var controller in linkController.GetAllLinks())
            {
                var template = new LinkTemplate(controller, CurrentLibraryElementId);
                LinkTemplates.Add(template);
            }
            linkController.LinkAdded += LinkTabableLinkAdded;
            linkController.LinkRemoved += LinkTabableLinkRemoved;
        }

        private void LinkTabableLinkRemoved(object sender, string linkLibraryElementID)
        {
            if (CurrentLibraryElementId == null)
            {
                return;
            }
            foreach (var template in new HashSet<LinkTemplate>(LinkTemplates))
            {
                if (template.ID == linkLibraryElementID)
                {
                    LinkTemplates.Remove(template);
                }
            }
        }

        private void LinkTabableLinkAdded(object sender, LinkViewModel linkViewModel)
        {
            if (CurrentLibraryElementId == null)
            {
                return;
            }
            var template = new LinkTemplate((LinkLibraryElementController)linkViewModel.Controller.LibraryElementController, CurrentLibraryElementId);
            UITask.Run(delegate {
            foreach (var existingTemplate in LinkTemplates)
            {
                if (existingTemplate.ID == template.ID)
                {
                    return;
                }
            }

            LinkTemplates.Add(template);
            });
        }

        public void SortByTitle()
        {
            if (LinkTemplates.Count < 1)
            {
                return;
            }

            List<LinkTemplate> list = new List<LinkTemplate>(LinkTemplates.OrderBy(template => template.Title));

            LinkTemplates.Clear();
            foreach (var linkTemplate in list)
            {
                LinkTemplates.Add(linkTemplate);
            }

        }

        public async Task CreateLinkAsync(string idToLinkTo)
        {
            // don't link to itself or if the CurrentLibraryElemtnID is null
            if (CurrentLibraryElementId == null || CurrentLibraryElementId == idToLinkTo)
            {
                return;
            }
            var currentLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(CurrentLibraryElementId);
            var toLinkLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(idToLinkTo);
            currentLibraryElementController.LinkAdded -= LinkTabableLinkAdded;
            

            var createNewLinkLibraryElementRequestArgs = new CreateNewLinkLibraryElementRequestArgs();
            createNewLinkLibraryElementRequestArgs.LibraryElementModelInId = CurrentLibraryElementId;
            createNewLinkLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Link;
            createNewLinkLibraryElementRequestArgs.LibraryElementId = SessionController.Instance.GenerateId();
            createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId = idToLinkTo;
            createNewLinkLibraryElementRequestArgs.Title = $"Link from {currentLibraryElementController.Title} to {toLinkLibraryElementController.Title}";

            var contentRequestArgs = new CreateNewContentRequestArgs();
            contentRequestArgs.LibraryElementArgs = createNewLinkLibraryElementRequestArgs;
            var request = new CreateNewContentRequest(contentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();


            var newLinkController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(createNewLinkLibraryElementRequestArgs.LibraryElementId); 
            var template = new LinkTemplate(newLinkController, CurrentLibraryElementId);

            LinkTemplates.Add(template);
            currentLibraryElementController.LinkAdded += LinkTabableLinkAdded;
        }
        public void SortByLinkedTo()
        {
            if (LinkTemplates.Count < 1)
            {
                return;
            }

            List<LinkTemplate> list = new List<LinkTemplate>(LinkTemplates.OrderBy(template => template.LinkedTo));

            LinkTemplates.Clear();
            foreach (var linkTemplate in list)
            {
                LinkTemplates.Add(linkTemplate);
            }

        }

        
    }
}
