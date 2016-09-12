using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class InkModel
    {
        /// <summary>
        /// The Unique id for the ink stroke
        /// </summary>
        public string InkStrokeId { get; private set; }

        /// <summary>
        /// The content where the ink is drawn on
        /// </summary>
        public string ContentId { get; private set; }

        /// <summary>
        /// These are the list of points that make up the ink stroke
        /// </summary>
        public List<PointModel> InkPoints { get; private set; }

        /// <summary>
        /// To create a new Ink model, create a new instance then call unpack from database
        /// message passing in the message with ink id, content id, and ink points
        /// </summary>
        public InkModel()
        {
            
        }

        /// <summary>
        /// Will populate this model with the data in the passed in message. 
        /// This should only be used by the server when creating a new ink model. Message SHOULD HAVE INK STROKE ID, CONTENT ID, LIST OF POINTS
        /// </summary>
        /// <param name="props"></param>
        public void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey(NusysConstants.INK_TABLE_STROKE_ID))
            {
                InkStrokeId = props.GetString(NusysConstants.INK_TABLE_STROKE_ID);
            }
            if (props.ContainsKey(NusysConstants.INK_TABLE_CONTENT_ID))
            {
                ContentId = props.GetString(NusysConstants.INK_TABLE_CONTENT_ID);
            }
            if (props.ContainsKey(NusysConstants.INK_TABLE_POINTS))
            {
                InkPoints = props.GetList<PointModel>(NusysConstants.INK_TABLE_POINTS);
            }
        }
    }
}
