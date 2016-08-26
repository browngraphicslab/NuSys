using System;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : ElementViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;

        #endregion Private Members

        public string Text { get; set; }

        public delegate void TextBindingChangedHandler(object source, string text);
        public event TextBindingChangedHandler TextBindingChanged;
        public delegate void TextUnselectedHandler(object source);
        public event TextUnselectedHandler TextUnselected;

        private TextNodeController _textNodeController;

        public TextNodeViewModel(ElementController controller) : base(controller)
        {           
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 164, 220, 255));
            Text = controller.LibraryElementController?.Data;
            ((TextNodeController) controller).TextChanged += TextChanged;
            ((TextNodeController) controller).SelectionChanged += SelectionChanged;
            controller.Disposed += ControllerOnDisposed;
            if(Controller.Model.Height < 45)
            {
                Controller.SetSize(Controller.Model.Width, 45);
            }
        }

        private void SelectionChanged(object source, bool selected)
        {
            if (!selected)
            {
                TextUnselected?.Invoke(this);
            }
        }

        private void ControllerOnDisposed(object source, object args)
        {
            if (Controller != null)
            {
                ((TextNodeController) Controller).TextChanged -= TextChanged;
                ((TextNodeController) Controller).SelectionChanged -= SelectionChanged;
                Controller.Disposed -= ControllerOnDisposed;
            }
        }

        private void TextChanged (object sender, string text)
        {
            TextBindingChanged?.Invoke(this, text);
            Text = text;
        }
    }
}