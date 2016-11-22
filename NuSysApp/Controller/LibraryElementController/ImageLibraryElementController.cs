using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Newtonsoft.Json;
using NusysIntermediate;
using Windows.Storage;
using WinRTXamlToolkit.IO.Extensions;

namespace NuSysApp
{
    public class ImageLibraryElementController : LibraryElementController
    {
        public event LocationChangedEventHandler LocationChanged;
        public delegate void LocationChangedEventHandler(object sender, Point topLeft);

        public event SizeChangedEventHandler SizeChanged;
        public delegate void SizeChangedEventHandler(object sender, double width, double height);

        public ImageLibraryElementModel ImageLibraryElementModel { get { return LibraryElementModel as ImageLibraryElementModel; } }

        public ImageLibraryElementController(ImageLibraryElementModel model) : base(model)
        {
        }

        public void SetSize(double width, double height)
        {
            ImageLibraryElementModel.NormalizedWidth = width;
            ImageLibraryElementModel.NormalizedHeight = height;
            SizeChanged?.Invoke(this,width,height);
        }

        public void SetHeight(double normalizedHeight)
        {
            ImageLibraryElementModel.NormalizedHeight = Math.Max(0, Math.Min(1, normalizedHeight)); ; ;
            SizeChanged?.Invoke(this, ImageLibraryElementModel.NormalizedWidth, ImageLibraryElementModel.NormalizedHeight);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_HEIGHT_KEY, normalizedHeight);
            }
        }
        public void SetWidth(double normalizedWidth)
        {
            ImageLibraryElementModel.NormalizedWidth = Math.Max(0, Math.Min(1, normalizedWidth)); ; ;
            SizeChanged?.Invoke(this, ImageLibraryElementModel.NormalizedWidth, ImageLibraryElementModel.NormalizedHeight);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_WIDTH_KEY, normalizedWidth);
            }
        }

        /// <summary>
        /// method used to set the top left normalized x coordinate of the image.
        /// Will fire an event and update the server if we're not currently getting and update from the server.
        /// </summary>
        /// <param name="normalizedX"></param>
        public void SetXLocation(double normalizedX)
        {
            ImageLibraryElementModel.NormalizedX = Math.Max(0, Math.Min(1 - ImageLibraryElementModel.NormalizedWidth, normalizedX));
            LocationChanged?.Invoke(this, new Point(ImageLibraryElementModel.NormalizedX, ImageLibraryElementModel.NormalizedY));

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_X_KEY, normalizedX);
            }
        }
        
        /// <summary>
        /// method used to set the top left normalized Y coordinate of the image.
        /// Will fire an event and update the server if we're not currently getting and update from the server.
        /// </summary>
        /// <param name="normalizedX"></param>
        public void SetYLocation(double normalizedY)
        {
            ImageLibraryElementModel.NormalizedY = Math.Max(0, Math.Min(1 - ImageLibraryElementModel.NormalizedHeight, normalizedY)); ;

            LocationChanged?.Invoke(this, new Point(ImageLibraryElementModel.NormalizedX, ImageLibraryElementModel.NormalizedY));

            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_Y_KEY, normalizedY);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_HEIGHT_KEY))
            {
                SetHeight(message.GetDouble(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_HEIGHT_KEY));
            }
            if (message.ContainsKey(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_WIDTH_KEY))
            {
                SetWidth(message.GetDouble(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_WIDTH_KEY));
            }

            if (message.ContainsKey(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_X_KEY))
            {
                SetXLocation(message.GetDouble(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_X_KEY));
            }
            if (message.ContainsKey(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_Y_KEY))
            {
                SetYLocation(message.GetDouble(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_Y_KEY));
            }

            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
