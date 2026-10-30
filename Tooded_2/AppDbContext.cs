using Microsoft.EntityFrameworkCore;

namespace Tooded_2
{
    public class AppDbContext : DbContext
    {
        public DbSet<Toode> Toodetabel { get; set; }
        public DbSet<Kategooria> Kategooriatabel { get; set; }
        public DbSet<Kasutaja> Kasutajatabel { get; set; }
        public DbSet<Muuk> Muugitabel { get; set; }
        public DbSet<ShopSetting> ShopSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=Tooded_2;Trusted_Connection=True;Encrypt=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
            modelBuilder.Entity<Toode>().ToTable("Toodetabel");
            modelBuilder.Entity<Kasutaja>().ToTable("Kasutajatabel");
        }
    }
}