using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using WinRTXamlToolkit.Controls.DataVisualization;

namespace NuSysApp
{
    /// <summary>
    /// "Common class that bundles common features" -lmurray
    /// </summary>
    public abstract class DetailViewPage : RectangleUIElement
    {
        /// <summary>
        /// The library element controller associated with this region page
        /// </summary>
        private LibraryElementController _controller;

        /// <summary>
        /// Rectangle holding the content of the region page
        /// </summary>
        private RectangleUIElement _contentRect;

        /// <summary>
        /// the stack layout manager managing the layout of the image on the window
        /// </summary>
        private StackLayoutManager _contentLayoutManager;

        /// <summary>
        /// The add region button
        /// </summary>
        private ButtonUIElement _addRegionButton;

        /// <summary>
        /// The width of the add region button
        /// </summary>
        private float _addRegionButtonWidth = 150;

        /// <summary>
        /// The _analysis ui element associated with the regions page
        /// </summary>
        private ImageAnalysisUIElement _analysisUIElement;

        /// <summary>
        /// The layout manager for the analysis ui element
        /// </summary>
        private StackLayoutManager _imageAnalysisLayoutManager;

        /// <summary>
        /// True if the page should show image analysis, false otherwise
        /// </summary>
        private bool _showsImageAnalysis;

        /// <summary>
        /// True if page should should regions, false otherwise
        /// </summary>
        private bool _showRegions;

        private float _imageHeight;

        private float _imageAnalysisMinHeight = 230;

        private ButtonUIElement _dragToCollectionButton;

        /// <summary>
        /// Rectangle used to display icon of dragged to collection element
        /// </summary>
        private RectangleUIElement _dragRect;

        /// <summary>
        /// popup menu used to display add region messages to the user
        /// </summary>
        private FlyoutPopup _addRegionPopup;

        /// <summary>
        /// button to expand the detail view page, should only happen if the element is an image or a PDF
        /// </summary>
        private RectangleButtonUIElement _expandButton;
        
        /// <summary>
        /// button to open word from the detail view if the element is a word element
        /// </summary>
        private RectangleButtonUIElement _wordButton;

        /// <summary>
        /// text that says where the element's origin is from
        /// </summary>
        private MarkdownConvertingTextbox _originWords;

        /// <summary>
        /// if this element has an origin, then this will be set to the origin, otherwise it is null
        /// </summary>
        private LibraryElementController _origin;

        protected DetailViewPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller, bool showsImageAnalysis, bool showRegions) : base(parent, resourceCreator)
        {
            // set the controller properly
            _controller = controller;

            // initialize _contentLayoutManager
            _contentLayoutManager = new StackLayoutManager();

            // set image analysis support
            _showsImageAnalysis = showsImageAnalysis;

            // set region support
            _showRegions = showRegions;

            // initialize the add region button and the _addRegionButtonLayoutManager
            _addRegionButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "Add Region")
            {
                Width=150,
                Height = 40
            };
            AddChild(_addRegionButton);

            /// add the analysis stuff only if it is supported
            if (_showsImageAnalysis)
            {
                // initialize the layout manager for the analysis ui element
                _imageAnalysisLayoutManager = new StackLayoutManager();

                _imageAnalysisLayoutManager.SetMargins(15);

                // initialize the analysis ui element
                _analysisUIElement = new ImageAnalysisUIElement(this, resourceCreator, controller);
                AddChild(_analysisUIElement);
                _imageAnalysisLayoutManager.AddElement(_analysisUIElement);
            }

            _dragRect = new RectangleUIElement(this, resourceCreator)
            {
                IsVisible = false
            };
            AddChild(_dragRect);            

            _dragToCollectionButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.DraggableStyle,
                "Drag to Collection")
            {
                BorderColor = Constants.DARK_BLUE,
                BorderWidth = 2
            };
            _dragToCollectionButton.Width = 150;
            _dragToCollectionButton.Height = 40;
            AddChild(_dragToCollectionButton);

            if (controller.LibraryElementModel.Type == NusysConstants.ElementType.Image ||
                controller.LibraryElementModel.Type == NusysConstants.ElementType.PDF ||
                controller.LibraryElementModel.Type == NusysConstants.ElementType.Video)
            {
                _expandButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "Expand");
                _expandButton.Width = 150;
                _expandButton.Height = 40;
                AddChild(_expandButton);

                _expandButton.Tapped += ExpandButton_Tapped;
            }

            if (controller.LibraryElementModel.Type == NusysConstants.ElementType.Word)
            {
                _wordButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "Open Word");
                _wordButton.Width = 150;
                _wordButton.Height = 40;
                AddChild(_wordButton);

                _wordButton.Tapped += WordButtonOnTapped;
            }

            _originWords = new MarkdownConvertingTextbox(this, Canvas)
            {
                Background = Colors.Transparent,
                Height = 40,
                Width = 500,
                Scrollable = false,
                TextColor = Constants.DARK_BLUE,
                Text = "this item had its origin deleted.",
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
            };
            AddChild(_originWords);



            // set the tapped method on the addRegionButton
            _addRegionButton.Tapped += AddRegionButton_Tapped;
            _dragToCollectionButton.DragCompleted += _dragToCollectionButton_DragCompleted;
            _dragToCollectionButton.DragStarted += _dragToCollectionButton_DragStarted;
            _dragToCollectionButton.Dragged += _dragToCollectionButton_Dragged;
            _originWords.Tapped += ElementOrigin_Tapped;
        }

        /// <summary>
        /// navigates to the origin of the element - location in library if it is the original element, the source if it is a copy, or the parent if it is a region
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ElementOrigin_Tapped(TextboxUIElement sender, Vector2 position)
        {
            if (_origin != null)
            {
                SessionController.Instance.NuSessionView.ShowDetailView(_origin);
            }
        }

        private async void WordButtonOnTapped(ButtonUIElement button)
        {
            Debug.Assert(_controller.LibraryElementModel.Type == NusysConstants.ElementType.Word);
            var request = new GetWordDocumentRequest(new GetWordDocumentRequestArgs() {ContentId =  _controller.LibraryElementModel.ContentDataModelId});
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            Debug.Assert(request.WasSuccessful() == true);
            if (request.WasSuccessful() == true)
            {
                await UITask.Run(async delegate{ 
                    var bytes = request.GetReturnedDocumentBytes();
                    var path = _controller.LibraryElementModel.ContentDataModelId + ".docx";
                    var folder = ApplicationData.Current.LocalFolder;
                    var fullPath = folder.Path + "\\" + path;

                    await Task.Run(async delegate
                    {
                        ApplicationData.Current.LocalFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
                        try
                        {
                            File.WriteAllBytes(fullPath, bytes);
                        }
                        catch (Exception e)
                        {
                            //do nothing
                        }
                    });

                    var launcherOptions = new LauncherOptions() { UI = { PreferredPlacement = Placement.Right, InvocationPoint = new Point(SessionController.Instance.SessionView.ActualWidth / 2, 0.0) } };
                    launcherOptions.TreatAsUntrusted = false;
                    launcherOptions.PreferredApplicationDisplayName = "NUSYS";
                    launcherOptions.PreferredApplicationPackageFamilyName = "NuSys";
                    launcherOptions.DesiredRemainingView = ViewSizePreference.UseHalf;

                    await Task.Run(async delegate
                    {
                        var storageFile = await StorageFile.GetFileFromPathAsync(fullPath);
                        File.SetAttributes(fullPath, System.IO.FileAttributes.Normal);
                        await Launcher.LaunchFileAsync(storageFile, launcherOptions);
                    });
                });
            }
        }

        /// <summary>
        /// overwritten in image and pdf classes so you can expand depending on what you're looking at
        /// </summary>
        protected virtual void ExpandButton_Tapped(ButtonUIElement button)
        {
            
        }

        /// <summary>
        /// Fired 60 times a second while the pointer is being dragged after tapping on drag to collection button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _dragToCollectionButton_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dragRect.Transform.LocalPosition = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix) + pointer.Delta;
        }

        /// <summary>
        /// Fired once when the pointer is first dragged after tapping on the drag to collection button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void _dragToCollectionButton_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dragRect.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, _controller.SmallIconUri);
            var width = (float)(_dragRect.Image as CanvasBitmap).SizeInPixels.Width / (_dragRect.Image as CanvasBitmap).SizeInPixels.Height * 100;
            var height = 100;

            if (height > 0 && width > 0)
            {
                _dragRect.Height = height;
                _dragRect.Width = width;
            }
            else
            {
                _dragRect.Width = 100;
                _dragRect.Height = 100;
            }

            _dragRect.IsVisible = true;
        }

        /// <summary>
        /// Fired once when the pointer stops dragging after tapping on the drag to collection button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void _dragToCollectionButton_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            await StaticServerCalls.AddElementToCurrentCollection(pointer.CurrentPoint, _controller.LibraryElementModel.Type, _controller);
            _dragRect.Image = null;
            _dragRect.IsVisible = false;
        }

        /// <summary>
        /// Called whenever the add region button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddRegionButton_Tapped(ButtonUIElement addRegionButton)
        {
            Debug.Assert(addRegionButton!= null);
            _addRegionPopup = new FlyoutPopup(this, Canvas);
            _addRegionPopup.Transform.LocalPosition = new Vector2(addRegionButton.Transform.LocalPosition.X,
                addRegionButton.Transform.LocalPosition.Y + addRegionButton.Height);
            _addRegionPopup.AddFlyoutItem("Public", OnAddPublicRegionFlyoutTapped, Canvas);
            _addRegionPopup.AddFlyoutItem("Private", OnAddPrivateRegionFlyoutTapped, Canvas);
            AddChild(_addRegionPopup);
        }

        /// <summary>
        /// Method called when adding a private region flyout is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnAddPrivateRegionFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            AddRegion(NusysConstants.AccessType.Private);
            _addRegionPopup.DismissPopup();
        }

        /// <summary>
        /// Method called when adding a public region flyout is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnAddPublicRegionFlyoutTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            AddRegion(NusysConstants.AccessType.Public);
            _addRegionPopup.DismissPopup();
        }


        /// <summary>
        /// The dispose method, remove events here, dispose of objects here
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _contentLayoutManager.Dispose();

            _addRegionButton.Tapped -= AddRegionButton_Tapped;
            _dragToCollectionButton.DragCompleted -= _dragToCollectionButton_DragCompleted;
            _dragToCollectionButton.DragStarted -= _dragToCollectionButton_DragStarted;
            _dragToCollectionButton.Dragged -= _dragToCollectionButton_Dragged;

            if (_showsImageAnalysis)
            {
                _imageAnalysisLayoutManager.Dispose();
            }

            if (_expandButton != null)
            {
                _expandButton.Tapped -= ExpandButton_Tapped;
            }

            if (_wordButton != null)
            {
                _wordButton.Tapped -= WordButtonOnTapped;
            }

            _originWords.Tapped -= ElementOrigin_Tapped;

            base.Dispose();
        }

        private void UpdateOriginText()
        {
            if (_controller.LibraryElementModel.Origin.Type == LibraryElementOrigin.OriginType.Copy)
            {
                var originalElement =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        _controller.LibraryElementModel.Origin.OriginId);
                if (originalElement != null)
                {
                    _originWords.Text = "this item is a copy of ";
                    _originWords.Text += "__"+originalElement.Title+"__";
                }
                _origin = originalElement;
            }
            else if (_controller.LibraryElementModel.Origin.Type == LibraryElementOrigin.OriginType.LibraryImport)
            {
                _originWords.Text = "this item was imported directly to the library";
            }
            else if (_controller.LibraryElementModel.Origin.Type == LibraryElementOrigin.OriginType.Region)
            {
                var originalElement =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        _controller.LibraryElementModel.Origin.OriginId);
                if (originalElement != null)
                {
                    _originWords.Text = "this item is a region of ";
                    _originWords.Text +="__"+ originalElement.Title+"__";
                }
                _origin = originalElement;
            }
            else
            {
                _originWords.Text = "this item had an origin that was deleted.";
            }
        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // get the image height for use in laying out the image on top of the image analysis
            var heightMultiplier = _showsImageAnalysis ? .75f : .9f;

            //set image height based on other ui elements
            _imageHeight = Math.Min(Height - _imageAnalysisMinHeight - _contentLayoutManager.TopMargin - _dragToCollectionButton.Height, Height * heightMultiplier);

            // set image height if the expand button is present
            if (_expandButton != null)
            {
                _imageHeight = Math.Min(Height - _imageAnalysisMinHeight - _contentLayoutManager.TopMargin - _expandButton.Height
                    - _dragToCollectionButton.Height, Height * heightMultiplier);
            }

            // set the image
            _contentLayoutManager.SetSize(Width, _imageHeight);
            _contentLayoutManager.VerticalAlignment = VerticalAlignment.Top;
            _contentLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _contentLayoutManager.ItemWidth = Width - 20;
            _contentLayoutManager.ItemHeight = _imageHeight;
            _contentLayoutManager.SetMargins(20);
            _contentLayoutManager.TopMargin = 40;
            _contentLayoutManager.ArrangeItems(new Vector2(0, 0));

            if (_showRegions)
            {
                _addRegionButton.IsVisible = true;
                _addRegionButton.Transform.LocalPosition = new Vector2(Width / 2 - _addRegionButton.Width / 2,
                        _imageHeight + _contentLayoutManager.TopMargin + 10);
                _originWords.IsVisible = false;
            }
            else
            {
                _addRegionButton.IsVisible = false;
            }

            if (_showsImageAnalysis)
            {
                // set the image analysis
                _imageAnalysisLayoutManager.SetSize(Width, Height - _imageHeight - _contentLayoutManager.TopMargin - _dragToCollectionButton.Height);
                _imageAnalysisLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
                _imageAnalysisLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
                _imageAnalysisLayoutManager.Spacing = 5;
                _imageAnalysisLayoutManager.ArrangeItems(new Vector2(0, _imageHeight + _contentLayoutManager.TopMargin));
            }

            _dragToCollectionButton.Transform.LocalPosition = new Vector2(Width / 2 - _dragToCollectionButton.Width / 2, Height - _dragToCollectionButton.Height - 10);

            // if the expand button is present, arrange UI elements accordingly
            if (_expandButton != null)
            {
                _expandButton.Transform.LocalPosition = new Vector2(Width / 2 - _expandButton.Width / 2,
                    _imageHeight + _contentLayoutManager.TopMargin + 10);
                if (_showsImageAnalysis)
                {
                    _imageAnalysisLayoutManager.SetSize(Width,
                        Height - _imageHeight - _contentLayoutManager.TopMargin - (_expandButton.Height + 20) -
                        _dragToCollectionButton.Height);
                    _imageAnalysisLayoutManager.ArrangeItems(new Vector2(0,
                        _imageHeight + _contentLayoutManager.TopMargin + _expandButton.Height + 20));
                }
                if (_showRegions)
                {
                    _addRegionButton.Transform.LocalPosition = new Vector2(Width / 2 - _addRegionButton.Width / 2,
                        _imageHeight + _contentLayoutManager.TopMargin + _expandButton.Height + 20);
                }
            }

            if (_wordButton != null)
            {
                _wordButton.Transform.LocalPosition = new Vector2(Width/2 - _wordButton.Width/2,
                        _imageHeight + _contentLayoutManager.TopMargin + 10);
            }

            _originWords.Transform.LocalPosition = new Vector2(Width/2 - _originWords.Width/2, -3);
            UpdateOriginText();

            base.Update(parentLocalToScreenTransform);
        }

        public override async Task Load()
        {
            _originWords.Load();
            base.Load();
        }

        /// <summary>
        /// Sets the content to display on the regions page, this is the most important method
        /// </summary>
        /// <param name="content"></param>
        protected void SetContent(RectangleUIElement content)
        {
            // remove the previous content
            if (_contentRect != null)
            {
                RemoveChild(_contentRect);
                _contentLayoutManager.Remove(_contentRect);
                _contentRect.Dispose();
            }
            
            // set content rect to the new content
            _contentRect = content;

            // add the new content
            _contentLayoutManager.AddElement(_contentRect);
            AddChild(_contentRect);
        }


        /// <summary>
        /// Adds a region to the library element that is correlated with the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void AddRegion(NusysConstants.AccessType access, int? currPage = null)
        { 

            // get appropriate new region message based on the current controller
            CreateNewLibraryElementRequestArgs regionRequestArgs = GetNewCreateLibraryElementRequestArgs();
            Debug.Assert(regionRequestArgs != null);

            //create the args and set the parameters that all regions will need
            regionRequestArgs.ContentId = _controller.LibraryElementModel.ContentDataModelId;
            regionRequestArgs.LibraryElementType = _controller.LibraryElementModel.Type;
            regionRequestArgs.Title = "Region " + _controller.Title; // TODO factor out this hard-coded string to a constant
            regionRequestArgs.ParentLibraryElementId = _controller.LibraryElementModel.LibraryElementId;
            regionRequestArgs.Large_Thumbnail_Url = _controller.LibraryElementModel.LargeIconUrl;
            regionRequestArgs.Medium_Thumbnail_Url = _controller.LibraryElementModel.MediumIconUrl;
            regionRequestArgs.Small_Thumbnail_Url = _controller.LibraryElementModel.SmallIconUrl;
            regionRequestArgs.AccessType = access;
            regionRequestArgs.Origin = new LibraryElementOrigin() {Type =  LibraryElementOrigin.OriginType.Region,OriginId = _controller.LibraryElementModel.LibraryElementId };

            var request = new CreateNewLibraryElementRequest(regionRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();
        }

        /// <summary>
        /// Returns the create new library element request args based on the current controller
        /// </summary>
        /// <returns></returns>
        private CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {

            switch (_controller.LibraryElementModel.Type)
            {

                // images are initialized at (.25, .25) with half width half height
                case NusysConstants.ElementType.Image:
                    var imageLibraryElement = (_controller as ImageLibraryElementController)?.ImageLibraryElementModel;
                    Debug.Assert(imageLibraryElement != null);
                    var imageArgs = new CreateNewImageLibraryElementRequestArgs
                    {
                        NormalizedX = imageLibraryElement.NormalizedX + .25*imageLibraryElement.NormalizedWidth,
                        NormalizedY = imageLibraryElement.NormalizedY + .25*imageLibraryElement.NormalizedHeight,
                        NormalizedHeight = .5*imageLibraryElement.NormalizedHeight,
                        NormalizedWidth = .5*imageLibraryElement.NormalizedWidth,
                        AspectRatio = imageLibraryElement.Ratio
                    };
                    return imageArgs;

                // pdfs are initialized at (.25, .25) with half width half height
                case NusysConstants.ElementType.PDF:
                    var pdfLibraryElement = (_controller as PdfLibraryElementController)?.PdfLibraryElementModel;


                    var currPage = (_contentRect as DetailViewPdfRegionContent)?.CurrentPage;
                    Debug.Assert(currPage != null);

                    Debug.Assert(pdfLibraryElement != null);
                    var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs
                    {
                        PdfPageEnd = currPage,
                        PdfPageStart = currPage,
                        NormalizedX = pdfLibraryElement.NormalizedX + .25 * pdfLibraryElement.NormalizedWidth,
                        NormalizedY = pdfLibraryElement.NormalizedY + .25 * pdfLibraryElement.NormalizedHeight,
                        NormalizedHeight = .5 * pdfLibraryElement.NormalizedHeight,
                        NormalizedWidth = .5 * pdfLibraryElement.NormalizedWidth,
                    };
                    return pdfArgs;

                // audio is initialized at .25 from start time with duration of .5
                case NusysConstants.ElementType.Audio:
                    var audioModel = (_controller as AudioLibraryElementController)?.AudioLibraryElementModel;
                    Debug.Assert(audioModel != null);
                    var audioArgs = new CreateNewAudioLibraryElementRequestArgs
                    {
                        StartTime = audioModel.NormalizedStartTime + (audioModel.NormalizedDuration)*.25,
                        Duration = audioModel.NormalizedDuration*.5
                    };

                    return audioArgs;

                // video is initialized at .25 from start time with duration of .5
                case NusysConstants.ElementType.Video:
                    var videoModel = (_controller as VideoLibraryElementController)?.VideoLibraryElementModel;
                    Debug.Assert(videoModel != null);
                    var vidArgs = new CreateNewVideoLibraryElementRequestArgs
                    {
                        StartTime = videoModel.NormalizedStartTime + videoModel.NormalizedDuration*.25,
                        Duration = videoModel.NormalizedDuration*.5,
                        AspectRatio = videoModel.Ratio
                    };
                    return vidArgs;
                default:
                    throw new ArgumentOutOfRangeException($"You passed in the element type: {_controller.LibraryElementModel.Type}! What were you thinking! We don't support regions for that?!" +
                                                          $"Alternatively, how did we not implement support for that here!");
            }



        }
    }
}
