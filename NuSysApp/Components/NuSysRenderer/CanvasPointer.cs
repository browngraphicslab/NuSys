using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class CanvasPointer
    {
        public PointerPoint Pointer;
        public DateTime StartTime = DateTime.MinValue;
        public DateTime LastUpdated = DateTime.MinValue;
        public string PointerId;
        public PointerDeviceType DeviceType;

        public CanvasPointer() { }

        public CanvasPointer(PointerPoint pointerpoint)
        {
            StartPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y);
            CurrentPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y);
            StartTime = DateTime.Now;
            LastUpdated = DateTime.Now;
            Pointer = pointerpoint;
            DeviceType = pointerpoint.PointerDevice.PointerDeviceType;
        }

        public Vector2 StartPoint
        {
            get
            {
                return _startPoint;
            }
            set
            {
                CurrentPoint = value;
                _startPoint = value;
            }
        }

        public Vector2 CurrentPoint;
        public double DistanceTraveled;
        private Vector2 _startPoint;
        private Vector2 _prevPoint;


        public void Update(Point point)
        {
            _prevPoint = CurrentPoint;
            var newPoint = new Vector2((float)point.X, (float)point.Y);
            DistanceTraveled += Math.Abs(newPoint.X - CurrentPoint.X) + Math.Abs(newPoint.Y - CurrentPoint.Y);
            CurrentPoint = newPoint;
            LastUpdated = DateTime.Now;
        }

        public double MillisecondsActive
        {
            get { return LastUpdated.Subtract(StartTime).TotalMilliseconds; }
        }

        public Vector2 Delta
        {
            get { return CurrentPoint - StartPoint; }
        }

        public Vector2 DeltaSinceLastUpdate
        {
            get { return CurrentPoint - _prevPoint; }
        }

        public double DistanceTo(CanvasPointer other)
        {
            return MathUtil.Dist(CurrentPoint, other.CurrentPoint);
        }

        public double StartTimeDelta(CanvasPointer other)
        {
            return (StartTime - other.StartTime).TotalMilliseconds;
        }
    }
}
