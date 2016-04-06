using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;


namespace NuSysApp
{
    public static class Extensions
    {


        public static System.Numerics.Vector2 ToSystemVector2(this Point p)
        {
            return new System.Numerics.Vector2((float)p.X, (float)p.Y);
        }

        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static Vector2 ToVector2(this Point2d p)
        {
            return new Vector2(p.X, p.Y);
        }
    }

    public class Vector2
    {
        public double X = 0;
        public double Y = 0;

        public Vector2()
        {
        }

 

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Dot(Vector2 other)
        {
            return X*other.X + Y*other.Y;
        }

        public double Cross(Vector2 other)
        {
            return X*other.Y - Y*other.X;
        }

        public Vector2 Perp()
        {
            return new Vector2(-Y, X);
        }

        public Vector2 GetNormalized()
        {
            double l = Length();
            if (Math.Abs(l) < MathUtil.EPSILON)
                return new Vector2();
            return this/l;
        }

        public static double PerpProduct(Vector2 v0, Vector2 v1)
        {
            return (v0.X*v1.Y - v0.Y*v1.X);
        }

        public double Length()
        {
            double d = X*X + Y*Y;
            if (Math.Abs(d) < MathUtil.EPSILON)
                return 0;
            return Math.Sqrt(X*X + Y*Y);
        }

        public double Length2()
        {
            return X*X + Y*Y;
        }

        public static Vector2 operator +(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v0.X + v1.X, v0.Y + v1.Y);
        }

        public static Vector2 operator -(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v0.X - v1.X, v0.Y - v1.Y);
        }

        public static Vector2 operator *(Vector2 v0, double d)
        {
            return new Vector2(v0.X*d, v0.Y*d);
        }

        public static double operator *(Vector2 v0, Vector2 v1)
        {
            return v0.Dot(v1);
        }

        public static Vector2 operator /(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v0.X/v1.X, v0.Y/v1.Y);
        }

        public static Vector2 operator /(Vector2 v0, double r)
        {
            return new Vector2(v0.X/r, v0.Y/r);
        }

        public static implicit operator Point(Vector2 v)
        {
            return new Point(v.X, v.Y);
        }

        public static implicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
        
        public void DrawPointOn(Canvas canvas, Color color, double perimeter = 4)
        {
            var e = new Ellipse
            {
                Width = perimeter,
                Height = perimeter,
                Fill = new SolidColorBrush(color)
            };

            Canvas.SetLeft(e, X - perimeter);
            Canvas.SetTop(e, Y - perimeter);
            canvas.Children.Add(e);
        }

        public override string ToString()
        {
            return "Vector2( " + X + ", " + Y + " )";
        }
    }
}