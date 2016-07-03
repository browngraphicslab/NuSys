using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkEditorTabViewModel
    {
        public ObservableCollection<LinkTemplate> LinkTemplates = new ObservableCollection<LinkTemplate>();

        private ILinkable _linkable;

        public LinkEditorTabViewModel()
        {
            SessionController.Instance.LinkController.OnLinkRemoved += LinkController_OnLinkRemoved;
            SessionController.Instance.LinkController.OnNewLink += LinkController_OnNewLink;
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
                LinkTemplates.Add(template);
            }
        }

        private void LinkController_OnLinkRemoved(LinkLibraryElementController link)
        {
            if (_linkable == null)
            {
                return;
            }
            foreach (var template in LinkTemplates)
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
            
        }
    }
}
