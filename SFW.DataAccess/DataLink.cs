using System.Data;

namespace SFW.DataAccess
{
    public class DataLink : Base
    {
        /// <summary>
        /// Model master dataset
        /// </summary>
        public static DataSet MasterDataSet { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DataLink()
        { }
    }
}
