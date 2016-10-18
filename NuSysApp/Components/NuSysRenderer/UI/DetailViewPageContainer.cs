﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewPageContainer : RectangleUIElement
    {

        private LibraryElementController _currentController;

        private TabContainerUIElement<DetailViewPageUIElement> _pageTabContainer;

        private TextboxUIElement _titleTextBox;

        private StackLayoutManager _tabContainerLayoutManager;

        private StackLayoutManager _titleLayoutManager;


        public DetailViewPageContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _pageTabContainer = new TabContainerUIElement<DetailViewPageUIElement>(this, Canvas);
            _pageTabContainer.TabsIsCloseable = false;

            _titleTextBox = new TextboxUIElement(this, Canvas)
            {
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left
            };

            _titleLayoutManager = new StackLayoutManager();
            _titleLayoutManager.AddElement(_titleTextBox);
            _tabContainerLayoutManager = new StackLayoutManager();
            _tabContainerLayoutManager.AddElement(_pageTabContainer);
            BorderWidth = 0;
            AddChild(_pageTabContainer);
            AddChild(_titleTextBox);
        }

        /// <summary>
        /// Show a library element in the page container
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public void ShowLibraryElement(string libraryElementModelId)
        {
            // set the _currentController to the new Library element that is going to eb shown
            _currentController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);

            _titleTextBox.Text = _currentController.Title;

            // clear all the old tabs
            _pageTabContainer.ClearTabs();

            // all types have a home and metadata
            _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Home), "Home");
            _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Metadata), "Metadata");

            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                    break;
                case NusysConstants.ElementType.Image:
                    _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Region), "Regions");

                    break;
                case NusysConstants.ElementType.Word:
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    break;
                case NusysConstants.ElementType.Collection:
                    break;
                case NusysConstants.ElementType.PDF:
                    _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Region), "Regions");

                    break;
                case NusysConstants.ElementType.Audio:
                    _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Region), "Regions");

                    break;
                case NusysConstants.ElementType.Video:
                    _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Region), "Regions");
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

            _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Links), "Links");
            _pageTabContainer.AddTab(new DetailViewPageUIElement(libraryElementModelId, DetailViewPageType.Aliases), "Aliases");
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            var titleHeight = 25;

            _titleLayoutManager.SetSize(Width, Height);
            _titleLayoutManager.SetMargins(BorderWidth);
            _titleLayoutManager.VerticalAlignment = VerticalAlignment.Top;
            _titleLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _titleLayoutManager.ItemHeight = titleHeight;
            _titleLayoutManager.ArrangeItems();


            _tabContainerLayoutManager.SetSize(Width, Height);
            _tabContainerLayoutManager.SetMargins(BorderWidth);
            _tabContainerLayoutManager.TopMargin = titleHeight  + 2 * BorderWidth;
            _tabContainerLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _tabContainerLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _tabContainerLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
