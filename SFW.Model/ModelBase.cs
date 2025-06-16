using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable, INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Model SQL Connection object
        /// </summary>
        public static SqlConnection ModelSqlCon { get; set; }

        /// <summary>
        /// Model master dataset
        /// </summary>
        public static DataSet MasterDataSet { get; set; }

        /// <summary>
        /// List of implemented module objects
        /// </summary>
        public static List<Module> LoadedModules { get; set; }

        /// <summary>
        /// Model default fresh facility
        /// </summary>
        public static int ModelFacility { get; set; }

        /// <summary>
        /// Current database version
        /// </summary>
        public static string DatabaseVersion
        {
            get
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM SFW_Version", ModelSqlCon))
                    {
                        return cmd.ExecuteScalar().ToString();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void OnDispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ModelBase()
        { }
    }
}
