﻿using System;
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
        public DateTime StartTime = DateTime.MinValue;
        public DateTime LastUpdated = DateTime.MinValue;
        public uint PointerId;
        public PointerDeviceType DeviceType;
        public PointerPointProperties Properties;
        public Vector2 StartPoint;
        public Vector2 CurrentPoint;

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
            DistanceTraveled = 0;
            _prevPoint = new Vector2((float)pointerpoint.Position.X, (float)pointerpoint.Position.Y); ;

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
            if (other == null)
                return 0;
            return (StartTime - other.StartTime).TotalMilliseconds;
        }
    }
}