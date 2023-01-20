using SFW.Tools;

namespace SFW.Commands
{
    public class DiamondEntry
    {
        #region Properties

        public static string DiamondNumber;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DiamondEntry()
        { }

        /// <summary>
        /// Shows the diamond entry tool window
        /// </summary>
        /// <returns>diaomnd number</returns>
        public static string Show()
        {
            DiamondNumber = string.Empty;
            var _win = new DiamondEntry_View();
            _win.DataContext = new DiamondEntry_ViewModel();
            _win.ShowDialog();
            return DiamondNumber;
        }
    }
}
