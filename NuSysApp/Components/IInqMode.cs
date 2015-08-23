using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public interface IInqMode
    {
        void OnPointerPressed(InqCanvas inqCanvas, PointerRoutedEventArgs e);
        void OnPointerMoved(InqCanvas inqCanvas, PointerRoutedEventArgs e);
        void OnPointerReleased(InqCanvas inqCanvas, PointerRoutedEventArgs e);
    }
}
