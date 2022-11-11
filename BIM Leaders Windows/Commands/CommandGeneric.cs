using System;
using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    public class CommandGeneric : ICommand
    {
        private Action _action;

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public CommandGeneric(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}