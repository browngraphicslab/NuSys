using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// The Library element model class for images.
    /// This library element model sub class only adds a double Ratio for the aspect ration of the image.
    /// </summary>
    public class ImageLibraryElementModel : LibraryElementModel
    {
        /// <summary>
        /// The double ratio of the image that this is a library element for. 
        /// The ratio should be calculated as the Width/Height.
        /// </summary>
        public double Ratio { get; set; }
        
        public double NormalizedX { get; set; }

        public double NormalizedY { get; set; }

        public double NormalizedWidth { get; set; }
        public double NormalizedHeight { get; set; }

        /// <summary>
        /// the constructor just takes in an id and then tells the base class what type of library element model this is.
        /// Ity also takes in an element type (uesd for pdfs)
        /// </summary>
        /// <param name="libraryElementId"></param>
        public ImageLibraryElementModel(string libraryElementId, NusysConstants.ElementType type = NusysConstants.ElementType.Image) : base(libraryElementId, type) { }

        /// <summary>
        /// override unpack from database keys method used to set the properties of this model after a message is recieved from a sql query.
        /// This method should just set the double ratio property if it exists in the database message.
        /// </summary>
        /// <param name="message"></param>
        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_RATIO_KEY))
            {
                Ratio = message.GetDouble(NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_RATIO_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
