using Windows.Foundation;

namespace NuSysApp
{
    public class AddPointEventArgs : System.EventArgs
    {
        public AddPointEventArgs(Point p)
        {
            PointToAdd = p;
        }

        public Point PointToAdd { get; }

    }
}