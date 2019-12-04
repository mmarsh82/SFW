namespace SFW.Queries
{
    public class PartLabel_ViewModel : ViewModelBase
    {
        #region Properties

        private string dmdNbr;
        public string DiamondNumber 
        {
            get
            { return dmdNbr; }
            set
            { dmdNbr = value; OnPropertyChanged(nameof(DiamondNumber)); }
        }

        #endregion

        /// <summary>
        /// Part Label ViewModel default constructor
        /// </summary>
        public PartLabel_ViewModel()
        { }

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }
    }
}
