namespace NuSysApp2
{
    public class TextInputBlockViewModel : BaseINPC
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }   
        }
    }
}