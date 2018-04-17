using System;
using System.Data.SqlClient;

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable
    {
        #region Properties

        /// <summary>
        /// SQL Connection for all model data
        /// </summary>
        public static SqlConnection SqlModelCon { get; private set; }

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

        /// <summary>
        /// Model Base Constructor
        /// </summary>
        public ModelBase()
        {
            if (SqlModelCon == null)
            {
                SqlModelCon = new SqlConnection("Server=SQL-WCCO;User ID=omni;Password=Public2017@WORK!;Database=WCCO_MAIN;Connection Timeout=5");
            }
        }
    }
}
