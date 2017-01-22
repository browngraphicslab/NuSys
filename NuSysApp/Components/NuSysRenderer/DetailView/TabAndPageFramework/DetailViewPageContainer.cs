using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// "Bundles common features of detail view pages" -lmurray
    /// </summary>
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
        private LockableTextBoxUIElement _titleBox;

        private ButtonUIElement _settingsButton;

        /// <summary>
        /// True if the page has called the Load method
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// Popup used to change the access of regions
        /// </summary>
        private FlyoutPopup _changeAccessPopup;

        /// <summary>
        /// The flyout instance used for the settings popup.  
        /// Will re-instantiate every time the settings button is pressed;
        /// </summary>
        private FlyoutPopup _settingsPopup;


        /// a decorative line
        /// </summary>
        private RectangleUIElement _line;

        /// <summary>
        /// popup used to display when the user changes collection settings to a more public version
        /// </summary>
        private ConfirmationPopupUIElement _collectionSettingChangedPopup;

        /// <summary>
        /// the private counter to keep track of the number of library elements requested to be shown.
        /// This will be used to end asynchronous tasks early if they are no longer the most recent reuqest.
        /// </summary>
        private int _shownLibraryElementCount;

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

        /// <summary>
        /// Event handler for when the settings button is pressed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void SettingsButton_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_settingsPopup != null)
            {
                _settingsPopup.DismissPopup();
                return;
            }

            _settingsPopup = new FlyoutPopup(this, Canvas);
            _settingsPopup.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X - _settingsPopup.Width/2,
                _settingsButton.Transform.LocalPosition.Y + _settingsButton.Height);
            _settingsPopup.AddFlyoutItem("Scroll To", OnScrollToFlyoutTapped, Canvas);
            _settingsPopup.AddFlyoutItem("Copy", OnCopyFlyoutTapped, Canvas);
            _settingsPopup.AddFlyoutItem("Change Access", OnChangeAccessFlyoutTapped, Canvas);
            if (this._currentController.LibraryElementModel.Type == NusysConstants.ElementType.Collection)
            {
                _settingsPopup.AddFlyoutItem("Change Collection Settings", CollectionSettingsOnTapped, Canvas);
            }
            _settingsPopup.AddFlyoutItem("Delete", OnDeleteFlyoutTapped, Canvas);
            _settingsPopup.Dismissed += SettingsPopupOnDismissed;
            AddChild(_settingsPopup);
        }

        /// <summary>
        /// Event handler called whenever the settings popup is dismissed;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="popupUiElement"></param>
        private void SettingsPopupOnDismissed(object sender, EventArgs args)
        {
            _settingsPopup.Dismissed -= SettingsPopupOnDismissed;
            _settingsPopup = null;
        }

        /// <summary>
        /// Event handler called when the collections settings flyout button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void CollectionSettingsOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var popup = new CollectionSettingsPopup(this, Canvas,_currentController as CollectionLibraryElementController);
            AddChild(popup);
            popup.Height = Height/4;
            popup.Width = Width/2;
            popup.Transform.LocalPosition = new Vector2(Width/2 - popup.Width/2, Height/2 - popup.Height/2);
            if (_settingsPopup != null)
            {
                _settingsPopup.DismissPopup();
            }            
        }


        /// <summary>
        /// Called whenever the change access option is tapped in the flyout setting menu
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnChangeAccessFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // create the change Access menu
            _changeAccessPopup = new FlyoutPopup(this, Canvas);

            _changeAccessPopup.Transform.LocalPosition = new Vector2(_settingsButton.Transform.LocalPosition.X - _changeAccessPopup.Width / 2,
                _settingsButton.Transform.LocalPosition.Y + _settingsButton.Height);

            if (_currentController.LibraryElementModel.AccessType == NusysConstants.AccessType.Private)
            {
                _changeAccessPopup.AddFlyoutItem("Make Read Only", OnReadOnlyTapped, Canvas);
            }
            if (_currentController.LibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly ||
                _currentController.LibraryElementModel.AccessType == NusysConstants.AccessType.Private)
            {
                _changeAccessPopup.AddFlyoutItem("Make Public", OnPublicTapped, Canvas);
            }

            if (_currentController.LibraryElementModel.AccessType == NusysConstants.AccessType.Public)
            {
                _changeAccessPopup.AddFlyoutItem("Cannot Change Access", OnCannotChangeAccessTapped, Canvas);
            }

            AddChild(_changeAccessPopup);

        }

        /// <summary>
        /// Method callewd when the change access cannot change access option is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnCannotChangeAccessTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _changeAccessPopup.DismissPopup();
        }

        /// <summary>
        /// Method called when the change access public option is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnPublicTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_currentController.LibraryElementModel.Type == NusysConstants.ElementType.Collection)
            {
                _collectionSettingChangedPopup = new ConfirmationPopupUIElement(this, ResourceCreator,
                    ConfirmMakeCollectionPublicTapped, null)
                {
                    Message =
                        $"Making a collection public will make all the nodes within it public as well. Would you like to continue?"
                };
                AddChild(_collectionSettingChangedPopup);

            }
            else
            {
                _currentController.SetAccessType(NusysConstants.AccessType.Public);
                _changeAccessPopup.DismissPopup();
            }
        }

        /// <summary>
        /// Fired when the user confirms that they would like to make a collection public
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ConfirmMakeCollectionPublicTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // change the access type of the current collection to public
            _currentController.SetAccessType(NusysConstants.AccessType.Public);

            // get all the element controllers
            foreach (var kvp in SessionController.Instance.ElementModelIdToElementController)
            {
                var elementController = kvp.Value;

                // if the element controller is in the current collection
                if (elementController.Model.ParentCollectionId ==
                    _currentController.LibraryElementModel.LibraryElementId)
                {
                    // if the elemtn controller has the wrong access type
                    if (elementController.LibraryElementModel.AccessType == NusysConstants.AccessType.Private)
                    {
                        // get the library element controller and change the access type 
                        elementController.LibraryElementController.SetAccessType(NusysConstants.AccessType.Public);
                    }
                }
            }

        }

        /// <summary>
        /// Fired when the user confirms that they woudl like to make a collection read only
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ConfirmMakeCollectionReadOnlyTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // change the access type of the current collection to read only
            _currentController.SetAccessType(NusysConstants.AccessType.ReadOnly);

            // get all the element controllers
            foreach (var kvp in SessionController.Instance.ElementModelIdToElementController)
            {
                var elementController = kvp.Value;

                // if the element controller is in the current collection
                if (elementController.Model.ParentCollectionId ==
                    _currentController.LibraryElementModel.LibraryElementId)
                {
                    // if the elemtn controller has the wrong access type
                    if (elementController.LibraryElementModel.AccessType == NusysConstants.AccessType.Private)
                    {
                        // get the library element controller and change the access type 
                        elementController.LibraryElementController.SetAccessType(NusysConstants.AccessType.ReadOnly);
                    }
                }
            }
        }

        /// <summary>
        /// Method called when the change access read only option is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnReadOnlyTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_currentController.LibraryElementModel.Type == NusysConstants.ElementType.Collection)
            {
                _collectionSettingChangedPopup = new ConfirmationPopupUIElement(this, ResourceCreator,
                    ConfirmMakeCollectionReadOnlyTapped, null)
                {
                    Message =
                        $"Making a collection readonly will make all the nodes within it readonly as well. Would you like to continue?"
                };
                AddChild(_collectionSettingChangedPopup);
            }
            else
            {
                _currentController.SetAccessType(NusysConstants.AccessType.ReadOnly);
                _changeAccessPopup.DismissPopup();
            }
        }

        /// <summary>
        /// Called whenever the copy option is tapped in the flyout setting menu
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnCopyFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.CreateDeepCopy(_currentController.LibraryElementModel.LibraryElementId);
        }

        /// <summary>
        /// Called whenever the delete option is tapped in the flyout setting menu
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void OnDeleteFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if the thing we are deleting was a linke
            if (_currentController.LibraryElementModel.Type ==NusysConstants.ElementType.Link)
            {
                await SessionController.Instance.LinksController.RemoveLink(_currentController.LibraryElementModel.LibraryElementId);
                if (SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.BtnDelete.IsVisible == true)
                {
                    SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.BtnDelete.IsVisible = false;
                }
            } else
            {
                // if the thing we are deleting is the collection we are currently in, do not delete
                if (_currentController.LibraryElementModel ==
                    SessionController.Instance.CurrentCollectionLibraryElementModel)
                {
                    var deleteFailed = new CenteredPopup(SessionController.Instance.NuSessionView, Canvas, "You cannot delete the collection you are currently in!");
                    SessionController.Instance.NuSessionView.AddChild(deleteFailed);
                }
                else
                {
                    DeleteLibraryElementRequest request =
                        new DeleteLibraryElementRequest(_currentController.LibraryElementModel.LibraryElementId);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    if (request.WasSuccessful() == true)
                    {
                        request.DeleteLocally();

                    }
                }
            }

            //Dismisses the flyout popup
            var popup = item.Parent as FlyoutPopup;
            Debug.Assert(popup != null);
            if (popup == null)
            {
                return;
            }

            popup.DismissPopup();
        }

        /// <summary>
        /// Called whenever the scroll to option is tapped in the flyout setting menu
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnScrollToFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.NuSessionView.Library.IsVisible = true;
            SessionController.Instance.NuSessionView.Library.LibraryListView.ScrollTo(_currentController.LibraryElementModel);
            SessionController.Instance.NuSessionView.Library.LibraryListView.SelectItem(_currentController.LibraryElementModel);
        }

        public override async Task Load()
        {
            //todo figure out why ScrollableTextbox breaks if you put these in the constructor. 
            _titleBox = new LockableTextBoxUIElement(this, Canvas, null, false, false)
            {
                Height = 50,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                FontSize = 30,
                FontFamily = UIDefaults.TitleFont
            };
            AddChild(_titleBox);
            _titleBox.TextChanged += OnTitleTextChanged;
            _titleBox.Transform.LocalPosition = new Vector2(_titleBox.Transform.LocalPosition.X + 30, _titleBox.Transform.LocalPosition.Y);

            _settingsButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/settings icon.png"));

            /// a decorative line :)
            _line = new RectangleUIElement(this, Canvas)
            {
                Background = Constants.MED_BLUE,
                Height = 1
            };
            _line.Width = Width - 20;
            AddChild(_line);
            _line.Transform.LocalPosition = new Vector2(10, _titleBox.Transform.LocalPosition.Y + _titleBox.Height);

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
            _shownLibraryElementCount++;
            ShowTabType(tabType,_shownLibraryElementCount);
        }

        /// <summary>
        /// private async method for showing the tab type
        /// </summary>
        /// <param name="tabType"></param>
        /// <returns></returns>
        private async Task ShowTabType(DetailViewPageTabType tabType, int requestNumber)
        {
            var controllerId = _currentController.LibraryElementModel.LibraryElementId;
            var rect = await DetailViewPageFactory.GetPage(this, Canvas, tabType.Type, _currentController);
            if (requestNumber == _shownLibraryElementCount)
            {
                if (rect != null && controllerId == _currentController.LibraryElementModel.LibraryElementId)
                {
                    rect.Height = this.Height;
                    rect.Width = this.Width;
                    _pageTabContainer.SetPage(rect);
                    OnPageTabChanged?.Invoke(_currentController.LibraryElementModel.LibraryElementId, tabType);
                }
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
        private async void OnTitleTextChanged(InteractiveBaseRenderItem item, string text)
        {
            _currentController.TitleChanged -= OnCurrentControllerTitleChanged;
            await UITask.Run(() =>
            {
                _currentController.SetTitle(text);
            });
            _currentController.TitleChanged += OnCurrentControllerTitleChanged;
        }

        /// <summary>
        /// Show a library element in the page container
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        public async Task ShowLibraryElement(string libraryElementModelId, DetailViewPageTabType pageToShow)
        {
            _shownLibraryElementCount++;
            int requestNumber = _shownLibraryElementCount;
            // if we are already showing the library elment model that was selected then just return
            if (!_loaded)
            {
                return;
            }

            if (_currentController != null)
            {
                _currentController.TitleChanged -= OnCurrentControllerTitleChanged;
            }

            // set the _currentController to the new Library element that is going to eb shown
            _currentController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);

            // set the title of the title box but don't send a request to the server
            _titleBox.TextChanged -= OnTitleTextChanged;
            _titleBox.Text = _currentController.Title;
            _titleBox.TextChanged += OnTitleTextChanged;

            _titleBox.SetNewId(_currentController.LibraryElementModel.LibraryElementId);

            if (_currentController.LibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly)
            {
                _titleBox.IsEditable = _currentController.LibraryElementModel.Creator == WaitingRoomView.UserID;
            }
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

            if (requestNumber == _shownLibraryElementCount)
            {
                // show the passed in page on the detail viewer
                await ShowTabType(pageToShow, requestNumber);
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_loaded)
            {
                _titleBox.Transform.LocalPosition = new Vector2(BorderWidth);
                _titleBox.Width = Width - 2*BorderWidth - _settingsButton.Width;

                _settingsButton.Transform.LocalPosition = new Vector2(Width - _settingsButton.Width - BorderWidth, BorderWidth);
                _settingsButton.ImageBounds = new Rect(.25, .25, .5, .5);

                _pageTabContainer.Page.Height = Height;
                _pageTabContainer.Page.Width = Width;

                _tabContainerLayoutManager.SetSize(Width, Height);
                _tabContainerLayoutManager.SetMargins(BorderWidth);
                _tabContainerLayoutManager.TopMargin = _titleBox.Height + BorderWidth + 10;
                _tabContainerLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
                _tabContainerLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
                _tabContainerLayoutManager.ArrangeItems();

                _line.Width = Width - 20;
                _line.Transform.LocalPosition = new Vector2(10, _titleBox.Transform.LocalPosition.Y + _titleBox.Height);
            }



            base.Update(parentLocalToScreenTransform);
        }
    }
}
