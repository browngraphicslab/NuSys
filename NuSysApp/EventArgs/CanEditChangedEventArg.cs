using System;

namespace NuSysApp
{
    public class CanEditChangedEventArg: SuperEventArgs
    {
       

        public CanEditChangedEventArg(string text, AtomModel.EditStatus editStatus):base(text)
        {
            EditStatus = editStatus;
        }

        public AtomModel.EditStatus EditStatus { get; }
    }
}
