using Microsoft.EntityFrameworkCore;

namespace SFW.DataAccess.Data
{
    public class MachineContext : DbContext
    {
        public MachineContext(DbContextOptions options) : base(options) { }
    }
}
