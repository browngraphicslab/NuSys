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
using NusysIntermediate;

namespace NuSysApp
{
    public abstract class DetailViewRegionPage : RectangleUIElement
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
        /// The rectangle containing the buttons which are used to add public or private regions. Made visisble when the add region button is pressed
        /// </summary>
        private AddRegionPublicPrivateUIElement _addRegionUIElement;

        /// <summary>
        /// The _analysis ui element associated with the regions page
        /// </summary>
        private ImageAnalysisUIElement _analysisUIElement;

        /// <summary>
        /// The layout manager for the analysis ui element
        /// </summary>
        private StackLayoutManager _imageAnalysisLayoutManager;

        private bool _supportsImageAnalysis;

        protected DetailViewRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller, bool supportsImageAnalysis) : base(parent, resourceCreator)
        {
            // set the controller properly
            _controller = controller;

            // initialize _contentLayoutManager
            _contentLayoutManager = new StackLayoutManager();

            // set image analysis support
            _supportsImageAnalysis = supportsImageAnalysis;

            // initialize the add region button and the _addRegionButtonLayoutManager
            _addRegionButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator))
            {
                Background = Colors.Azure,
                ButtonText = "+",
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray
            };
            _addRegionButtonLayoutManager = new StackLayoutManager();
            AddChild(_addRegionButton);
            _addRegionButtonLayoutManager.AddElement(_addRegionButton);

            // initialize the add region ui element
            _addRegionUIElement = new AddRegionPublicPrivateUIElement(this, resourceCreator);
            _addRegionUIElement.IsVisible = false;
            AddChild(_addRegionUIElement);

            /// add the analysis stuff only if it is supported
            if (_supportsImageAnalysis)
            {
                // initialize the layout manager for the analysis ui element
                _imageAnalysisLayoutManager = new StackLayoutManager();

                // initialize the analysis ui element
                _analysisUIElement = new ImageAnalysisUIElement(this, resourceCreator, controller);
                AddChild(_analysisUIElement);
                _imageAnalysisLayoutManager.AddElement(_analysisUIElement);
            }



            // set the tapped method on the addRegionButton
            _addRegionButton.Tapped += AddRegionButton_Tapped;
        }

        /// <summary>
        /// Called whenever the add region button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddRegionButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            _addRegionUIElement.IsVisible = true;
            _addRegionUIElement.OnRegionAdded += OnRegionAdded;
        }

        /// <summary>
        /// Fired when a region is added from the _addRegionUIElement
        /// </summary>
        /// <param name="access"></param>
        private void OnRegionAdded(NusysConstants.AccessType access)
        {
            _addRegionUIElement.OnRegionAdded -= OnRegionAdded;
            _addRegionUIElement.IsVisible = false;
            AddRegion(access);
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

            if (_supportsImageAnalysis)
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
            // set the add region button
            _addRegionButtonLayoutManager.SetSize(_addRegionButtonLeftRightMargin * 2 + _addRegionButtonWidth, Height);
            _addRegionButtonLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _addRegionButtonLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _addRegionButtonLayoutManager.ItemWidth = _addRegionButtonWidth;
            _addRegionButtonLayoutManager.ItemHeight = _addRegionButtonWidth;
            _addRegionButtonLayoutManager.ArrangeItems();

            // set the addRegionUIElement so that it shows up on the addregionbutton
            _addRegionUIElement.Height = 100;
            _addRegionUIElement.Width = 100;
            _addRegionUIElement.Transform.LocalPosition = new Vector2(_addRegionButton.Transform.LocalX, _addRegionButton.Transform.LocalY - 100);

            // get the image height for use in laying out the image on top of the image analysis
            var imageHeight = Height * .75f;

            // set the image
            _contentLayoutManager.SetSize(Width - _addRegionButtonLayoutManager.Width, imageHeight);
            _contentLayoutManager.VerticalAlignment = VerticalAlignment.Top;
            _contentLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _contentLayoutManager.ItemWidth = Width - _addRegionButtonLayoutManager.Width - 20;
            _contentLayoutManager.ItemHeight = imageHeight;
            _contentLayoutManager.TopMargin = 20;
            _contentLayoutManager.ArrangeItems(new Vector2(_addRegionButtonLayoutManager.Width, 0));


            if (_supportsImageAnalysis)
            {
                // set the image analysis
                _imageAnalysisLayoutManager.SetSize(Width, Height - imageHeight);
                _imageAnalysisLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
                _imageAnalysisLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
                _imageAnalysisLayoutManager.ArrangeItems(new Vector2(0, imageHeight));
            }


            base.Update(parentLocalToScreenTransform);
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
