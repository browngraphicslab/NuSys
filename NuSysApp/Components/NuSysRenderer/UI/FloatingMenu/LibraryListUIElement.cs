using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;
using Buffer = Windows.Storage.Streams.Buffer;

namespace NuSysApp
{
    public class LibraryListUIElement : ResizeableWindowUIElement
    {

        /// <summary>
        /// The actual list view used to display the library
        /// </summary>
        public ListViewUIElementContainer<LibraryElementModel> LibraryListView;

        /// <summary>
        /// list of RectangleUIElements which are used to display icons while dragging
        /// </summary>
        private List<RectangleUIElement> _libraryDragElements;

        /// <summary>
        /// True if the drag icons are visible false otherwise
        /// </summary>
        private bool _isDragVisible;

        /// <summary>
        /// how much each of the dragged icons and dropped library elements will be offset from eachother in postive x and positive y pixel coordinates
        /// </summary>
        private float _itemDropOffset = 10;

        /// <summary>
        /// The add file button on the top right corner of the library list
        /// </summary>
        private TransparentButtonUIElement _addFileButton;

        private bool _dragCanceled;

        /// <summary>
        /// A dictionary of fileids to access types, static because the adding files methods have to be static
        /// </summary>
        private static Dictionary<string, NusysConstants.AccessType> _fileIdToAccessMap = new Dictionary<string, NusysConstants.AccessType>();

        /// <summary>
        /// Define the quality of the waveform
        /// </summary>
        public enum WaveFormQuality
        {
            /// <summary>
            /// Lowest Quality, somewhat blurry on one region level
            /// </summary>
            Low,
            /// <summary>
            /// Medium Quality, somewhat blurry on two region levels
            /// </summary>
            Medium,
            /// <summary>
            /// High Quality, somewhat blurry on three region levels
            /// </summary>
            High
        }

        /// <summary>
        /// Search bar for the LibraryListUIElement
        /// </summary>
        private ScrollableTextboxUIElement _searchBar;


        /// <summary>
        /// Filter button for activating the filter menu
        /// </summary>
        private ButtonUIElement _filterButton;


        /// <summary>
        /// the menu used for filtering library elements
        /// </summary>
        private FilterMenu _filterMenu { get; }

        /// <summary>
        /// The button that is used to activate the bing popup
        /// </summary>
        private ButtonUIElement _bingButton;
        /// <summary>
        /// This is the popup that executes a bing search
        /// </summary>
        private BingSearchPopup _bingSearchPopup;

        /// <summary>
        /// True if the library needs to be filtered
        /// </summary>
        private bool _filterIsDirty;

        /// <summary>
        /// ienumerable of selected controllers
        /// </summary>
        private IEnumerable<LibraryElementController> _previouslySelectedControllers;

        /// <summary>
        /// Sets the visibility of the library
        /// </summary>
        public bool IsVisible
        {
            get { return base.IsVisible; }
            set
            {
                ToggleVisibility(value);
            }
        }

        ///// <summary>
        ///// TEST BUTTON
        ///// </summary>
        //private RectangleButtonUIElement _testbutton;

        public LibraryListUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            // initialize the ui of the library listview
            InitializeLibraryList();
            // add the libary list view as a child
            AddChild(LibraryListView);

            //setup the bing button and it's popup
            _bingButton = new TransparentButtonUIElement(this, ResourceCreator)
            {
                ImageBounds = new Rect(.25, .25, .5, .5)
            };
            AddButton(_bingButton, TopBarPosition.Right);

            _addFileButton = new TransparentButtonUIElement(this, ResourceCreator, UIDefaults.PrimaryStyle)
            {
                ImageBounds = new Rect(.25, .25, .5, .5)
            };
            // add the addfile button to the window
            AddButton(_addFileButton, TopBarPosition.Right);

            // initialize the search bar
            _searchBar = new ScrollableTextboxUIElement(this, Canvas, false, true)
            {
                Height = UIDefaults.SearchBarHeight,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                FontSize = 14,
                BorderWidth = 1,
                BorderColor = Constants.MED_BLUE,
                Background = Colors.White,
                FontFamily = UIDefaults.TextFont
            };
            _searchBar.TextChanged += SearchBarTextChanged;
            AddChild(_searchBar);

            TopBarColor = Constants.LIGHT_BLUE;
            TopBarHeight = 50;
            Background = Colors.White;
            BorderColor = Constants.MED_BLUE;
            BorderWidth = 1;
            IsSnappable = true;

            ShowClosable();

            // initialize the filter button
            _filterButton = new RectangleButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "Filter")
            {
                Width = UIDefaults.FilterButtonWidth,
                Height = UIDefaults.SearchBarHeight,
            };
            AddChild(_filterButton);

            _filterMenu = new FilterMenu(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(_filterMenu);


            // initialize the list of library drag elements
            _libraryDragElements = new List<RectangleUIElement>();

            // add the add file button tapped event
            _addFileButton.Tapped += AddFileButtonTapped;

            // add dragging events
            LibraryListView.RowDragged += LibraryListView_RowDragged;
            LibraryListView.RowDragCompleted += LibraryListView_RowDragCompleted;
            LibraryListView.RowTapped += OnLibraryItemSelected;
            LibraryListView.RowDoubleTapped += LibraryListView_RowDoubleTapped;

            _filterButton.Tapped += OnFilterButtonTapped;
            _bingButton.Tapped += _bingButton_Tapped;
            BrushManager.BrushUpdated += BrushManager_BrushUpdated;

            _dragCanceled = false;

            // events so that the library list view adds and removes elements dynamically
            SessionController.Instance.ContentController.OnNewLibraryElement += UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete += UpdateLibraryListToRemoveElement;

            _previouslySelectedControllers = new List<LibraryElementController>();
        }

        /// <summary>
        /// Event handler for when the filter is changed in the filter menu
        /// </summary>
        /// <param name="controllersRemoved"></param>
        /// <param name="controllersAdded"></param>
        private void BrushManager_BrushUpdated(IEnumerable<LibraryElementController> controllersRemoved, IEnumerable<LibraryElementController> controllersAdded)
        {
            LibraryListView.FilterBy(ApplyFilter);
        }

        /// <summary>
        /// Event handler for when the text of the library search bar changes
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void SearchBarTextChanged(InteractiveBaseRenderItem item, string text)
        {
            _filterIsDirty = true;
        }

        /// <summary>
        /// Applies both the brush filter and the search filter to the library search function
        /// </summary>
        /// <param name="lem"></param>
        /// <returns></returns>
        private bool ApplyFilter(LibraryElementModel lem)
        {
            return ApplySearchTextFilter(lem) && ApplyBrushFilter(lem);
        }

        /// <summary>
        /// private method to apply the search text filter the library search function
        /// </summary>
        /// <param name="lem"></param>
        /// <returns></returns>
        private bool ApplySearchTextFilter(LibraryElementModel lem)
        {
            var search = _searchBar.Text.ToLower();
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            if (lem.Title.ToLower().Contains(search))
            {
                return true;
            }
            var creator =
                SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(lem.Creator);
            if (creator.ToLower().Contains(search))
            {
                return true;
            }

            if (lem.Type.ToString().ToLower().Contains(search))
            {
                return true;
            }
            if (lem.Keywords != null)
            {
                foreach (var tag in lem.Keywords)
                {
                    if (tag.Text.ToLower().Contains(search))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// private method to apply the brush filter the library search function
        /// </summary>
        /// <param name="lem"></param>
        /// <returns></returns>
        private bool ApplyBrushFilter(LibraryElementModel lem)
        {
            if (BrushManager.HasBrush)
            {
                return BrushManager.ControllersWithHighlight.Any(cont => cont.LibraryElementModel == lem);
            }
            return true;

        }

        private void OnFilterButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _filterMenu.ClearFilter();
            _filterMenu.IsVisible = !_filterMenu.IsVisible;
            _filterMenu.Height = 400;
            _filterMenu.Width = 200;
        }

        private void _bingButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _bingSearchPopup = new BingSearchPopup(this, Canvas);
            _bingSearchPopup.Width = 300;
            _bingSearchPopup.Transform.LocalPosition = new Vector2(Width-_bingSearchPopup.Width,_bingButton.Height);
            AddChild(_bingSearchPopup);
        }
        /// <summary>
        /// Fired whenever a row is selected, causes the session controller to fetch the content data model for that row
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void OnLibraryItemSelected(LibraryElementModel item, string columnName, CanvasPointer pointer, bool isSelected)
        {

            // first we just try to get the content data model for the element that was selected since that it is important for loading images
            if (!SessionController.Instance.ContentController.ContainsContentDataModel(item.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    if (item.Type == NusysConstants.ElementType.Collection)
                    {
                        var request = new GetEntireWorkspaceRequest(item.LibraryElementId);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                        Debug.Assert(request.WasSuccessful() == true);
                        await request.AddReturnedDataToSessionAsync();
                        await request.MakeCollectionFromReturnedElementsAsync();
                    }
                    else
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(item.ContentDataModelId);
                    }
                });
            }

            // then we get the list of controllers which are currently selected in the library
            var currentlySelectedControllers = LibraryListView.GetSelectedItems().Select(lem => SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId));

            // if there are any controllers, then we turn off highlighting //todo why!
            if (currentlySelectedControllers.Any())
            {
                BrushManager.SetBrushVisibility(false);
            }

            // then we determine which controllers have been deselected by querying all previously selected controllers except
            // the currently selected controllers. we remove the highlight from these deselected controllers
            var deselectedControllers = _previouslySelectedControllers.Except(currentlySelectedControllers);
            foreach(var controller in deselectedControllers)
            {
                controller?.RemoveHighlight();
            }

            // then we add a highlight to the currently selected controllers,
            //todo AS OF NOW, ADDING HIGHLIGHT MULTIPLE TIMES IS SAFE BUT THIS COULD BE DANGEROUS
            foreach(var controller in currentlySelectedControllers)
            {
                controller?.AddHighlight();
            }

            // now we set the previously selected controllers to the list of controllers which are currently selected
            _previouslySelectedControllers = new List<LibraryElementController>(currentlySelectedControllers);

            //todo what is this doing here. 
            if (!currentlySelectedControllers.Any())
            {
                BrushManager.SetBrushVisibility(true);
            }

        }


        private void LibraryListView_RowDoubleTapped(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(item.LibraryElementId);
            Debug.Assert(controller != null);
            if (controller == null)
            {
                return;
            }
            SessionController.Instance.NuSessionView.ShowDetailView(controller);

        }

        /// <summary>
        /// Loads any async resources we need
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _addFileButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/add elements.png"));
            _bingButton.Image =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/logo_bing_en-US.png"));
            base.Load(); 
        }

        /// <summary>
        /// Called when the addfile button is tapped, triggers the adding file sequence of events
        /// </summary>
        /// <param name="interactiveBaseRenderItem"></param>
        /// <param name="pointer"></param>
        private void AddFileButtonTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                AddFile();
            });

        }

        /// <summary>
        /// Fired when the drag event is completed, removes any drag icons from the display, and adds each of the dragged elements to the current collection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private async void LibraryListView_RowDragCompleted(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            if (_dragCanceled)
            {
                _dragCanceled = false;
                return;
            }
            // remove each of the drag elements
            foreach (var rect in _libraryDragElements)
            {
                RemoveChild(rect);
            }
            _libraryDragElements.Clear();
            _isDragVisible = false;

            // add each of the items to the collection
            foreach (var lem in LibraryListView.GetSelectedItems().ToArray())
            {
                var libraryElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);
                await
                    StaticServerCalls.AddElementToWorkSpace(pointer.CurrentPoint,
                            libraryElementController.LibraryElementModel.Type, libraryElementController)
                        .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Fired when a row is dragged from
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void LibraryListView_RowDragged(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            if (_dragCanceled)
            {
                return;
            }
            // if we are currently dragging
            if (_isDragVisible)
            {
                // simply move each of the element sto the new drag location
                var position = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix) + pointer.Delta;

                //If we are on the listview, "put the elements back"
                if (LibraryListView.HitTest(pointer.CurrentPoint) != null)
                {
                    // remove each of the drag elements
                    foreach (var rect in _libraryDragElements)
                    {
                        RemoveChild(rect);
                    }
                    _libraryDragElements.Clear();
                    _isDragVisible = false;
                    _dragCanceled = true;
                }
                else
                {
                    //Otherwise move each of the library drag elements
                    foreach (var element in _libraryDragElements)
                    {
                        element.Transform.LocalPosition = position + new Vector2(_itemDropOffset * _libraryDragElements.IndexOf(element));
                    }
                }

            }
            else
            {
                // set drag visible to true so future calls of this event do not reach this control flow branch
                _isDragVisible = true;

                // get the current position of the pointer relative to the local matrix
                var position = pointer.StartPoint;
                // convert the list of selected library element models from the libraryListView into a list of controllers
                var selectedControllers =
                    LibraryListView.GetSelectedItems()
                        .Select(
                            model =>
                                SessionController.Instance.ContentController.GetLibraryElementController(
                                    model.LibraryElementId))
                        .ToList();

                foreach (var controller in selectedControllers)
                {
                    var rect = new RectangleUIElement(this, ResourceCreator);
                    Task.Run(async delegate
                    {
                        rect.Image = await LoadCanvasBitmap(controller.SmallIconUri);
                        Debug.Assert(rect.Image is CanvasBitmap);
                        rect.Width = (float) (rect.Image as CanvasBitmap).SizeInPixels.Width/ (rect.Image as CanvasBitmap).SizeInPixels.Height * 100;
                        rect.Height = 100;


                    });
                    rect.Transform.LocalPosition = position + new Vector2(_itemDropOffset * selectedControllers.IndexOf(controller));
                    _libraryDragElements.Add(rect);
                    position += new Vector2(_itemDropOffset, _itemDropOffset);
                    AddChild(rect);
                }
            }
        }

        private async Task<ICanvasImage> LoadCanvasBitmap(Uri smallIconURI)
        {
            return await MediaUtil.LoadCanvasBitmapAsync(Canvas, smallIconURI);
        }

        /// <summary>
        /// Fired when the library list is closed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        protected override void CloseButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // hide the library
            ToggleVisibility(false);
            base.CloseButtonOnTapped(item, pointer);
        }

        /// <summary>
        /// Toggles the visibility of the library, call this method with no parameters if you want to actaully toggle the visibility
        /// call it with a value of true or false if you want to override the toggle and manually set the library's visibility to
        /// true or false
        /// </summary>
        /// <param name="value"></param>
        private void ToggleVisibility(bool? value = null)
        {
            // CAREFUL, WE HAVE OVERRIDE IsVisbile in this class. WITHIN THIS METHOD YOU MUST SET
            // base.IsVisible, DIRECTLY SETTING IsVisible IS A VERY EASY WAY OF ENTERING INFINITE LOOPS!
            // YOU HAVE BEEN WARNED!

            // if the passed in value is not null
            if (value.HasValue)
            {
                // set the base visibilty to the passed in value
                base.IsVisible = value.Value;
            }
            else
            {
                // otherwise toggle the base visibility
                base.IsVisible = !base.IsVisible;
            }

            if (_previouslySelectedControllers != null)
            {
                // if we are now visible, then readd the highlight from the previously selected controllers
                if (base.IsVisible)
                {
                    // otherwise remove the highlight from the previously selected controllers
                    foreach (var controller in _previouslySelectedControllers)
                    {
                        controller?.AddHighlight();
                    }
                }
                else
                {
                    // otherwise remove the highlight from the previously selected controllers
                    foreach (var controller in _previouslySelectedControllers)
                    {
                        controller?.RemoveHighlight();
                    }
                }


            }

        }

        public override void Dispose()
        {
            LibraryListView.RowDragged -= LibraryListView_RowDragged;
            LibraryListView.RowDragCompleted -= LibraryListView_RowDragCompleted;
            LibraryListView.RowTapped -= OnLibraryItemSelected;
            LibraryListView.RowDoubleTapped -= LibraryListView_RowDoubleTapped;

            _filterButton.Tapped -= OnFilterButtonTapped;


            SessionController.Instance.ContentController.OnNewLibraryElement -= UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete -= UpdateLibraryListToRemoveElement;
            _addFileButton.Tapped -= AddFileButtonTapped;

            _searchBar.TextChanged -= SearchBarTextChanged;

            _bingButton.Tapped -= _bingButton_Tapped;
            BrushManager.BrushUpdated -= BrushManager_BrushUpdated;
            base.Dispose();
        }

        /// <summary>
        /// Initialize the UI for the library list 
        /// </summary>
        public void InitializeLibraryList()
        {
            LibraryListView = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                MultipleSelections = false
               
            };

            var imgColumn = new LibraryListImageColumn<LibraryElementModel>(Canvas);
            imgColumn.Title = "";
            imgColumn.RelativeWidth = 1;
            imgColumn.ColumnFunction = model => model.GetController().SmallIconUri;


            var listColumn1 = new ListTextColumn<LibraryElementModel>();
            listColumn1.Title = "Title";
            listColumn1.RelativeWidth = 2;
            listColumn1.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Type";
            listColumn2.RelativeWidth = 1.25f;
            listColumn2.ColumnFunction = model => model.Type.ToString();

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Creator";
            listColumn3.RelativeWidth = 1;
            listColumn3.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn4 = new ListTextColumn<LibraryElementModel>();
            listColumn4.Title = "Last Edited Timestamp";
            listColumn4.RelativeWidth = 1.8f;
            listColumn4.ColumnFunction = model => model.GetController()?.GetLastEditedTimestampInMinutes(); //Trims the seconds portion of the timestamp

            var listColumn5 = new ListTextColumn<LibraryElementModel>();
            listColumn5.Title = "Tags";
            listColumn5.RelativeWidth = 1f;
            listColumn5.ColumnFunction = model => model.Keywords != null ? string.Join(", ", model.Keywords.Select(i => i.Text)) : "";

            var listColumn6= new ListTextColumn<LibraryElementModel>();
            listColumn6.Title = "Parent";
            listColumn6.RelativeWidth = 1f;
            listColumn6.ColumnFunction = model => SessionController.Instance.ContentController.GetLibraryElementController(model.ParentId) != null? SessionController.Instance.ContentController.GetLibraryElementController(model.ParentId).Title : "" ;

            var listColumn7 = new ListTextColumn<LibraryElementModel>();
            listColumn7.Title = "Creation Date";
            listColumn7.RelativeWidth = 1f;
            listColumn7.ColumnFunction = model => model.GetController().GetCreationTimestampInMinutes();

            var listColumn8 = new ListTextColumn<LibraryElementModel>();
            listColumn8.Title = "Access";
            listColumn8.RelativeWidth = 1f;
            listColumn8.ColumnFunction = model => model.AccessType.ToString();

            LibraryListView.AddColumns(new List<ListColumn<LibraryElementModel>> { imgColumn, listColumn1, listColumn2, listColumn3, listColumn4 });

            LibraryListView.AddColumnOptions(new List<ListColumn<LibraryElementModel>> {listColumn5, listColumn8, listColumn7,listColumn6 });

            LibraryListView.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());

            BorderWidth = 5;
            BorderColor = Colors.Black;
            TopBarColor = Colors.Azure;
            Width = 500;
            Height = 400;
            MinWidth = 500;
            MinHeight = 400;


        }

        /// <summary>
        /// Removes an element from the libary list when it is deleted
        /// </summary>
        /// <param name="element"></param>
        private void UpdateLibraryListToRemoveElement(LibraryElementModel element)
        {
            Debug.Assert(element != null);
            LibraryListView?.RemoveItems(new List<LibraryElementModel> {element});
        }

        /// <summary>
        /// Adds an element to the library list when it is added to the library
        /// </summary>
        /// <param name="libraryElement"></param>
        private void UpdateLibraryListWithNewElement(LibraryElementModel libraryElement)
        {
            LibraryListView.AddItems(new List<LibraryElementModel> {libraryElement});
            LibraryListView.ScrollTo(libraryElement);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            LibraryListView.Width = Width - 2 * BorderWidth;
            LibraryListView.Height = Height - TopBarHeight - BorderWidth - UIDefaults.SearchBarHeight;
            LibraryListView.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);
            _searchBar.Width = Width - 2*BorderWidth - UIDefaults.FilterButtonWidth;
            _searchBar.Transform.LocalPosition = new Vector2(BorderWidth, Height - BorderWidth - UIDefaults.SearchBarHeight);
            _filterButton.Transform.LocalPosition = new Vector2(BorderWidth + _searchBar.Width, Height - BorderWidth - UIDefaults.SearchBarHeight);
            _filterMenu.Transform.LocalPosition = new Vector2(Width, 0);

            if (_filterIsDirty)
            {
                //Finally, filter by the search function
                LibraryListView.FilterBy(ApplyFilter);
                _filterIsDirty = false;
            }

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Returns a list of library element model ids for the files which were just added to the library
        /// </summary>
        /// <param name="storageFiles"></param>
        /// <returns></returns>
        public static async Task<List<LibraryElementController>> AddFile(IReadOnlyList<StorageFile> storageFiles = null)
        {

            // clear the fileIdToAccessMap
            _fileIdToAccessMap.Clear();
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            NusysConstants.ElementType elementType = NusysConstants.ElementType.Text;
            string data = string.Empty;
            string title = string.Empty;
            // a list of strings containing pdf text for each page
            List<string> pdfTextByPage = new List<string>();
            int pdfPageCount = 0;
            double aspectRatio = 0;

            var addedLibraryElemControllers = new List<LibraryElementController>();

            if (storageFiles == null)
            {
                storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);
            }

            // get the fileAddedAclsPopup from the session view
            var fileAddedAclsPopup = SessionController.Instance.SessionView.FileAddedAclsPopup;

            // get a mapping of the acls for all of the storage files using the fileAddedAclsPopup
            var tempfileIdToAccessMaps = await fileAddedAclsPopup.GetAcls(storageFiles);

            if (tempfileIdToAccessMaps == null) //if the user canceled the document import
            {
                return addedLibraryElemControllers;
            }

            foreach (var fileAccess in tempfileIdToAccessMaps)
            {
                _fileIdToAccessMap.Add(fileAccess.Key, fileAccess.Value);
            }

            // check if the user has canceled the upload
            if (_fileIdToAccessMap == null)
            {
                return addedLibraryElemControllers;
            }

            foreach (var storageFile in storageFiles ?? new List<StorageFile>())
            {
                if (storageFile == null)
                    return addedLibraryElemControllers;

                var contentId = SessionController.Instance.GenerateId();

                var fileType = storageFile.FileType.ToLower();
                title = storageFile.DisplayName;

                bool validFileType = true;
                // Create a thumbnail dictionary mapping thumbnail sizes to the byte arrays.
                // Note that only video and images are to get thumbnails this way, currently.
                var thumbnails = new Dictionary<NusysConstants.ThumbnailSize, string>();
                thumbnails[NusysConstants.ThumbnailSize.Small] = string.Empty;
                thumbnails[NusysConstants.ThumbnailSize.Medium] = string.Empty;
                thumbnails[NusysConstants.ThumbnailSize.Large] = string.Empty;

                if (Constants.ImageFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Image;
                    var bytes = await MediaUtil.StorageFileToByteArray(storageFile);
                    data = Convert.ToBase64String(bytes);

                    var thumb = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
                    aspectRatio = thumb.OriginalWidth / (double)thumb.OriginalHeight;

                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.WordFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Word;

                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    data = Convert.ToBase64String(fileBytes);
                }
                else if (Constants.PowerpointFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Powerpoint;
                }
                else if (Constants.PdfFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.PDF;

                    // create a list of strings which contain the image data for each page of the pdf
                    List<string> pdfPages = new List<string>();

                    // read the contents of storageFile into a MUPDF Document
                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    data = Convert.ToBase64String(fileBytes);
                    var MuPdfDoc = await MediaUtil.DataToPDF(data);

                    pdfPageCount = MuPdfDoc.PageCount;

                    // get variables for drawing the page
                    var pageSize = MuPdfDoc.GetPageSize(0);
                    var width = pageSize.X;
                    var height = pageSize.Y;

                    // create an image to use for converting
                    var image = new WriteableBitmap(width, height);

                    // create a buffer to draw the page on
                    IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                    buf.Length = image.PixelBuffer.Length;

                    // draw the page onto the buffer
                    MuPdfDoc.DrawPage(0, buf, 0, 0, width, height, false);
                    var ss = buf.AsStream();

                    // copy the buffer to the image
                    await ss.CopyToAsync(image.PixelBuffer.AsStream());
                    image.Invalidate();

                    // save the image as a file (temporarily)
                    var x = await image.SaveAsync(NuSysStorages.SaveFolder);

                    thumbnails = await MediaUtil.GetThumbnailDictionary(x);

                    // delete the image file that we saved
                    await x.DeleteAsync(StorageDeleteOption.PermanentDelete);

                }
                else if (Constants.VideoFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Video;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    var thumb = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
                    aspectRatio = thumb.OriginalWidth / (double)thumb.OriginalHeight;

                    data = Convert.ToBase64String(fileBytes);
                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.AudioFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Audio;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    var frameWorkWaveForm = GetWaveFormFrameWorkElement(fileBytes);
                    thumbnails = await GetThumbnailsFromFrameworkElement(frameWorkWaveForm);

                    // override the largest thumbnail for higher resolution
                    thumbnails[NusysConstants.ThumbnailSize.Large] =
                        await GetImageAsStringFromFrameworkElement(frameWorkWaveForm);

                    data = Convert.ToBase64String(fileBytes);
                }
                else
                {
                    validFileType = false;
                }
                if (validFileType)
                {
                    CreateNewContentRequestArgs args;
                    //if there is pdf text, add it to the request
                    if (pdfTextByPage.Any())
                    {
                        args = new CreateNewPdfContentRequestArgs
                        {
                            PdfText = JsonConvert.SerializeObject(pdfTextByPage),
                            PageCount = pdfPageCount
                        };
                        pdfTextByPage.Clear();
                    }
                    else
                    {
                        args = new CreateNewContentRequestArgs();
                    }
                    args.ContentId = contentId;
                    args.DataBytes = data;

                    //add the extension if there is one
                    if (fileType != null)
                    {
                        args.FileExtension = fileType;
                    }

                    CreateNewLibraryElementRequestArgs libraryElementArgs;
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Image:
                            var imageArgs = new CreateNewImageLibraryElementRequestArgs();
                            imageArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = imageArgs;
                            break;
                        case NusysConstants.ElementType.Word:
                            var wordArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                            wordArgs.PdfPageStart = 0;
                            wordArgs.PdfPageEnd = pdfPageCount;
                            wordArgs.AspectRatio = aspectRatio;
                            wordArgs.NormalizedHeight = 1;
                            wordArgs.NormalizedWidth = 1;
                            libraryElementArgs = wordArgs;
                            break;
                        case NusysConstants.ElementType.PDF:
                            var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                            pdfArgs.PdfPageStart = 0;
                            pdfArgs.PdfPageEnd = pdfPageCount;
                            pdfArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = pdfArgs;
                            break;
                        case NusysConstants.ElementType.Video:
                            var videoArgs = new CreateNewVideoLibraryElementRequestArgs();
                            videoArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = videoArgs;
                            break;
                        case NusysConstants.ElementType.Audio:
                            libraryElementArgs = new CreateNewAudioLibraryElementRequestArgs();
                            break;
                        default:
                            libraryElementArgs = new CreateNewLibraryElementRequestArgs();
                            break;
                    }

                    args.LibraryElementArgs = libraryElementArgs;

                    //add the three thumbnails
                    args.LibraryElementArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                    args.LibraryElementArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                    args.LibraryElementArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];

                    args.LibraryElementArgs.Title = title;
                    args.LibraryElementArgs.LibraryElementType = elementType;

                    // add the acls from the map, default to private instead of throwing an error on release
                    Debug.Assert(_fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId), "The mapping from the fileAddedPopup is not being output or set correctly");
                    if (_fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId))
                    {
                        args.LibraryElementArgs.AccessType = _fileIdToAccessMap[storageFile.FolderRelativeId];
                        args.LibraryElementArgs.LibraryElementId = SessionController.Instance.GenerateId();
                    }
                    else
                    {
                        args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Private;
                    }

                    var request = new CreateNewContentRequest(args);

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

                    // if we succesfully complete the request
                    if (request.AddReturnedLibraryElementToLibrary())
                    {
                        // add the library element controller of the new content to the list we are going to return
                        addedLibraryElemControllers.Add(SessionController.Instance.ContentController.GetLibraryElementController(args.LibraryElementArgs.LibraryElementId));
                        Task.Run(async delegate
                        {
                            await Task.Delay(250);//TODO def don't do this, first fix issue #1595
                            var controller = SessionController.Instance.ContentController?.GetLibraryElementController(args?.LibraryElementArgs?.LibraryElementId);
                            if (controller != null)
                            {
                                SessionController.Instance.NuSessionView?.Library?.LibraryListView?.ScrollTo(controller.LibraryElementModel);
                                SessionController.Instance.NuSessionView?.Library?.LibraryListView?.SelectItem(controller.LibraryElementModel);
                            }
                        });
                    }
                    

                    vm.ClearSelection();
                }
                else
                {
                    Debug.WriteLine("tried to import invalid filetype");
                }

                _fileIdToAccessMap.Remove(storageFile.FolderRelativeId);
            }

            return addedLibraryElemControllers;
        }

        /// <summary>
        /// Converts Audio into a Framework element representing its waveform. The quality level defaults to low but
        /// can be set higher if desired.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        private static FrameworkElement GetWaveFormFrameWorkElement(Byte[] bytes, WaveFormQuality quality = WaveFormQuality.Low)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();

            WaveStream waveStream = new MediaFoundationReaderUniversal(stream);
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
            waveStream.Position = 0;
            int bytesRead = 1;
            int samplesPerPixel = 1024;

            if (waveStream.TotalTime.TotalMinutes > 15)
            {
                samplesPerPixel = (int)Math.Pow(2, 15);
            }
            else if (waveStream.TotalTime.TotalMinutes > 8)
            {
                samplesPerPixel = (int)Math.Pow(2, 14);
            }
            else if (waveStream.TotalTime.TotalMinutes > 5)
            {
                samplesPerPixel = (int)Math.Pow(2, 13);
            }
            else if (waveStream.TotalTime.TotalMinutes > 3)
            {
                samplesPerPixel = (int)Math.Pow(2, 12);
            }
            else if (waveStream.TotalTime.TotalMinutes > 0.5)
            {
                samplesPerPixel = (int)Math.Pow(2, 11);
            }

            // change the quality of the waveform by affecting the number of samples used
            switch (quality)
            {
                case WaveFormQuality.Low:
                    break;
                case WaveFormQuality.Medium:
                    samplesPerPixel /= 2;
                    break;
                case WaveFormQuality.High:
                    samplesPerPixel /= 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quality), quality, "The quality level is not supported here");
            }


            byte[] waveData = new byte[samplesPerPixel * bytesPerSample];
            var visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel * bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100 * (highPercent);
                line.Y2 = 100 * (lowPercent);
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 1;
                x++;
                visualGrid.Children.Add(line);
            }
            visualGrid.Height = 100;
            visualGrid.Width = x;
            Line middleLine = new Line();
            middleLine.X1 = 0;
            middleLine.X2 = x;
            middleLine.Y1 = visualGrid.Height / 2;
            middleLine.Y2 = visualGrid.Height / 2;

            middleLine.Stroke = new SolidColorBrush(Colors.Crimson);
            middleLine.StrokeThickness = 1;
            visualGrid.Children.Add(middleLine);

            return visualGrid;
        }

        /// <summary>
        /// Returns the thumbnails from a Framework element
        /// </summary>
        /// <param name="frameWorkElement"></param>
        /// <returns></returns>
        private static async Task<Dictionary<NusysConstants.ThumbnailSize, string>> GetThumbnailsFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Windows.UI.Xaml.Controls.Canvas.SetTop(frameWorkElement, -frameWorkElement.Height * 2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int)frameWorkElement.Width, (int)frameWorkElement.Height);

            // remove the visual grid from the canvas
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(frameWorkElement);

            // create a buffer from the rendered bitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // create a WriteableBitmap with desired width and height
            var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);

            // write the pixels to the bitmap
            using (Stream bitmapStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(pixels, 0, pixels.Length);
            }

            // save the writeable bitmap to a file
            var tempFile = await writeableBitmap.SaveAsync(NuSysStorages.SaveFolder);

            // get the thumbnails from the image file
            var thumbnails = await MediaUtil.GetThumbnailDictionary(tempFile);

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            return thumbnails;
        }

        /// <summary>
        /// Pass in a framework element and render a 1 : 1 pixel representation image of it.
        /// Returns the image as a string for use in JSON
        /// </summary>
        /// <param name="frameWorkElement"></param>
        /// <returns>A string representation of the passed in framework element as an image</returns>
        private static async Task<string> GetImageAsStringFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Windows.UI.Xaml.Controls.Canvas.SetTop(frameWorkElement, -frameWorkElement.Height * 2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int)frameWorkElement.Width, (int)frameWorkElement.Height);

            // remove the visual grid from the canvas
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(frameWorkElement);

            // create a buffer from the rendered bitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // create a WriteableBitmap with desired width and height
            var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);

            // write the pixels to the bitmap
            using (Stream bitmapStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(pixels, 0, pixels.Length);
            }

            // save the writeable bitmap to a file
            var tempFile = await writeableBitmap.SaveAsync(NuSysStorages.SaveFolder);

            // use the system to convert the file to a string
            var imageAsString = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(tempFile));

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            // return the string representation of the image
            return imageAsString;
        }


    }
}