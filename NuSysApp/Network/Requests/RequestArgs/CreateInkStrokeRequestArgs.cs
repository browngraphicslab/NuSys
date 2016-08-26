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
            return message;
        }
    }
}