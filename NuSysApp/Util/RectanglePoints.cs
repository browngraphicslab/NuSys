using Windows.UI.Xaml.Shapes;

namespace NuSysApp.Util
{
    public class RectanglePoints
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        public RectanglePoints(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public Rectangle getRectangle()
        {
            return new Rectangle();
        }
    }
}