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

namespace ResetTest
{

    public class InqCanvasView : Canvas
    {
        private bool _isEnabled;
        private uint _pointerId = uint.MaxValue;
        public bool IsPressed = false;

        private PointerEventHandler _pointerPressedHandler;
        private PointerEventHandler _pointerMovedHandler;
        private PointerEventHandler _pointerReleasedHandler;

        public InqCanvasView()
        {

            _pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
            _pointerMovedHandler = new PointerEventHandler(OnPointerMoved);
            _pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

            // Initally, set mode to Inq drawing.
           
        }



        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("asdffffffffffffffffffffffffffffff");
            if (_pointerId != uint.MaxValue)
            {
                e.Handled = true;
                return;
            }

            _pointerId = e.Pointer.PointerId;
     
                CapturePointer(e.Pointer);
            
            //PointerMoved += OnPointerMoved;
            //PointerReleased += OnPointerReleased;
            AddHandler(PointerMovedEvent, _pointerMovedHandler, true);
            AddHandler(PointerReleasedEvent, _pointerReleasedHandler, true);
            IsPressed = true;
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }

  
            
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId != _pointerId)
            {
                e.Handled = true;
                return;
            }
            //PointerMoved -= OnPointerMoved;
            RemoveHandler(PointerMovedEvent,_pointerMovedHandler);
            RemoveHandler(PointerReleasedEvent,_pointerReleasedHandler);
           // PointerReleased -= OnPointerReleased;
            _pointerId = uint.MaxValue;
            if (this.PointerCaptures != null && this.PointerCaptures.Count != 0)
            {
                ReleasePointerCapture(e.Pointer);
            }
            IsPressed = false;

      


            e.Handled = true;
        }
        
        


        

        public bool IsEnabled {
            get
            {
                return _isEnabled;
            }
            set
            { 
                if (value ==true)
                {
                    //PointerPressed += OnPointerPressed;
                    AddHandler(PointerPressedEvent, _pointerPressedHandler, true);

                } else
                {
                    //PointerPressed -= OnPointerPressed;
                    //PointerMoved -= OnPointerMoved;
                    //PointerReleased -= OnPointerReleased;
                    RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
                    RemoveHandler(PointerMovedEvent, _pointerMovedHandler);
                    RemoveHandler(PointerReleasedEvent,_pointerReleasedHandler);
                }
                _isEnabled = value;

                Debug.WriteLine("IsEnabled: " + _isEnabled);
            }
        }

        
    }
}
