using System;

namespace NuSysApp
{
    public class CanEditChangedEventArg: System.EventArgs
    {
       

        public CanEditChangedEventArg(AtomModel.EditStatus editStatus)
        {
            EditStatus = editStatus;
        }

        public AtomModel.EditStatus EditStatus { get; }
    }
}
