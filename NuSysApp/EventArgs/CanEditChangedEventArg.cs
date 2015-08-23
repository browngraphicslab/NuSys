using System;

namespace NuSysApp
{
    public class CanEditChangedEventArg: EventArgs
    {
        private readonly string _eventInfo;

        public CanEditChangedEventArg(string text, Atom.EditStatus editStatus)
        {
            _eventInfo = text;
            EditStatus = editStatus;
        }

        public string GetInfo()
        {
            return _eventInfo;
        }

        public Atom.EditStatus EditStatus { get; }
    }
}
