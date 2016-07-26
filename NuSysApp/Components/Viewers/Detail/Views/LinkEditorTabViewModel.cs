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

        private ILinkTabable _linkTabable;

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
            
            SessionController.Instance.ContentController.OnNewContent += ContentController_OnNewContent;
            SessionController.Instance.ContentController.OnElementDelete += ContentController_OnElementDelete;

        }

        private void ContentController_OnElementDelete(LibraryElementModel element)
        {
            foreach (var item in new HashSet<LibraryItemTemplate>(LibraryElements))
            {
                if (item.ContentID == element.LibraryElementId)
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

        internal void DeleteLink(string linkId)
        {
            // Rmeoves link from the linkTabable open in the DV
            _linkTabable.RequestRemoveLink(linkId);

            //Removes the link from the content at the other end of the Link
            var linkModel = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
            if (linkModel?.InAtomId == _linkTabable.ContentId)
            {
                var otherController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel?.OutAtomId);
                otherController?.RequestRemoveLink(linkId);
            } else if (linkModel?.OutAtomId == _linkTabable.ContentId)
            {
                var otherController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel?.InAtomId);
                otherController?.RequestRemoveLink(linkId);
            }

            //Create templates to display in the list view
            foreach (var template in LinkTemplates)
            {
                if (template.ID == linkId)
                {
                    LinkTemplates.Remove(template);
                    break;
                }
            }
        }

        private void LinkController_OnNewLink(LinkLibraryElementController link)
        {
            if (_linkTabable == null)
            {
                return;
            }
            if (_linkTabable.ContentId == link.LinkLibraryElementModel.InAtomId ||
                _linkTabable.ContentId == link.LinkLibraryElementModel.OutAtomId)
            {
                var template = new LinkTemplate(link, _linkTabable.ContentId);
                UITask.Run(async delegate {
                    LinkTemplates.Add(template);
                });
            }
        }

        private void LinkController_OnLinkRemoved(LinkLibraryElementController link)
        {
            if (_linkTabable == null)
            {
                return;
            }
            foreach (var template in new HashSet<LinkTemplate>(LinkTemplates))
            {
                if (template.Title == link.LinkLibraryElementModel.Title)
                {
                    LinkTemplates.Remove(template);
                }
            }
        }

        public void ChangeLinkTemplates(ILinkTabable linkTabable)
        {
            if (linkTabable == null)
            {
                return;
            }
            LinkTemplates.Clear();
            _linkTabable = linkTabable;
            foreach (var controller in linkTabable.GetAllLinks())
            {
                var template = new LinkTemplate(controller, linkTabable.ContentId);
                LinkTemplates.Add(template);
            }
            _linkTabable.LinkAdded += LinkTabableLinkAdded;
            _linkTabable.LinkRemoved += LinkTabableLinkRemoved;
        }

        private void LinkTabableLinkRemoved(object sender, string linkLibraryElementID)
        {
            if (_linkTabable == null)
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

        private void LinkTabableLinkAdded(object sender, LinkLibraryElementController controller)
        {
            if (_linkTabable == null)
            {
                return;
            }
            var template = new LinkTemplate(controller, _linkTabable.ContentId);
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

        public async void CreateLink(string idToLinkTo, string title, HashSet<Keyword> keywords = null)
        {
            if (_linkTabable == null)
            {
                return;
            }
            _linkTabable.LinkAdded -= LinkTabableLinkAdded;
            await _linkTabable.RequestAddNewLink(idToLinkTo, title);
            var newLinkController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerBetweenContent(
                _linkTabable.ContentId, idToLinkTo);
            if (newLinkController != null)
            {
                var template = new LinkTemplate(newLinkController, _linkTabable.ContentId);
                if (keywords != null)
                {
                    newLinkController.SetKeywords(keywords);
                }
                LinkTemplates.Add(template);
                //var linkId = SessionController.Instance.LinksController.GetLinkIdBetween(_linkTabable.ContentId, idToLinkTo);
                //var linkController = SessionController.Instance.ContentController.GetLibraryElementController(linkId) as LinkLibraryElementController;
                //linkController?.SetTitle(title);
                _linkTabable.LinkAdded += LinkTabableLinkAdded;
            }
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
