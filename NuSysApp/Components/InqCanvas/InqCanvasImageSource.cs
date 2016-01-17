using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{

    //TODO: add stuff for clipping the image
    public sealed class InqCanvasImageSource : Windows.UI.Xaml.Media.Imaging.SurfaceImageSource
    {
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct2D1.Device d2dDevice;
        private SharpDX.Direct2D1.DeviceContext d2dContext;
        //private SharpDX.Direct2D1.Factory factory;
        private readonly int width;
        private readonly int height;

        private List<SharpDX.Direct2D1.PathGeometry> lines;

        public InqCanvasImageSource(int pixelWidth, int pixelHeight, bool isOpaque)
            : base(pixelWidth, pixelHeight, isOpaque)
        {
            width = pixelWidth;
            height = pixelHeight;

            lines = new List<SharpDX.Direct2D1.PathGeometry>();

            CreateDeviceResources();

            Application.Current.Suspending += OnSuspending;
        }

        public void Dispose()
        {
            Utilities.Dispose(ref d2dDevice);
            Utilities.Dispose(ref d2dContext);
            foreach (SharpDX.Direct2D1.PathGeometry geom in lines)
            {
                geom.Dispose();
            }
        }

        private void CreateDeviceResources()
        {
            //we have to dispose explicitly cause were not using c++
            Dispose();
            //Utilities.Dispose(ref factory);
            //add support for surfaces with different color channel orders cause we need this to use Direct2d
            var creationFlags = DeviceCreationFlags.BgraSupport;

            //supported directx levels
            SharpDX.Direct3D.FeatureLevel[] featureLevels =
{
                SharpDX.Direct3D.FeatureLevel.Level_11_1,
                SharpDX.Direct3D.FeatureLevel.Level_11_0,
                SharpDX.Direct3D.FeatureLevel.Level_10_1,
                SharpDX.Direct3D.FeatureLevel.Level_10_0,
                SharpDX.Direct3D.FeatureLevel.Level_9_3,
                SharpDX.Direct3D.FeatureLevel.Level_9_2,
                SharpDX.Direct3D.FeatureLevel.Level_9_1,
            };
            d3dDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, creationFlags, featureLevels);
            //factory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded);

            // Get the Direct3D 11.1 API device.
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                // Create the Direct2D device object and a corresponding context.
                d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice);

                d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.None);

                // Query for ISurfaceImageSourceNative interface.
                using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
                    sisNative.Device = dxgiDevice;
            }
        }

        public void BeginDraw()
        {
            BeginDraw(new Windows.Foundation.Rect(0, 0, width, height));
        }

        public void BeginDraw(Windows.Foundation.Rect updateRect)
        {
            var updateRectNative = new Rectangle
            {
                Left = (int)updateRect.Left,
                Top = (int)updateRect.Top,
                Right = (int)updateRect.Right,
                Bottom = (int)updateRect.Bottom
            };

            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
            {

                try
                {
                    RawPoint offset;
                    using (var surface = sisNative.BeginDraw(updateRectNative, out offset))
                    {
                        using (var bitmap = new Bitmap1(d2dContext, surface))
                        {
                            d2dContext.Target = bitmap;
                        }

                        d2dContext.BeginDraw();

                        //we should maybe turn off AA cause i've heard its super slow in Direct2D
                        d2dContext.PushAxisAlignedClip(
                            new RectangleF(
                                (offset.X),
                                (offset.Y),
                                (offset.X + (float)updateRect.Width),
                                (offset.Y + (float)updateRect.Height)
                                ),
                            AntialiasMode.Aliased
                            );

                        d2dContext.Transform = Matrix3x2.Translation(offset.X, offset.Y);
                    }
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved ||
    ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset)
                    {
                        // If the device has been removed or reset, attempt to recreate it and continue drawing.
                        CreateDeviceResources();
                        BeginDraw(updateRect);
                    }
                    else
                    {
                        throw;
                    }
                }

            }
        }

        public void EndDraw()
        {
            // Remove the transform and clip applied in BeginDraw since
            // the target area can change on every update.
            d2dContext.Transform = Matrix3x2.Identity;
            d2dContext.PopAxisAlignedClip();

            // Remove the render target and end drawing.
            d2dContext.EndDraw();

            d2dContext.Target = null;

            // Query for ISurfaceImageSourceNative interface.
            using (var sisNative = ComObject.QueryInterface<ISurfaceImageSourceNative>(this))
                sisNative.EndDraw();
        }

        public void Clear(Windows.UI.Color color)
        {
            d2dContext.Clear(ConvertToColorF(color));
        }

        public void FillSolidRect(Windows.UI.Color color, Windows.Foundation.Rect rect)
        {
            // Create a solid color D2D brush.
            using (var brush = new SolidColorBrush(d2dContext, ConvertToColorF(color)))
            {
                // Draw a filled rectangle.
                d2dContext.FillRectangle(ConvertToRectF(rect), brush);
            }
        }


        private List<RawVector2> _currLine = new List<RawVector2>();
         
        //this is VERY naive. am currently working on a better method
        public void DrawContinuousLine(Windows.Foundation.Point nextPoint)
        {
            _currLine.Add(ConvertToRawVector2(nextPoint));

            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(d2dContext.Factory);
            GeometrySink sink = geometry.Open();

            sink.BeginFigure(_currLine.First(), new FigureBegin());
            sink.AddLines(_currLine.ToArray());
            sink.EndFigure(new FigureEnd());
            sink.Close();
            sink.Dispose();

            BeginDraw();
            Clear(Windows.UI.Colors.White);
            using (var brush = new SolidColorBrush(d2dContext, ConvertToColorF(Windows.UI.Colors.Black)))
            {

                d2dContext.DrawGeometry(geometry, brush);
                foreach (SharpDX.Direct2D1.PathGeometry geom in lines)
                {
                    d2dContext.DrawGeometry(geom, brush);
                }
            }
            EndDraw();
            geometry.Dispose();
        }

        public void EndContinuousLine()
        {
            _currLine.Clear();
        }

        public void AddLine(Windows.UI.Color color, Windows.Foundation.Point[] points)
        {
            RawVector2[] converted = new RawVector2[points.Length - 1];
            //we want to start at index 1 cause we want to start the figure at index 0
            for (int i = 1; i < points.Length; i++)
            {
                converted[i - 1] = ConvertToRawVector2(points[i]);
            }


            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(d2dContext.Factory);
            GeometrySink sink = geometry.Open();

            sink.BeginFigure(ConvertToRawVector2(points[0]), new FigureBegin());
            sink.AddLines(converted);
            sink.EndFigure(new FigureEnd());
            sink.Close();

            lines.Add(geometry);
            sink.Dispose();
        }

        public void RenderLines()
        {

            BeginDraw();
            Clear(Windows.UI.Colors.White);
            using (var brush = new SolidColorBrush(d2dContext, ConvertToColorF(Windows.UI.Colors.Black)))
            {
                foreach(SharpDX.Direct2D1.PathGeometry geom in lines) {
                    d2dContext.DrawGeometry(geom, brush);
                }
            }
            EndDraw();
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            // Hints to the driver that the app is entering an idle state and that its memory can be used temporarily for other apps.
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device3>())
                dxgiDevice.Trim();
        }

        private static Color ConvertToColorF(Windows.UI.Color color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        private static RectangleF ConvertToRectF(Windows.Foundation.Rect rect)
        {
            return new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        private static RawVector2 ConvertToRawVector2(Windows.Foundation.Point p)
        {
            RawVector2 r = new RawVector2();
            r.X = (float)p.X;
            r.Y = (float)p.Y;
            return r;
        }
    }
}
