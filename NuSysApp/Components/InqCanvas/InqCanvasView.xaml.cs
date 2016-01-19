using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public InqCanvasView(InqCanvasViewModel vm)
        {
            this.InitializeComponent();
            _viewModel = vm;
            DataContext = vm;

            _pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
            _pointerMovedHandler = new PointerEventHandler(OnPointerMoved);
            _pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

            IsEnabled = false;
            // Initally, set mode to Inq drawing.

            _mode = new DrawInqMode(vm.CanvasSize, vm.Model.Id, null);

            if (_viewModel == null)
                return;
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
                _mode = new DrawInqMode(_viewModel.CanvasSize, _viewModel.Model.Id, null);
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

                }
                else
                {
                    RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
                    RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
                    RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
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

        private bool needsRender = false;
        private D3D11.Device1 device;
        private D3D11.DeviceContext1 d3dContext;
        private DXGI.SwapChain1 swapChain;
        private D3D11.Texture2D backBufferTexture;
        private D3D11.RenderTargetView backBufferView;
        private SharpDX.Direct2D1.RenderTarget renderTarget;

        private List<RawVector2> _currLine = new List<RawVector2>();
        private List<SharpDX.Direct2D1.PathGeometry> _lines = new List<SharpDX.Direct2D1.PathGeometry>();


        private void SwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {

            // Get the default hardware device and enable debugging. Don't care about the available feature level.
            // DeviceCreationFlags.BgraSupport must be enabled to allow Direct2D interop.
            SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(D3D.DriverType.Hardware, D3D11.DeviceCreationFlags.BgraSupport);

            // Query the default device for the supported device and context interfaces.
            device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
            d3dContext = device.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>();

            float pixelScale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi/96.0f;

            // Query for the adapter and more advanced DXGI objects.
            //SharpDX.DXGI.Device2 dxgiDevice2 = device.QueryInterface<SharpDX.DXGI.Device2>();
            //SharpDX.DXGI.Adapter dxgiAdapter = dxgiDevice2.Adapter;
            //SharpDX.DXGI.Factory2 dxgiFactory2 = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>();

            // Description for our swap chain settings.
            DXGI.SwapChainDescription1 description = new DXGI.SwapChainDescription1()
            {
                AlphaMode = DXGI.AlphaMode.Ignore,
                // Double buffer.
                BufferCount = 2,
                // BGRA 32bit pixel format.
                Format = DXGI.Format.B8G8R8A8_UNorm,
                // Unlike in CoreWindow swap chains, the dimensions must be set.
                Height = (int)(this.SwapChainPanel.RenderSize.Height * pixelScale),
                Width = (int)(this.SwapChainPanel.RenderSize.Width * pixelScale),
                // Default multisampling.
                SampleDescription = new DXGI.SampleDescription(1, 0),
                // In case the control is resized, stretch the swap chain accordingly.
                Scaling = DXGI.Scaling.Stretch,
                // No support for stereo display.
                Stereo = false,
                // Sequential displaying for double buffering.
                SwapEffect = DXGI.SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = DXGI.Usage.BackBuffer | DXGI.Usage.RenderTargetOutput,
            };

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

            DXGI.Surface surface = backBufferTexture.QueryInterface<DXGI.Surface>();
            renderTarget = new SharpDX.Direct2D1.RenderTarget(d2dContext.Factory, surface, new SharpDX.Direct2D1.RenderTargetProperties()
            {
                PixelFormat = new SharpDX.Direct2D1.PixelFormat(
                        SharpDX.DXGI.Format.Unknown,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied),
            });


            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void DrawContinuousLine(Windows.Foundation.Point nextPoint)
        {
            RawVector2 next = ConvertToRawVector2(nextPoint);
            if (_currLine.Count() != 0 && _currLine.Last().X == next.X && _currLine.Last().Y == next.Y)
            {
                return;
            }
            _currLine.Add(next);

            needsRender = true;
        }

        public void EndContinuousLine()
        {
            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(renderTarget.Factory);
            GeometrySink sink = geometry.Open();

            sink.BeginFigure(_currLine.First(), new FigureBegin());
            sink.AddLines(_currLine.ToArray());
            sink.EndFigure(new FigureEnd());
            sink.Close();
            sink.Dispose();
            _lines.Add(geometry);
            _currLine.Clear();
            needsRender = false;
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            if(!needsRender)
            {
                return;
            }

            renderTarget.BeginDraw();
            renderTarget.PushAxisAlignedClip(
            new RectangleF(
                5000,
                5000,
                6000,
                6000
                ),
            AntialiasMode.Aliased
            );
            renderTarget.Clear(ConvertToColorF(Colors.White));
            using (var brush = new SharpDX.Direct2D1.SolidColorBrush(renderTarget, ConvertToColorF(Windows.UI.Colors.Black)))
            {
                foreach(SharpDX.Direct2D1.PathGeometry l in _lines) {
                    renderTarget.DrawGeometry(l, brush);
                }
                SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(renderTarget.Factory);
                GeometrySink sink = geometry.Open();

                sink.BeginFigure(_currLine.First(), new FigureBegin());
                sink.AddLines(_currLine.ToArray());
                sink.EndFigure(new FigureEnd());
                sink.Close();
                sink.Dispose();
                renderTarget.DrawGeometry(geometry, brush);
            }
            renderTarget.PopAxisAlignedClip();
            renderTarget.EndDraw();

            // Tell the swap chain to present the buffer.
            this.swapChain.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());
            needsRender = false;
        }

        private static SharpDX.Color ConvertToColorF(Windows.UI.Color color)
        {
            return new SharpDX.Color(color.R, color.G, color.B, color.A);
        }

        private static RawVector2 ConvertToRawVector2(Windows.Foundation.Point p)
        {
            RawVector2 r = new RawVector2();
            r.X = (float)p.X;
            r.Y = (float)p.Y;
            return r;
        }

        private void DisposeResources()
        {
            Utilities.Dispose(ref device);
            Utilities.Dispose(ref d3dContext);
            Utilities.Dispose(ref swapChain);
            //Utilities.Dispose(ref d2dContext);
            //Utilities.Dispose(ref d2dTarget);
        }
    }
}
