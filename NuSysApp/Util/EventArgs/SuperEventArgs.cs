namespace NuSysApp
{
    public abstract class SuperEventArgs : System.EventArgs
    {
        private readonly string _eventInfo;

        protected SuperEventArgs(string text)
        {
            _eventInfo = text;
           
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

    }
}
