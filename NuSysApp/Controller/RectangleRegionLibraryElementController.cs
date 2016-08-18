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
                _debouncingDictionary.Add(NusysConstants.RECTANGLE_REGION_HEIGHT_KEY, height);
            }
        }
        public void SetWidth(double width)
        {
            RectangleRegionModel.Width = width;
            SizeChanged?.Invoke(this, RectangleRegionModel.Width, RectangleRegionModel.Height);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.RECTANGLE_REGION_WIDTH_KEY, width);
            }
        }
        public void SetLocation(Point topLeft)
        {

            RectangleRegionModel.TopLeftPoint = new PointModel(Math.Max(0.001, topLeft.X), Math.Max(0.001, topLeft.Y));
            LocationChanged?.Invoke(this, new Point(RectangleRegionModel.TopLeftPoint.X, RectangleRegionModel.TopLeftPoint.Y));
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY, RectangleRegionModel.TopLeftPoint);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_HEIGHT_KEY))
            {
                SetHeight(message.GetDouble(NusysConstants.RECTANGLE_REGION_HEIGHT_KEY));
            }
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_WIDTH_KEY))
            {
                SetWidth(message.GetDouble(NusysConstants.RECTANGLE_REGION_WIDTH_KEY));
            }
            if (message.ContainsKey(NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY))
            {
                var pointString = message.GetString(NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY);
                var point = JsonConvert.DeserializeObject<Point>(pointString);
                SetLocation(point);
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
