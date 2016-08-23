using Windows.UI.Xaml.Input;

namespace NuSysApp2
{
    public interface IInqMode
    {
        void OnPointerPressed(InqCanvasView inqCanvas, PointerRoutedEventArgs e);
        void OnPointerMoved(InqCanvasView inqCanvas, PointerRoutedEventArgs e);
        void OnPointerReleased(InqCanvasView inqCanvas, PointerRoutedEventArgs e);
    }
}
