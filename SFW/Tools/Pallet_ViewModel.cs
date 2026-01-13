using SFW.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using SFW.Model.Product;

namespace SFW.Tools
{
    public class Pallet_ViewModel : ViewModelBase
    {
        #region Properties

        public DataTable PalletTable { get; set; }

        public List<SkuContainer.TableChanges> ChangeList { get; set; }

        private RelayCommand _saveICommand;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Pallet_ViewModel()
        {
            PalletTable = SkuContainer.GetPalletTable(App.AppSqlCon);
            PalletTable.PrimaryKey = new DataColumn[1] { PalletTable.Columns[0] };
            PalletTable.RowDeleting += PalletTable_RowChange;
            PalletTable.RowDeleted += PalletTable_RowChange;
            PalletTable.RowChanged += PalletTable_RowChange;
            ChangeList = new List<SkuContainer.TableChanges>();
        }

        /// <summary>
        /// Tracks all row additions and edits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Change events with changed object</param>
        private void PalletTable_RowChange(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Delete && !e.Row.HasErrors)
            {
                e.Row.RowError = e.Row.ItemArray[0].ToString();
            }
            ChangeList.Add(new SkuContainer.TableChanges { Action = e.Action, TableRow = e.Row });
        }

        #region Save ICommand

        public ICommand SaveICommand
        {
            get
            {
                if (_saveICommand == null)
                {
                    _saveICommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
                }
                return _saveICommand;
            }
        }

        private void SaveCommandExecute(object parameter)
        {
            SkuContainer.ChangePalletTable(ChangeList, App.AppSqlCon);
            PalletTable.AcceptChanges();
            ChangeList.Clear();
        }

        private bool SaveCommandCanExecute(object parameter) => ChangeList.Count > 0;

        #endregion
    }
}
