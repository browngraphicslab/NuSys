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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
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

            _mode = new DrawInqMode(vm.CanvasSize, vm.Model.Id);

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
    }
}
