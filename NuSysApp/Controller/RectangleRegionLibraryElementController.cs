using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

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
            if (_blockServerInteraction)
            {
                return;
            }

            RectangleRegionModel.Height = height;
            SizeChanged?.Invoke(this, RectangleRegionModel.Width, RectangleRegionModel.Height);
            _debouncingDictionary.Add("rectangle_height", height);
        }
        public void SetWidth(double width)
        {
            if (_blockServerInteraction)
            {
                return;
            }

            RectangleRegionModel.Width = width;
            SizeChanged?.Invoke(this, RectangleRegionModel.Width, RectangleRegionModel.Height);
            _debouncingDictionary.Add("rectangle_width", width);
        }
        public void SetLocation(Point topLeft)
        {
            if (_blockServerInteraction)
            {
                return;
            }
            RectangleRegionModel.TopLeftPoint = new Point(Math.Max(0.001, topLeft.X), Math.Max(0.001, topLeft.Y));
            LocationChanged?.Invoke(this, RectangleRegionModel.TopLeftPoint);
            _debouncingDictionary.Add("rectangle_location", RectangleRegionModel.TopLeftPoint);
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
                SetLocation(message.GetPoint("rectangle_location"));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
