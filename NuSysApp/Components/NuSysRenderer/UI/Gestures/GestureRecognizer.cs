using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class GestureRecognizer
    {


        public GestureRecognizer()
        {



        }


        public virtual void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {

        }

        public virtual void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args)
        {


        }

        public virtual void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {
        }

        public virtual void ProcessMouseWheelEvent(FrameworkElement sender, PointerRoutedEventArgs args)
        {

        }
    }
}