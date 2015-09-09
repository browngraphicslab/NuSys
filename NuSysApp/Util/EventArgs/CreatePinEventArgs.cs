using System;

namespace NuSysApp
{
    public class CreatePinEventArgs : SuperEventArgs
    {
        public CreatePinEventArgs(string text, PinModel node):base(text)
        {
            CreatedPin = node;
        }

        public PinModel CreatedPin { get; }
    }
}
