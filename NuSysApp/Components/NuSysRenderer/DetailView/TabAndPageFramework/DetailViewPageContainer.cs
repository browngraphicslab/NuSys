using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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

        /// <summary>
        /// The title of the library element
        /// </summary>
        private ScrollableTextboxUIElement _titleBox;

        private ButtonUIElement _settingsButton;

        /// <summary>
        /// True if the page has called the Load method
        /// </summary>
        private bool _loaded;

        public DetailViewPageContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _pageTabContainer = new TabContainerUIElement<DetailViewPageTabType>(this, Canvas)
            {
                TabsIsCloseable = false,
                TabHorizontalAlignment = HorizontalAlignment.Stretch,
                TabTextAlignment = CanvasHorizontalAlignment.Center,
                TabColor = Constants.MED_BLUE,
                Background = Constants.LIGHT_BLUE,
                TabBarBackground = Constants.LIGHT_BLUE,
                TabHeight = 40,
                TabSpacing = 25,
                TitleColor = Colors.White
            };

            _settingsButton = new ButtonUIElement(this, resourceCreator)
            {
                Width = 50,
                Height = 50,
            };
            AddChild(_settingsButton);

            _tabContainerLayoutManager = new StackLayoutManager();
            _tabContainerLayoutManager.AddElement(_pageTabContainer);
            BorderWidth = 0;
            AddChild(_pageTabContainer);

            _settingsButton.Pressed += SettingsButton_Pressed;

            _pageTabContainer.OnCurrentTabChanged += ShowPageType;
        }

        private void SettingsButton_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var settingsPopup = new FlyoutPopup(this, Canvas);
            settingsPopup.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X - settingsPopup.Width/2,
                _settingsButton.Transform.LocalPosition.Y + _settingsButton.Height);
            settingsPopup.AddFlyoutItem("Scroll To", null, Canvas);
            settingsPopup.AddFlyoutItem("Delete", null, Canvas);
            settingsPopup.AddFlyoutItem("Copy", null, Canvas);
            settingsPopup.AddFlyoutItem("Change Access", null, Canvas);

            AddChild(settingsPopup);
        }

        public override async Task Load()
        {

            //todo figure out why ScrollableTextbox breaks if you put these in the constructor. 
            _titleBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Height = 50,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 30,
                FontFamily = UIDefaults.FontFamily
            };
            AddChild(_titleBox);
            _titleBox.TextChanged += OnTitleTextChanged;

            _settingsButton.Image =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/settings icon.png"));

             _loaded = true;
            base.Load();
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
                _pageTabContainer.SetPage(rect);
                OnPageTabChanged?.Invoke(_currentController.LibraryElementModel.LibraryElementId, tabType);
            }
        }

        /// <summary>
        /// Fired whenever the current controllers title changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCurrentControllerTitleChanged(object sender, string e)
        {
            _titleBox.TextChanged -= OnTitleTextChanged;
            _titleBox.Text = e;
            _titleBox.TextChanged += OnTitleTextChanged;
        }

        /// <summary>
        /// Fired whenever the user changes the text of the title
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void OnTitleTextChanged(InteractiveBaseRenderItem item, string text)
        {
            _currentController.TitleChanged -= OnCurrentControllerTitleChanged;
            _currentController.SetTitle(text);
            _currentController.TitleChanged += OnCurrentControllerTitleChanged;
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

            if (_currentController != null)
            {
                _currentController.TitleChanged -= OnCurrentControllerTitleChanged;
            }

            // set the _currentController to the new Library element that is going to eb shown
            _currentController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            _titleBox.Text = _currentController.Title;
            _currentController.TitleChanged += OnCurrentControllerTitleChanged;
            

            // clear all the old tabs
            _pageTabContainer.ClearTabs();

            // all types have a home and metadata
            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Home), "Home", false);
            _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Metadata), "Metadata", false);

            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Image:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Region), "Regions", false);
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
            }
            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                case NusysConstants.ElementType.Image:
                case NusysConstants.ElementType.Collection:
                case NusysConstants.ElementType.PDF:
                case NusysConstants.ElementType.Audio:
                case NusysConstants.ElementType.Video:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Links), "Links", false);
                    break;
            }

            switch (_currentController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                case NusysConstants.ElementType.Image:
                case NusysConstants.ElementType.Collection:
                case NusysConstants.ElementType.PDF:
                case NusysConstants.ElementType.Audio:
                case NusysConstants.ElementType.Video:
                    _pageTabContainer.AddTab(new DetailViewPageTabType(DetailViewPageType.Aliases), "Aliases", false);
                    break;
            }

            // show the passed in page on the detail viewer
            ShowPageType(pageToShow);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_loaded)
            {
                _titleBox.Transform.LocalPosition = new Vector2(BorderWidth);
                _titleBox.Width = Width - 2*BorderWidth - _settingsButton.Width;

                _settingsButton.Transform.LocalPosition = new Vector2(Width - _settingsButton.Width - BorderWidth, BorderWidth);
                _settingsButton.ImageBounds = new Rect(_settingsButton.Width/4, _settingsButton.Height/4, _settingsButton.Width/2, _settingsButton.Height/2);

                _tabContainerLayoutManager.SetSize(Width, Height);
                _tabContainerLayoutManager.SetMargins(BorderWidth);
                _tabContainerLayoutManager.TopMargin = _titleBox.Height + BorderWidth;
                _tabContainerLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
                _tabContainerLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
                _tabContainerLayoutManager.ArrangeItems();
            }


            base.Update(parentLocalToScreenTransform);
        }
    }
}
