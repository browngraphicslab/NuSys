using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class RectanglePoints
    {
        public double _leftRatio;
        public double _topRatio;
        public double _widthRatio;
        public double _heightRatio;

        public RectanglePoints(double leftRatio, double topRatio, double widthRatio, double heightRatio)
        {
            _leftRatio = leftRatio;
            _topRatio = topRatio;
            _widthRatio = widthRatio;
            _heightRatio = heightRatio;
        }

        public Rectangle getRectangle()
        {
            Rectangle rectangle = new Rectangle();

            //rectangle.Width = _width;
            //rectangle.Height = _height;
            rectangle.StrokeThickness = 2;

            rectangle.Stroke = new SolidColorBrush(Colors.Black);
            rectangle.Fill = new SolidColorBrush(Colors.Transparent);

            return rectangle;
        }

        public double getLeftRatio()
        {
            return _leftRatio;
        }

        public double getTopRatio()
        {
            return _topRatio;
        }

        public double getWidthRatio()
        {
            return _widthRatio;
        }

        public double getHeightRatio()
        {
            return _heightRatio;
        }


    }
}