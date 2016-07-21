using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RectangleRegion : Region
    {

        public RectangleRegion(Point topLeft, Point bottomRight, string name = "Untitled Rectangle") : base(name)
        {
            TopLeftPoint = topLeft;
            //BottomRightPoint = bottomRight;
            Width = bottomRight.X - topLeft.X;
            Height = bottomRight.Y - topLeft.Y;

            Type = RegionType.Rectangle;
        }

        public Point TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }

        public override Task UnPack(Message message)
        {
            if (message.ContainsKey("rectangle_height"))
            {
                Height = (message.GetDouble("rectangle_height"));
            }
            if (message.ContainsKey("rectangle_width"))
            {
                Width = (message.GetDouble("rectangle_width"));
            }
            if (message.ContainsKey("rectangle_location"))
            {
                TopLeftPoint = (message.GetPoint("rectangle_location"));
            }
            base.UnPack(message);
        }
    }
}
