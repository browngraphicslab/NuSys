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
        public string InkStrokeId { get; set; }

        /// <summary>
        /// The content where the ink is drawn on
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// These are the list of points that make up the ink stroke
        /// </summary>
        public List<PointModel> InkPoints { get; set; }

        /// <summary>
        /// the color of the ink stroke.
        /// stored in the nusys intermediate type for color, colorModel.
        /// </summary>
        public ColorModel Color { get; set; }

        /// <summary>
        /// the double thickness of the ink stroke.  
        /// </summary>
        public double Thickness { get; set; }

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
            if (props.ContainsKey(NusysConstants.INK_TABLE_INK_COLOR))
            {
                Color = props.Get<ColorModel>(NusysConstants.INK_TABLE_INK_COLOR);
            }
            if (props.ContainsKey(NusysConstants.INK_TABLE_INK_THICKNESS))
            {
                Thickness = props.GetDouble(NusysConstants.INK_TABLE_INK_THICKNESS);
            }

        }
    }
}
