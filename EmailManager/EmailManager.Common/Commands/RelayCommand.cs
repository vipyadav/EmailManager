using System;
using System.Windows.Input;

namespace EmailManager.Common.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execteMethod) : this(execteMethod, x => true) { }

        public RelayCommand(Action<object> execteMethod, Predicate<object> canexecuteMethod)
        {
            _execute = execteMethod;
            _canExecute = canexecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

    }
}
