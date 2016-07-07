using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class LinkEditorTabViewModel
    {
        public ObservableCollection<LinkTemplate> LinkTemplates { get; }
        public ObservableCollection<LibraryItemTemplate> LibraryElements { get; } 

        private ILinkable _linkable;

        public LinkEditorTabViewModel()
        {
            LinkTemplates = new ObservableCollection<LinkTemplate>();
            LibraryElements = new ObservableCollection<LibraryItemTemplate>();

            var idList = SessionController.Instance.ContentController.IdList;
            foreach (var id in idList)
            {
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
                if (controller.LibraryElementModel.Type != ElementType.Collection)
                {
                    var libraryElementTemplate = new LibraryItemTemplate(controller);
                    LibraryElements.Add(libraryElementTemplate);
                }
                
            }
            SessionController.Instance.ContentController.OnNewContent += ContentController_OnNewContent;
            SessionController.Instance.ContentController.OnElementDelete += ContentController_OnElementDelete;
            SessionController.Instance.LinkController.OnLinkRemoved += LinkController_OnLinkRemoved;
            SessionController.Instance.LinkController.OnNewLink += LinkController_OnNewLink;
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
            if (controller.LibraryElementModel.Type != ElementType.Collection)
            {
                var libraryElementTemplate = new LibraryItemTemplate(controller);
                UITask.Run(delegate {
                    LibraryElements.Add(libraryElementTemplate);
                });
            }
        }

        internal void DeleteLink(string id)
        {
            SessionController.Instance.ActiveFreeFormViewer.AllContent.First().Controller.RequestDeleteVisualLink(id);
            SessionController.Instance.LinkController.RemoveLink(id);
            Task.Run(async delegate
            {
                var request = new DeleteLibraryElementRequest(id);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });

            foreach (var template in LinkTemplates)
            {
                if (template.ID == id)
                {
                    LinkTemplates.Remove(template);
                    break;
                }
            }
        }

        private void LinkController_OnNewLink(LinkLibraryElementController link)
        {
            if (_linkable == null)
            {
                return;
            }
            if (_linkable.Id == link.LinkLibraryElementModel.InAtomId ||
                _linkable.Id == link.LinkLibraryElementModel.OutAtomId)
            {
                var template = new LinkTemplate(link, _linkable.Id);
                UITask.Run(async delegate {
                    LinkTemplates.Add(template);
                });
            }
        }

        private void LinkController_OnLinkRemoved(LinkLibraryElementController link)
        {
            if (_linkable == null)
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

        public void ChangeLinkTemplates(ILinkable linkable)
        {
            if (linkable == null)
            {
                return;
            }
            LinkTemplates.Clear();
            _linkable = linkable;
            foreach (var controller in linkable.GetAllLinks())
            {
                var template = new LinkTemplate(controller, linkable.Id);
                LinkTemplates.Add(template);
            }
            _linkable.LinkAdded += _linkable_LinkAdded;
            _linkable.LinkRemoved += _linkable_LinkRemoved;
        }

        private void _linkable_LinkRemoved(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void _linkable_LinkAdded(object sender, LinkLibraryElementController controller)
        {
            if (_linkable == null)
            {
                return;
            }
            var template = new LinkTemplate(controller, _linkable.Id);
            UITask.Run(delegate {
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

        public async void CreateLink(LinkId idToLinkTo, string title)
        {
            if (_linkable == null)
            {
                return;
            }
            _linkable.LinkAdded -= _linkable_LinkAdded;
            await _linkable.RequestAddNewLink(idToLinkTo, title);
            var linkId = SessionController.Instance.LinkController.GetLinkIdBetween(_linkable.Id, idToLinkTo);
            //var linkController = SessionController.Instance.ContentController.GetLibraryElementController(linkId) as LinkLibraryElementController;
            //linkController?.SetTitle(title);
            _linkable.LinkAdded += _linkable_LinkAdded;
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
