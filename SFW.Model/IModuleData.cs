using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    interface IModuleData
    {
        DataTable GetTable(int i, SqlConnection sc);
    }
}
