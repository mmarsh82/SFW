using System;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable
    {
        #region Properties

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
            
        }
    }
}
