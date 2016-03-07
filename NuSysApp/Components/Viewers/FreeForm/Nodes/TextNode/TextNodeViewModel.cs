using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class TextNodeViewModel : ElementViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;

        #endregion Private Members
        public TextNodeViewModel(ElementController controller) : base(controller)
        {           
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 164, 220, 255));
        }
 
        #region Public Properties     

        public string RtfText
        {
            get
            {
                return _rtf;
            }

            set
            {
                _rtf = value;
                RaisePropertyChanged("RtfText");
            }

        }

        #endregion Public Properties
    }
}