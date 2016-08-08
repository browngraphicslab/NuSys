using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class RectangleRegion : Region
    {

        public RectangleRegion(string libraryId, NusysConstants.ElementType type) : base(libraryId, type)
        {
        }

        public PointModel TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY))
            {
                TopLeftPoint = message.Get<PointModel>(NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY);
            }
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_WIDTH_KEY))
            {
                Width = message.GetDouble(NusysConstants.RECTANGLE_REGION_WIDTH_KEY);
            }
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_HEIGHT_KEY))
            {
                Height = message.GetDouble(NusysConstants.RECTANGLE_REGION_HEIGHT_KEY);
            }


        }
    }
}
