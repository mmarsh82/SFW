using SFW.Model;
using System;
using System.ComponentModel;
using System.Data;

namespace SFW.Containerization
{
    public class ProductViewModel : ViewModelBase
    {
        #region Properties

        public DataView ContainerView { get; set; }

        private DataRowView _product;
        public DataRowView SelectedProduct
        {
            get
            { return _product; }
            set
            {
                if ((value == null || _product == null) && ContainerView != null && ContainerView.Count >= 1)
                {
                    value = ContainerView[0];
                }
                if (_product == null || value != _product)
                {
                    Loading = true;
                    ShowResults = false;
                    using (BackgroundWorker bw = new BackgroundWorker())
                    {
                        try
                        {
                            bw.DoWork += new DoWorkEventHandler(
                                delegate (object sender, DoWorkEventArgs e)
                                {
                                    if (value.Row.ItemArray[6].ToString() == "N")
                                    {
                                        IthResultsTable = Lot.GetLotHistoryTable(value.Row.ItemArray[1].ToString().Split('|')[0], 1, App.AppSqlCon);
                                    }
                                    else
                                    {
                                        IthResultsTable = Lot.GetLotHistoryTable(value.Row.ItemArray[1].ToString(), value.Row.ItemArray[3].ToString(), 1, App.AppSqlCon);
                                    }
                                    OnPropertyChanged(nameof(IthResultsTable));
                                    Loading = false;
                                    ShowResults = true;
                                });
                            bw.RunWorkerAsync();
                        }
                        catch (Exception)
                        {
                            Loading = false;
                        }
                    }
                }
                _product = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public DataTable IthResultsTable { get; set; }

        private bool _loading;
        public bool Loading
        {
            get
            { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }

        private bool _showResults;
        public bool ShowResults
        {
            get
            { return _showResults; }
            set
            {
                _showResults = value;
                OnPropertyChanged(nameof(ShowResults));
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductViewModel()
        {
            ContainerView = SkuContainer.GetContainerData(App.AppSqlCon).AsDataView();
        }
    }
}
