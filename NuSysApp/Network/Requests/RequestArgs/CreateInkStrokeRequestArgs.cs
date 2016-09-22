using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateInkStrokeRequestArgs : IRequestArgumentable
    {
        /// <summary>
        ///REQUIRED. The Unique id for the ink stroke
        /// </summary>
        public string InkStrokeId { get; set; }

        /// <summary>
        ///REQUIRED The content where the ink is drawn on
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// REQUIRED. These are the list of points that make up the ink stroke.
        /// </summary>
        public List<PointModel> InkPoints { get; set; }

        /// <summary>
        /// the double thickness that this ink stroke will have.
        /// NOT to be confused with the thickness of each point, this thickness represents the overall thickness of this custom ink stroke.
        /// Not setting this thickness value will default to 1;
        /// </summary>
        public double? Thickness { get; set; }

        /// <summary>
        /// The color of the ink stroke.  To be stored as a ColorModel.
        /// Not setting this (it being null), will result in the default color, black.
        /// </summary>
        public ColorModel Color { get; set; }

        /// <summary>
        /// This returns a message that contains all the data in this class ready to send of to the server. It also checks that the message has 
        /// everything it needs
        /// </summary>
        /// <returns></returns>
        public Message PackToRequestKeys()
        {
            Debug.Assert(ContentId != null);
            Debug.Assert(InkStrokeId != null);
            Debug.Assert(InkPoints != null);
            Message message = new Message();
            message[NusysConstants.CREATE_INK_STROKE_REQUEST_CONTENT_ID_KEY] = ContentId;
            message[NusysConstants.CREATE_INK_STROKE_REQUEST_POINTS_KEY] = JsonConvert.SerializeObject(InkPoints);
            message[NusysConstants.CREATE_INK_STROKE_REQUEST_STROKE_ID_KEY] = InkStrokeId;
            message[NusysConstants.CREATE_INK_STROKE_REQUEST_STROKE_THICKNESS_KEY] = Thickness ?? 1;
            message[NusysConstants.CREATE_INK_STROKE_REQUEST_COLOR_KEY] = Color ?? new ColorModel() { A=1, B=1, G=1, R=1 };
            return message;
        }
    }
}