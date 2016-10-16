using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewPageContainer : RectangleUIElement
    {

        private LibraryElementController _currentController;

        private TabContainerUIElement<string> _pageTabContainer;

        private StackLayoutManager _tabContainerLayoutManager;

        public DetailViewPageContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _pageTabContainer = new TabContainerUIElement<string>(this, Canvas);
            _pageTabContainer.TabsIsCloseable = false;

            _tabContainerLayoutManager = new StackLayoutManager();
            _tabContainerLayoutManager.AddElement(_pageTabContainer);
            BorderWidth = 0;
            AddChild(_pageTabContainer);
        }

        /// <summary>
        /// Show a library element in the page container
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public void ShowLibraryElement(string libraryElementModelId)
        {
            // set the _currentController to the new Library element that is going to eb shown
            _currentController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);

            // clear all the old tabs
            _pageTabContainer.ClearTabs();

            // all types have a home and metadata
            _pageTabContainer.AddTab("home", "Home");
            _pageTabContainer.AddTab("metadata", "Metadata");

            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                    break;
                case NusysConstants.ElementType.Image:
                    _pageTabContainer.AddTab("regions", "Regions");

                    break;
                case NusysConstants.ElementType.Word:
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    break;
                case NusysConstants.ElementType.Collection:
                    break;
                case NusysConstants.ElementType.PDF:
                    _pageTabContainer.AddTab("regions", "Regions");

                    break;
                case NusysConstants.ElementType.Audio:
                    _pageTabContainer.AddTab("regions", "Regions");

                    break;
                case NusysConstants.ElementType.Video:
                    _pageTabContainer.AddTab("regions", "Regions");
                    break;
                case NusysConstants.ElementType.Tag:
                    break;
                case NusysConstants.ElementType.Web:
                    break;
                case NusysConstants.ElementType.Area:
                    break;
                case NusysConstants.ElementType.Link:
                    break;
                case NusysConstants.ElementType.Recording:
                    break;
                case NusysConstants.ElementType.Tools:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _pageTabContainer.AddTab("links", "Links");
            _pageTabContainer.AddTab("aliases","Aliases");
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _tabContainerLayoutManager.SetSize(Height, Width);
            _tabContainerLayoutManager.SetMargins(BorderWidth);
            _tabContainerLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _tabContainerLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _tabContainerLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
