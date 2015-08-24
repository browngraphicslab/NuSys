using System;

namespace NuSysApp
{
    public class CanEditChangedEventArg: SuperEventArgs
    {
       

        public CanEditChangedEventArg(string text, Atom.EditStatus editStatus):base(text)
        {
            EditStatus = editStatus;
        }

        public Atom.EditStatus EditStatus { get; }
    }
}
