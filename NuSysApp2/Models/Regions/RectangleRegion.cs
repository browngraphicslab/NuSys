using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class RectangleRegion : Region
    {

        public RectangleRegion(string libraryId, ElementType type) : base(libraryId, type)
        {

        }

        public Point TopLeftPoint { set; get; }
        public double Width { set; get; }
        public double Height { set; get; }
        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("rectangle_width"))
            {
                Width = message.GetDouble("rectangle_width");
            }
            if (message.ContainsKey("rectangle_height"))
            {
                Height = message.GetDouble("rectangle_height");
            }
            if (message.ContainsKey("rectangle_top_left_point"))
            {
                TopLeftPoint = message.GetPoint("rectangle_top_left_point");
            }
            await base.UnPack(message);
        }
    }
}
