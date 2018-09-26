using IBMU2.UODOTNET;
using System;
using System.Windows.Input;

//Created by Michael Marsh 9-25-18

namespace SFW.Commands
{
    public sealed class M2kPriorityChange : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Manage 2000 Priority Change ICommand execution
        /// </summary>
        /// <param name="parameter">Order number to use to query</param>
        public void Execute(object parameter)
        {
            try
            {
                if(parameter != null)
                {
                    /*try
                    {
                        using (UniSession uSession = UniObjects.OpenSession("172.16.0.122", "omniquery", "omniquery", $"E:/roi/WCCO.TRAIN", "udcs"))
                        {
                            using (UniFile uFile = uSession.CreateUniFile("WP"))
                            {
                                using (UniDynArray udArray = uFile.Read("213260"))
                                {
                                    udArray.Insert(40, "A");
                                    uFile.Write("213826", udArray);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }*/
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
