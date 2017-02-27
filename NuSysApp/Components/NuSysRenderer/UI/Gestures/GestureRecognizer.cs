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
    public interface GestureRecognizer
    {





        void ProcessDownEvent(FrameworkElement sender, PointerRoutedEventArgs args);

        void ProcessMoveEvents(FrameworkElement sender, PointerRoutedEventArgs args);

        void ProcessUpEvent(FrameworkElement sender, PointerRoutedEventArgs args);

        void ProcessMouseWheelEvent(FrameworkElement sender, PointerRoutedEventArgs args);

        void ProcessExitedEvent(FrameworkElement sender, PointerRoutedEventArgs args);

    }
}