using Microsoft.EntityFrameworkCore;

namespace JsonStore.Data
{
    public class StoreContext : DbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options)
            : base(options)
        {
        }
    }
}
