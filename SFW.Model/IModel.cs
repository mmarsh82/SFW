using System.Data;

namespace SFW.Model
{
    public interface IModel
    {
        DataTable GetDataTable();
    }
}
