﻿using System;
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

        private ILinkTabable _linkTabable;

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
            //SessionController.Instance.LinksController.OnLinkRemoved += LinkController_OnLinkRemoved;
            //SessionController.Instance.LinksController.OnNewLink += LinkController_OnNewLink;
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
            //SessionController.Instance.ActiveFreeFormViewer.AllContent.First().Controller.RequestDeleteVisualLink(id);
            //SessionController.Instance.LinksController.RemoveLink(id);
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

        private void LinkTabableLinkRemoved(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void LinkTabableLinkAdded(object sender, LinkLibraryElementController controller)
        {
            if (_linkTabable == null)
            {
                return;
            }
            var template = new LinkTemplate(controller, _linkTabable.ContentId);
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

        public async void CreateLink(string idToLinkTo, string title)
        {
            if (_linkTabable == null)
            {
                return;
            }
            _linkTabable.LinkAdded -= LinkTabableLinkAdded;
            await _linkTabable.RequestAddNewLink(idToLinkTo, title);
            //var linkId = SessionController.Instance.LinksController.GetLinkIdBetween(_linkTabable.ContentId, idToLinkTo);
            //var linkController = SessionController.Instance.ContentController.GetLibraryElementController(linkId) as LinkLibraryElementController;
            //linkController?.SetTitle(title);
            _linkTabable.LinkAdded += LinkTabableLinkAdded;
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
