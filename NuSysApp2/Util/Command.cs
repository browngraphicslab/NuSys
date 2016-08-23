using System;
using System.Windows.Input;

namespace NuSysApp2
{
    public class Command : ICommand
    {
        private Action<object> execute;
        private Predicate<object> canExecute;
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this.canExecute != null && this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Undo(object parameter)
        {

        }

        public void Redo(object parameter)
        {

        }
    }
}
