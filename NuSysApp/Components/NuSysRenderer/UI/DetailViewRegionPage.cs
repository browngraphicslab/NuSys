using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DetailViewRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
        }

        /// <summary>
        /// Adds a region to the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void AddRegion(NusysConstants.AccessType access)
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
        public CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
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
                    Debug.Assert(pdfLibraryElement != null);
                    var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs
                    {
                        PdfPageEnd = 1, //todo these should be set to current page numbers
                        PdfPageStart = 1,
                        NormalizedX = .25*pdfLibraryElement.NormalizedX,
                        NormalizedY = .25*pdfLibraryElement.NormalizedY,
                        NormalizedHeight = .5*pdfLibraryElement.NormalizedHeight,
                        NormalizedWidth = .5*pdfLibraryElement.NormalizedWidth
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
