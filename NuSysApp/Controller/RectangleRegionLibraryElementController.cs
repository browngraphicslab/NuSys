using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class RectangleRegionLibraryElementController : RegionLibraryElementController
    {
        public event LocationChangedEventHandler LocationChanged;
        public delegate void LocationChangedEventHandler(object sender, Point topLeft);

        public event SizeChangedEventHandler SizeChanged;
        public delegate void SizeChangedEventHandler(object sender, double width, double height);

        public RectangleRegion RectangleRegionModel { get { return this.LibraryElementModel as RectangleRegion;} }

        public RectangleRegionLibraryElementController(RectangleRegion model) : base(model)
        {
        }

        public void SetSize(double width, double height)
        {
            RectangleRegionModel.Width = width;
            RectangleRegionModel.Height = height;
            SizeChanged?.Invoke(this,width,height);
        }

        public void SetHeight(double height)
        {
            RectangleRegionModel.Height = height;
            SizeChanged?.Invoke(this, RectangleRegionModel.Width, RectangleRegionModel.Height);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("rectangle_height", height);
            }
        }
        public void SetWidth(double width)
        {
            RectangleRegionModel.Width = width;
            SizeChanged?.Invoke(this, RectangleRegionModel.Width, RectangleRegionModel.Height);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("rectangle_width", width);
            }
        }
        public void SetLocation(Point topLeft)
        {

            RectangleRegionModel.TopLeftPoint = new PointModel(Math.Max(0.001, topLeft.X), Math.Max(0.001, topLeft.Y));
            LocationChanged?.Invoke(this, new Point(RectangleRegionModel.TopLeftPoint.X, RectangleRegionModel.TopLeftPoint.Y));
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("rectangle_location", RectangleRegionModel.TopLeftPoint);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey("rectangle_height"))
            {
                SetHeight(message.GetDouble("rectangle_height"));
            }
            if (message.ContainsKey("rectangle_width"))
            {
                SetWidth(message.GetDouble("rectangle_width"));
            }
            if (message.ContainsKey("rectangle_location"))
            {
                var pointString = message.GetString("rectangle_location");
                var point = JsonConvert.DeserializeObject<Point>(pointString);
                SetLocation(point);
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
