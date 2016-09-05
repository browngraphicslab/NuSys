using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using NusysIntermediate;

namespace NusysIntermediate
{
    /// <summary>
    /// public class for the video library element Models.
    /// Should only add a ratio property to the library element model.
    /// </summary>
    public class VideoLibraryElementModel : AudioLibraryElementModel
    {
        /// <summary>
        /// The double ratio of the video that this is a library element for. 
        /// The ratio should be calculated as the Width/Height.
        /// </summary>
        public double Ratio { get; set; }

        /// <summary>
        /// constructor takes in a library element id and tells the base class what type of library element this is.
        /// </summary>
        /// <param name="libraryElementId"></param>
        public VideoLibraryElementModel(string libraryElementId) : base(libraryElementId, NusysConstants.ElementType.Video) {}

        /// <summary>
        /// override unpack from database keys method used to set the proprties of this model after a message is recieved from a sql query.
        /// This method should just set the ratio property.
        /// </summary>
        /// <param name="message"></param>
        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.VIDEO_LIBRARY_ELEMENT_MODEL_RATIO_KEY))
            {
                Ratio = message.GetDouble(NusysConstants.VIDEO_LIBRARY_ELEMENT_MODEL_RATIO_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
