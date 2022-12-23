using System;
using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    public class CommandWindow : ICommand
    {
        private Action<Window> _action;

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public CommandWindow(Action<Window> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter as Window);
        }
    }
}