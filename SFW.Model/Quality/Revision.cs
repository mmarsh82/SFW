using System;
using System.Data;

namespace SFW.Model.Quality
{
    public class Revision : ModelBase
    {
        #region Properties

        public int RevisionId { get; set; }

        private Management.Employee _submitter;
        public Management.Employee Submitter
        {
            get
            { return _submitter; }
            set
            {
                _submitter = value;
                OnPropertyChanged(nameof(Submitter));
            }
        }

        private DateTime _submitDT;
        public DateTime SubmitDateTime
        {
            get
            { return _submitDT; }
            set
            {
                _submitDT = value;
                OnPropertyChanged(nameof(SubmitDateTime));
            }
        }
        public Reason DefectReason { get; set; }
        public Category DefectType { get; set; }
        public Defect DefectSubType { get; set; }

        private int _actLoss;
        public int ActualLoss
        {
            get
            { return _actLoss; }
            set
            {
                _actLoss = value;
                OnPropertyChanged(nameof(ActualLoss));
            }
        }

        private double _actCost;
        public double ActualCost
        {
            get
            { return _actCost; }
            set
            {
                _actCost = value;
                OnPropertyChanged(nameof(ActualCost));
            }
        }

        public Disposition Disposition { get; set; }
        public string Description { get; set; }

        private bool _cur;
        public bool Current
        {
            get
            { return _cur; }
            set
            {
                _cur = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        private FormType _ftype;
        public FormType RevFormType
        {
            get
            { return _ftype; }
            set
            {
                _ftype = value;
                OnPropertyChanged(nameof(RevFormType));
            }
        }

        private int _estLoss;
        public int EstimatedLoss
        {
            get
            { return _estLoss; }
            set
            {
                _estLoss = value;
                OnPropertyChanged(nameof(EstimatedLoss));
                OnPropertyChanged(nameof(EstimatedCost));
            }
        }

        public double EstimatedCost => EstimatedLoss * ProductValue;
        private double ProductValue;


        public Supplier FormSupplier { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Revision()
        { }

        /// <summary>
        /// NCR revisions Overridden constructor
        /// </summary>
        public Revision(Management.Employee submitter, FormType type)
        {
            RevisionId = 1;
            Submitter = submitter;
            SubmitDateTime = DateTime.Now;
            RevFormType = type;
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// <param name="ncrId">NCR Id to load</param>
        /// </summary>
        public Revision(DataRow ncrDataRow, double prodVal)
        {
            RevisionId = ncrDataRow.Field<int>("NcrRevisionId");
            Submitter = new Management.Employee(ncrDataRow.Field<string>("SubmitterId"), false);
            SubmitDateTime = ncrDataRow.Field<DateTime>("RevisionDateTime");
            RevFormType = Enum.TryParse(ncrDataRow.Field<string>("FormType"), out FormType ft) ? ft : FormType.NCR;
            DefectReason = new Reason(ncrDataRow.Field<int>("ReasonId"), ncrDataRow.Field<string>("ReasonDescription"));
            DefectSubType = new Defect(ncrDataRow.Field<int>("SubTypeId"), ncrDataRow.Field<string>("SubTypeDescription"), "");
            DefectType = new Category(ncrDataRow.Field<int>("TypeId"), ncrDataRow.Field<string>("TypeDescription"), RevFormType);
            Disposition = new Disposition(ncrDataRow.Field<int>("DispositionId"), ncrDataRow.Field<string>("DispositionDescription"), ncrDataRow.Field<string>("FormStatus"));
            FormSupplier = new Supplier(ncrDataRow.Field<int>("SupplierId"));
            Description = ncrDataRow.Field<string>("Description");
            Current = ncrDataRow.Field<int>("RevisionFilter") == RevisionId;
            ActualCost = ncrDataRow.SafeGetField<double>("ScrapCost");
            ActualLoss = ncrDataRow.SafeGetField<int>("ScrapQuantity");
            ProductValue = prodVal;
            EstimatedLoss = ncrDataRow.SafeGetField<int>("EstimatedLoss");
        }
    }
}
