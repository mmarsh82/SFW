using System;
using System.Reflection;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ShowNewWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _win = parameter.ToString();
            var type = Assembly.GetExecutingAssembly().GetType(_win);
            var oHandle = Activator.CreateInstance(null, _win);
            var mInfo = type.GetMethod("Show");
            mInfo.Invoke(oHandle.Unwrap(), null);
        }

        public bool CanExecute(object parameter) => parameter != null;
    }
}
