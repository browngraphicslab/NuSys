using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NuStarterProject.ViewModels
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
