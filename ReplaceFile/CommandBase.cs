using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReplaceFile
{
    class CommandBase: ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> ExecuteAction { get; set; }
        public Func<object, bool> CanExecuteFunc { get; set; }

        public CommandBase(Action<object> doexcute, Func<object, bool> canexcute)
        {
            ExecuteAction = doexcute;
            CanExecuteFunc = canexcute;
        }
        //public CommandBase(Action<object> doexcute)
        //{
        //    ExecuteAction = doexcute;
        //}
        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }




    }
}
