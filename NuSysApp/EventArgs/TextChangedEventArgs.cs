using System;

namespace NuSysApp
{
    public class TextChangedEventArgs: EventArgs
    {
        private readonly string _eventInfo;

        public TextChangedEventArgs(string eventInfo, string text)
        {
            _eventInfo = text;
            Text = text;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public string Text { get; }
    }
}
