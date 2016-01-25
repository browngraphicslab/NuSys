using System;

namespace NuSysApp
{
    public class TextChangedEventArgs: System.EventArgs
    {

        public TextChangedEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
