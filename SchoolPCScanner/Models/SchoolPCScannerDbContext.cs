using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SchoolPCScanner.Models
{
    public class SchoolPCScannerDbContext : IdentityDbContext  // DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<DamageRegistration> DamageRegistrations { get; set; }
        public DbSet<DamageType> DamageTypes { get; set; }
        public DbSet<TerminationRegistration> TerminationRegistrations { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }


        public SchoolPCScannerDbContext(DbContextOptions<SchoolPCScannerDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);


        }
    }
   
}
