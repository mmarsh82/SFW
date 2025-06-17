using System.Data;

namespace SFW.DataAccess
{
    public class MasterData : Base
    {
        /// <summary>
        /// Model master dataset
        /// </summary>
        public static DataSet MasterDataSet { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MasterData()
        { }
    }
}
