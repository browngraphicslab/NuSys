using System;

namespace NuSysApp
{
    public class TextChangedEventArgs: SuperEventArgs
    {

        public TextChangedEventArgs(string eventInfo, string text):base(eventInfo)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
