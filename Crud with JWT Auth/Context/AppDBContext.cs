

using Crud_with_JWT_Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Crud_with_JWT_Auth.Context
{
    public class AppDBContext : DbContext
    {
        public AppDBContext() : base()
        {
        }
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public virtual DbSet<Tasks> Tasks { get; set; }
    }
}
