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
        /// <summary>
        /// The current library elment controller being displayed
        /// </summary>
        private LibraryElementController _currentController;

        /// <summary>
        /// The tab container used to display the different page types, regions, home, metadata etc.
        /// </summary>
        private TabContainerUIElement<DetailViewPageTabType> _pageTabContainer;

        /// <summary>
        /// layout manager used to make sure the _pageTabContainer fills the window
        /// </summary>
        private StackLayoutManager _tabContainerLayoutManager;

        public delegate void OnDetailViewPageTabChanged(string libraryElementId , DetailViewPageTabType page);

        public event OnDetailViewPageTabChanged OnPageTabChanged;

        private TextboxUIElement _titleBox;

        private TransparentButtonUIElement _settingsButton;

        public DetailViewPageContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _pageTabContainer = new TabContainerUIElement<DetailViewPageTabType>(this, Canvas)
            {
                TabsIsCloseable = false,
                TabHorizontalAlignment = HorizontalAlignment.Stretch,
                TabTextAlignment = CanvasHorizontalAlignment.Center,
                TabColor = Colors.DarkSlateGray,
                TabSpacing = 25
            };

            _titleBox = new TextboxUIElement(this, Canvas)
            {
                Height = 100,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 30,
                Wrapping = CanvasWordWrapping.Character
            };
            AddChild(_titleBox);

            _settingsButton = new TransparentButtonUIElement(this, Canvas, UIDefaults.SecondaryStyle, "Settings");
            AddChild(_settingsButton);

            _tabContainerLayoutManager = new StackLayoutManager();
            _tabContainerLayoutManager.AddElement(_pageTabContainer);
            BorderWidth = 0;
            AddChild(_pageTabContainer);



            _pageTabContainer.OnCurrentTabChanged += ShowPageType;
        }

        public override async Task Load()
        {
            _settingsButton.Image = _settingsButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/gear.png"));
        }

        public override void Dispose()
        {
            _pageTabContainer.OnCurrentTabChanged -= ShowPageType;
            base.Dispose();
        }

        /// <summary>
        /// Called whenever the current tab is changed in the page container
        /// </summary>
        /// <param name="tabType"></param>
        private async void ShowPageType(DetailViewPageTabType tabType)
        {
            var rect = await DetailViewPageFactory.GetPage(this, Canvas, tabType.Type, _currentController);
            if (rect != null)
            {
                _titleBox.Text = _currentController.Title;
                _pageTabContainer.SetPage(rect);
                OnPageTabChanged?.Invoke(_currentController.LibraryElementModel.LibraryElementId, tabType);
            }
        }

        /// <summary>
        /// Show a library element in the page container
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public void ShowLibraryElement(string libraryElementModelId, DetailViewPageTabType pageToShow)
        {
            // if we are already showing the library elment model that was selected then just return
            if (_currentController?.LibraryElementModel.LibraryElementId == libraryElementModelId)
            {
                return;
            }

            // set the _currentController to the new Library element that is going to eb shown
            _currentController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);

            // clear all the old tabs
            _pageTabContainer.ClearTabs();

            // all types have a home and metadata
            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Home), "Home", false);
            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Metadata), "Metadata", false);

            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                    break;
                case NusysConstants.ElementType.Image:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Region), "Regions", false);

                    break;
                case NusysConstants.ElementType.Word:
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    break;
                case NusysConstants.ElementType.Collection:
                    break;
                case NusysConstants.ElementType.PDF:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Region), "Regions", false);

                    break;
                case NusysConstants.ElementType.Audio:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Region), "Regions", false);

                    break;
                case NusysConstants.ElementType.Video:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Region), "Regions", false);
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

            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Links), "Links", false);
            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Aliases), "Aliases", false);

            // show the passed in page on the detail viewer
            ShowPageType(pageToShow);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _titleBox.Transform.LocalPosition = new Vector2(BorderWidth);
            _titleBox.Width = Width - 2*BorderWidth;
            _titleBox.Height = 50;

            _tabContainerLayoutManager.SetSize(Width, Height);
            _tabContainerLayoutManager.SetMargins(BorderWidth);
            _tabContainerLayoutManager.TopMargin = _titleBox.Height + BorderWidth;
            _tabContainerLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _tabContainerLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _tabContainerLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
