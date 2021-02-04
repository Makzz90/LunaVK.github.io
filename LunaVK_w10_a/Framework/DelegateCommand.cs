using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace LunaVK.Framework
{
    public class DelegateCommand : ICommand
    {
        private Func<object, bool> canExecute;
        private Action<object> executeAction;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            if (executeAction == null)
                throw new ArgumentNullException("executeAction");
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool flag = true;
            if (this.canExecute != null)
                flag = this.canExecute(parameter);
            return flag;
        }

        public void Execute(object parameter)
        {
            this.executeAction(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, null);
        }
    }
}
