using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

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
        /// The layout manager for the add region button
        /// </summary>
        private StackLayoutManager _addRegionButtonLayoutManager;

        /// <summary>
        /// The add region button
        /// </summary>
        private ButtonUIElement _addRegionButton;

        /// <summary>
        /// The width of the add region button
        /// </summary>
        private float _addRegionButtonWidth = 25;

        /// <summary>
        /// The margin on the left and right of the add region button
        /// </summary>
        private float _addRegionButtonLeftRightMargin = 10;

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
            _addRegionButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle);

            _addRegionButtonLayoutManager = new StackLayoutManager();
            AddChild(_addRegionButton);
            _addRegionButtonLayoutManager.AddElement(_addRegionButton);

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

            _dragToCollectionButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle,
                "Drag to Collection");
            AddChild(_dragToCollectionButton);

            // set the tapped method on the addRegionButton
            _addRegionButton.Tapped += AddRegionButton_Tapped;
            _dragToCollectionButton.DragCompleted += _dragToCollectionButton_DragCompleted;
            _dragToCollectionButton.DragStarted += _dragToCollectionButton_DragStarted;
            _dragToCollectionButton.Dragged += _dragToCollectionButton_Dragged;

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
            _dragRect.Image = await CanvasBitmap.LoadAsync(Canvas, _controller.SmallIconUri);
            _dragRect.IsVisible = true;
        }

        /// <summary>
        /// Fired once when the pointer stops dragging after tapping on the drag to collection button
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _dragToCollectionButton_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.AddElementToCurrentCollection(pointer.CurrentPoint, _controller.LibraryElementModel.Type, _controller);
            _dragRect.Image = null;
            _dragRect.IsVisible = false;
        }

        /// <summary>
        /// Called whenever the add region button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddRegionButton_Tapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            var addRegionButton = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(addRegionButton!= null);
            _addRegionPopup = new FlyoutPopup(this, Canvas);
            _addRegionPopup.Transform.LocalPosition = new Vector2(addRegionButton.Transform.LocalPosition.X - _addRegionPopup.Width / 2,
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
            _addRegionButtonLayoutManager.Dispose();

            _addRegionButton.Tapped -= AddRegionButton_Tapped;
            _dragToCollectionButton.DragCompleted -= _dragToCollectionButton_DragCompleted;
            _dragToCollectionButton.DragStarted -= _dragToCollectionButton_DragStarted;
            _dragToCollectionButton.Dragged -= _dragToCollectionButton_Dragged;

            if (_showsImageAnalysis)
            {
                _imageAnalysisLayoutManager.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_showRegions)
            {
                // set the add region button
                _addRegionButtonLayoutManager.SetSize(_addRegionButtonLeftRightMargin*2 + _addRegionButtonWidth, Height);
                _addRegionButtonLayoutManager.VerticalAlignment = VerticalAlignment.Center;
                _addRegionButtonLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
                _addRegionButtonLayoutManager.ItemWidth = _addRegionButtonWidth;
                _addRegionButtonLayoutManager.ItemHeight = _addRegionButtonWidth;
                _addRegionButtonLayoutManager.ArrangeItems();

                // set visibility of add region button
                _addRegionButton.IsVisible = true;
            }
            else
            {
                _addRegionButton.IsVisible = false;
            }


            _dragToCollectionButton.Transform.LocalPosition = new Vector2(Width/2 + _dragToCollectionButton.Width/2,300);

            //var dragToCollectionHeight = _dragToCollectionButton.Height + 20;

            // get the image height for use in laying out the image on top of the image analysis
            var heightMultiplier = _showsImageAnalysis ? .75f : .9f;
            _imageHeight = Math.Min(Height - _imageAnalysisMinHeight - _contentLayoutManager.TopMargin, Height*heightMultiplier);

            // set the image
            var imageOffsetFromRegionButton = _showRegions ? _addRegionButtonLayoutManager.Width : 0;
            _contentLayoutManager.SetSize(Width - imageOffsetFromRegionButton, _imageHeight);
            _contentLayoutManager.VerticalAlignment = VerticalAlignment.Top;
            _contentLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _contentLayoutManager.ItemWidth = Width - imageOffsetFromRegionButton - 20;
            _contentLayoutManager.ItemHeight = _imageHeight;
            _contentLayoutManager.SetMargins(20);
            _contentLayoutManager.ArrangeItems(new Vector2(imageOffsetFromRegionButton, 0));

            if (_showsImageAnalysis)
            {
                // set the image analysis
                _imageAnalysisLayoutManager.SetSize(Width, Height - _imageHeight - _contentLayoutManager.TopMargin);
                _imageAnalysisLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
                _imageAnalysisLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
                _imageAnalysisLayoutManager.Spacing = 5;
                _imageAnalysisLayoutManager.ArrangeItems(new Vector2(0, _imageHeight + _contentLayoutManager.TopMargin));
            }


            base.Update(parentLocalToScreenTransform);
        }

        public override async Task Load()
        {
            _addRegionButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/add elements white.png"));
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
