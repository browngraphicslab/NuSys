using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{

    public class InqCanvasView : Canvas
    {
        private bool _isEnabled;
        private uint _pointerId = uint.MaxValue;
        private IInqMode _mode;
        public bool IsPressed = false;
        private InqCanvasViewModel _viewModel;

        public InqCanvasView()
        {
            // Initally, set mode to Inq drawing.
            DataContextChanged += delegate
            {

                _mode = new DrawInqMode(this);

                if (_viewModel == null)
                    return;

                _viewModel.Model.OnFinalizedLine += delegate(InqLineModel lineModel)
                {
                    var lineView = new InqLineView(new InqLineViewModel(lineModel));
                    var points = lineModel.Points;
                    this.Children.Add(lineView);
                };

            };
        }

        public InqCanvasViewModel ViewModel
        {
            set
            {
                DataContext = value;
                value.PropertyChanged += Update;
                _viewModel = value;
            }
            get { return _viewModel; }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_pointerId != uint.MaxValue)
            {
                e.Handled = true;
                return;
            }

            _pointerId = e.Pointer.PointerId;
            if (_mode is DrawInqMode)
            {
                CapturePointer(e.Pointer);
            }
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
            IsPressed = true;

            _mode.OnPointerPressed(this, e);

            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }

            _mode.OnPointerMoved(this, e);
            
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }
            PointerMoved -= OnPointerMoved;
            PointerReleased -= OnPointerReleased;
            _pointerId = uint.MaxValue;
            if (this.PointerCaptures != null && this.PointerCaptures.Count != 0)
            {
                ReleasePointerCapture(e.Pointer);
            }
            IsPressed = false;

            _mode.OnPointerReleased(this, e);

            e.Handled = true;
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
                _mode = new DrawInqMode(this);
            }
        }

        /// <summary>
        /// Turns highlighting on or off
        /// </summary>
        /// <param name="highlight"></param>
        public void SetHighlighting(bool highlight)
        {

            if (highlight)
            {
                _mode = new HighlightInqMode();
            }
            else
            {
                _mode = new DrawInqMode(this);
            }
        }


        public void ReRenderLines()
        {
            Children.Clear();

            var lines = ViewModel.Model.Lines;
            if (lines == null)
                return;

            foreach (InqLineModel line in lines)
            {
                var inqView = new InqLineView(new InqLineViewModel(line), line.StrokeThickness, line.Stroke);
                Children.Add(inqView);
            }
        }

        public bool IsEnabled {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (Parent == null)
                    return;

                if (value ==true)
                {
                    PointerPressed += OnPointerPressed;

                } else
                {
                    PointerPressed -= OnPointerPressed;
                    PointerMoved -= OnPointerMoved;
                    PointerReleased -= OnPointerReleased;
                }
                _isEnabled = value;
            }
        }

        public IInqMode Mode
        {
            get { return _mode; }
        }
        private void Update(object sender, PropertyChangedEventArgs e)
        {
            var vm = (InqCanvasViewModel)sender;
            switch (e.PropertyName)
            {
                case "PartialLineAdded":
                    Children.Add(new InqLineView(new InqLineViewModel(vm.LastPartialLineModel)));
                    break;
                case "FinalLineAdded":
                    var lineView = new InqLineView(new InqLineViewModel(vm.FinalLineModel));
                    this.Children.Add(lineView);
                    break;
            }
        }
    }
}
