using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    public class AppDbContext : DbContext
    {
        public DbSet<Toode> Toodetabel { get; set; }
        public DbSet<Kategooria> Kategooriatabel { get; set; }
        public DbSet<Kasutaja> Kasutajatabel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
<<<<<<< HEAD
                @"Server=HP-CZC2349HTM;Database=Tooded_DB;Trusted_Connection=True;Encrypt=False");
=======
                @"Server=(localdb)\MSSQLLocalDB;Database=Tooded_DB;Trusted_Connection=True;Encrypt=False");
>>>>>>> dd91bfb (lisan pood ja kastuaja süsteem)
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
            modelBuilder.Entity<Toode>().ToTable("Toodetabel");
            modelBuilder.Entity<Admin>().ToTable("Admin");
        }
    }
}