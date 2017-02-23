using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class CanvasPointer
    {
        public DateTime StartTime = DateTime.MinValue;
        public DateTime LastUpdated = DateTime.MinValue;
        public uint PointerId;
        public PointerDeviceType DeviceType;
        public PointerPointProperties Properties;
        public Vector2 StartPoint;
        public Vector2 CurrentPoint;
        public float Pressure;
        private Vector2[] _buffer;
        private bool _isBuffering;
        private uint _bufferLength;
        private int _bufferIndex;
        public bool IsEraser = false;
        public FrameworkElement SourceElement;
        public PointerRoutedEventArgs PointerRoutedEventArgs;
        /// <summary>
        /// since the PointerPoint is a XAML element, we cannot access the pointer's properties from the regular thread.
        /// So we have this IsRightButtonPressed getter.
        /// </summary>
        private bool _isRightButtonPressed;
        public bool IsRightButtonPressed
        {
            get { return _isRightButtonPressed; }
        }

        public CanvasPointer() { }
        public CanvasPointer(PointerPoint pointerpoint)
        {
            StartPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y);
            CurrentPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y);
            StartTime = DateTime.Now;
            LastUpdated = DateTime.Now;
            DeviceType = pointerpoint.PointerDevice.PointerDeviceType;
            PointerId = pointerpoint.PointerId;
            Properties = pointerpoint.Properties;
            _isRightButtonPressed = pointerpoint.Properties.IsRightButtonPressed;
            DistanceTraveled = 0;
            _prevPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y); ;
            Pressure = pointerpoint.Properties.Pressure;
            IsEraser = pointerpoint.Properties.IsEraser;
        }

        public override bool Equals(object obj)
        {
            var other = (CanvasPointer) obj;
            return PointerId == other.PointerId;
        }

        public override int GetHashCode()
        {
            return PointerId.GetHashCode();
        }

        public double DistanceTraveled;
        private Vector2 _prevPoint;


        public void Update(PointerPoint point)
        {
            _prevPoint = CurrentPoint;
            var newPoint = new Vector2((float)point.Position.X, (float)point.Position.Y);
            DistanceTraveled += Math.Abs(newPoint.X - CurrentPoint.X) + Math.Abs(newPoint.Y - CurrentPoint.Y);
            CurrentPoint = newPoint;
            LastUpdated = DateTime.Now;
            Pressure = point.Properties.Pressure;
            if (_isBuffering)
            {
                _buffer[_bufferIndex++] = CurrentPoint;
                if (_bufferIndex == _bufferLength)
                    _bufferIndex = 0;
            }
        }

        public void StartBuffering(uint length)
        {
            _bufferLength = length;
            _isBuffering = true;
            _buffer = new Vector2[length];
            for (int i = 0; i < length; i++)
            {
                _buffer[i] = CurrentPoint;
            }
        }

        public Vector2 GetBufferMean()
        {
            var sum = Vector2.Zero;
            for (int i = 0; i < _bufferLength; i++)
            {
                sum += _buffer[i];
            }
            return sum/_bufferLength;
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
            if (other == null)
                return 0;
            return (StartTime - other.StartTime).TotalMilliseconds;
        }
    }
}
