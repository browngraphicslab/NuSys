using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Windows.Graphics.Display;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp

    //TODO: fix interaction with other UI elements
    //fix size of canvas
{
    public sealed partial class InqCanvasView : UserControl
    {
        private bool _isEnabled;
        private uint _pointerId = uint.MaxValue;
        private IInqMode _mode;
        public bool IsPressed = false;
        private InqCanvasViewModel _viewModel;

        private PointerEventHandler _pointerPressedHandler;
        private PointerEventHandler _pointerMovedHandler;
        private PointerEventHandler _pointerReleasedHandler;
        private PointerEventHandler _pointerEnteredHandler;

        public InqCanvasView(InqCanvasViewModel vm)
        {
            this.InitializeComponent();
            _viewModel = vm;
            DataContext = vm;

            _pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
            _pointerMovedHandler = new PointerEventHandler(OnPointerMoved);
            _pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);
            _pointerEnteredHandler = new PointerEventHandler(OnPointerEntered);

            IsEnabled = false;
            // Initally, set mode to Inq drawing.

            _mode = new DrawInqMode(vm.CanvasSize, vm.Model.Id);

            if (_viewModel == null)
                return;
        }

        public InqCanvasViewModel ViewModel 
        {
            get { return (InqCanvasViewModel) DataContext; }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if ((e.GetCurrentPoint(this) as PointerPoint).Properties.IsBarrelButtonPressed)
            {

            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {

            if (_pointerId != uint.MaxValue)
            {
                return;
            }

            _pointerId = e.Pointer.PointerId;
            if (_mode is DrawInqMode)
            {
                CapturePointer(e.Pointer);
            }

            AddHandler(PointerMovedEvent, _pointerMovedHandler, true);
            AddHandler(PointerReleasedEvent, _pointerReleasedHandler, true);
            IsPressed = true;
            _mode.OnPointerPressed(this, e);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                return;
            }

            _mode.OnPointerMoved(this, e);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                return;
            }

            RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
            RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
            _pointerId = uint.MaxValue;
            if (this.PointerCaptures != null && this.PointerCaptures.Count != 0)
            {
                ReleasePointerCapture(e.Pointer);
            }
            IsPressed = false;

            _mode.OnPointerReleased(this, e);

        }

        /// <summary>
        /// Turns erasing on or off
        /// </summary>
        /// <param name="erase"></param>
        public void SetErasing(bool erase)
        {
            if (erase)
            {
                _mode = new EraseInqMode();
            }
            else
            {
                _mode = new DrawInqMode(_viewModel.CanvasSize, _viewModel.Model.Id);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {

                if (value)
                {
                    AddHandler(PointerPressedEvent, _pointerPressedHandler, true);
                    AddHandler(PointerEnteredEvent, _pointerEnteredHandler, true);
                }
                else
                {
                    RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
                    RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
                    RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
                    RemoveHandler(PointerEnteredEvent, _pointerEnteredHandler);
                }
                _isEnabled = value;
                IsHitTestVisible = value;
            }
        }

        public IInqMode Mode
        {
            get { return _mode; }
        }




















        //Graphics stuff

        //NOTE: the actual render target (where all the draw calls are made on) is in the ViewModel

        //whether we need to update the current frame
        private bool needsRender = false;
        //the device we're drawing to
        private D3D11.Device1 device;

        private D3D11.DeviceContext1 d3dContext;
        //swap chain and textures to use double buffering
        private DXGI.SwapChain1 swapChain;
        private D3D11.Texture2D backBufferTexture;
        private D3D11.RenderTargetView backBufferView;

        //the stroke that is currently being drawn
        private float size = 50000;


        public void SetTransform(CompositeTransform ct)
        {
            _viewModel.Transform = ct;

        }

        private void SwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            DisposeResources();

            _viewModel.Transform = new CompositeTransform { TranslateX = -50000, TranslateY = -50000 , CenterX = 50000, CenterY = 50000};

            // DeviceCreationFlags.BgraSupport is needed to use Direct2D
            SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(D3D.DriverType.Hardware, D3D11.DeviceCreationFlags.BgraSupport);

            // Query the default device for the supported device and context interfaces.
            device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            d3dContext = device.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            //convert from display dependent pixels to display independent pixels 
            float pixelScale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi/96.0f;


            // Description for our swap chain settings.
            DXGI.SwapChainDescription1 description = new DXGI.SwapChainDescription1()
            {
                AlphaMode = DXGI.AlphaMode.Ignore,
                // Double buffers
                BufferCount = 2,
                // BGRA 32bit pixel format
                Format = DXGI.Format.B8G8R8A8_UNorm,
                //set the width and height to current display dimensions
                Height = (int)(this.SwapChainPanel.RenderSize.Height * pixelScale),
                Width = (int)(this.SwapChainPanel.RenderSize.Width * pixelScale),
                // Default multisampling
                SampleDescription = new DXGI.SampleDescription(1, 0),
                // I dont know if this is actuall necessary? we'll see in testing I guess
                Scaling = DXGI.Scaling.Stretch,
                // No support for stereo display
                Stereo = false,
                // Double buffering
                SwapEffect = DXGI.SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = DXGI.Usage.BackBuffer | DXGI.Usage.RenderTargetOutput,
            };

            //make a direct2d context cause we will be using direct2d to actually draw the lines
            SharpDX.Direct2D1.DeviceContext d2dContext;
            using (DXGI.Device3 dxgiDevice3 = this.device.QueryInterface<DXGI.Device3>())
            {
                SharpDX.Direct2D1.Device d2dDevice = new SharpDX.Direct2D1.Device(dxgiDevice3);
                d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);
                // Get the DXGI factory automatically created when initializing the Direct3D device.
                using (DXGI.Factory3 dxgiFactory3 = dxgiDevice3.Adapter.GetParent<DXGI.Factory3>())
                {
                    // Create the swap chain and get the highest version available.
                    using (DXGI.SwapChain1 swapChain1 = new DXGI.SwapChain1(dxgiFactory3, this.device, ref description))
                    {
                        this.swapChain = swapChain1.QueryInterface<DXGI.SwapChain2>();
                    }
                }
            }

            // Obtain a reference to the native COM object of the SwapChainPanel.
            using (DXGI.ISwapChainPanelNative nativeObject = ComObject.As<DXGI.ISwapChainPanelNative>(this.SwapChainPanel))
            {
                // Set its swap chain.
                nativeObject.SwapChain = this.swapChain;
            }

            // Create a Texture2D from the existing swap chain to use as 
            this.backBufferTexture = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(this.swapChain, 0);
            this.backBufferView = new D3D11.RenderTargetView(this.device, this.backBufferTexture);

            //create a surface from the texture so we can write to it with Direct2d (yay double buffering)
            DXGI.Surface surface = backBufferTexture.QueryInterface<DXGI.Surface>();
            //generate the render target
            _viewModel.RenderTarget = new SharpDX.Direct2D1.RenderTarget(d2dContext.Factory, surface, new SharpDX.Direct2D1.RenderTargetProperties()
            {
                PixelFormat = new SharpDX.Direct2D1.PixelFormat(
                        SharpDX.DXGI.Format.Unknown,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied),
            });

            //clear both buffers just cause
            _viewModel.RenderTarget.BeginDraw();
            _viewModel.RenderTarget.Clear(ConvertToColorF(Colors.Beige));
            _viewModel.RenderTarget.EndDraw();
            this.swapChain.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());
            _viewModel.RenderTarget.BeginDraw();
            _viewModel.RenderTarget.Clear(ConvertToColorF(Colors.Beige));
            _viewModel.RenderTarget.EndDraw();
            this.swapChain.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());

            //attach a function to the rendering event that will fire on every rendering call
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        //call to start drawing an in progress stroke
        public void BeginContinuousLine(double x, double y)
        {
            _viewModel.CurrentLine.Clear();
            DrawContinuousLine(x, y);
        }

        //call while updating a currently being drawn stroke
        public void DrawContinuousLine(double x, double y)
        {
            Windows.Foundation.Point p = _viewModel.Transform.Inverse.TransformPoint(new Windows.Foundation.Point(x, y));
            RawVector2 next = new RawVector2();
            next.X = (float)(p.X);
            next.Y = (float)(p.Y);
            if (_viewModel.CurrentLine.Count() != 0 && _viewModel.CurrentLine.Last().X == next.X && _viewModel.CurrentLine.Last().Y == next.Y)
            {
                return;
            }

            _viewModel.CurrentLine.Add(next);

            needsRender = true;
        }


        //called when we render things
        private void CompositionTarget_Rendering(object sender, object e)
        {

            ////optimize by not rendering when we dont need to
            //if (!needsRender)
            //{
            //    return;
            //}

            //begin the draw
            _viewModel.RenderTarget.BeginDraw();

            Matrix3x2 translation = Matrix3x2.Translation((float)_viewModel.Transform.TranslateX, (float)_viewModel.Transform.TranslateY);
            Matrix3x2 scale = Matrix3x2.Scaling((float)_viewModel.Transform.ScaleX, (float)_viewModel.Transform.ScaleY);
            Matrix3x2 toOrigin = Matrix3x2.Translation((float)(_viewModel.Transform.CenterX + _viewModel.Transform.TranslateX), (float)(_viewModel.Transform.CenterY+ _viewModel.Transform.TranslateY));
            Matrix3x2 fromOrigin = Matrix3x2.Translation((float)-(_viewModel.Transform.CenterX + _viewModel.Transform.TranslateX), (float)-(_viewModel.Transform.CenterY+ _viewModel.Transform.TranslateY));

            _viewModel.RenderTarget.Transform = translation * fromOrigin * scale * toOrigin;


            //clear the render target so we can draw to an empty space (direct2d is an immediate mode API)
            _viewModel.RenderTarget.Clear(ConvertToColorF(Colors.Beige));

            //eventually we will change the brush for each line according to that line's color
            using (var brush = new SharpDX.Direct2D1.SolidColorBrush(_viewModel.RenderTarget, ConvertToColorF(Windows.UI.Colors.Black)))
            {
                //draw all of the lines that have already been drawn (we've already created the geometries for these)
                foreach(SharpDX.Direct2D1.PathGeometry l in _viewModel.Lines) {
                    _viewModel.RenderTarget.DrawGeometry(l, brush);
                }

                //draw the line that is currently being drawn
                if(_viewModel.CurrentLine.Count() > 0)
                {
                    SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(_viewModel.RenderTarget.Factory);
                    GeometrySink sink = geometry.Open();

                    sink.BeginFigure(_viewModel.CurrentLine.First(), new FigureBegin());
                    sink.AddLines(_viewModel.CurrentLine.ToArray());

                    sink.EndFigure(new FigureEnd());
                    sink.Close();
                    sink.Dispose();
                    _viewModel.RenderTarget.DrawGeometry(geometry, brush);
                }
            }

            _viewModel.RenderTarget.EndDraw();

            // Tell the swap chain to present the panel we're currently drawing to
            this.swapChain.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());
            //weve rendered all we need to
            needsRender = false;
        }

        //converts from Windows.UI color to DirectX color
        private static SharpDX.Color ConvertToColorF(Windows.UI.Color color)
        {
            return new SharpDX.Color(color.R, color.G, color.B, color.A);
        }

        //Converts from Windows.Foundation Point to vector2
        private static RawVector2 ConvertToRawVector2(Windows.Foundation.Point p)
        {
            RawVector2 r = new RawVector2();
            r.X = (float)p.X;
            r.Y = (float)p.Y;
            return r;
        }

        private static RectangleF ConvertToRectF(Windows.Foundation.Rect rect)
        {
            return new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        //our version of a destructor(we need to explicitly free resources because
        //under the hood this is all C++
        private void DisposeResources()
        {
            Utilities.Dispose(ref device);
            Utilities.Dispose(ref d3dContext);
            Utilities.Dispose(ref swapChain);
            Utilities.Dispose(ref backBufferTexture);
            Utilities.Dispose(ref backBufferView);
            if(_viewModel.RenderTarget != null)
            {
                _viewModel.RenderTarget.Dispose();
            }
        }
    }
}
