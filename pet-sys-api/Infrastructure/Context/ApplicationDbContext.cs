using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<VeterinarianProfile> VeterinarianProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Phone).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = RoleNames.SysAdmin },
                new Role { Id = 2, Name = RoleNames.Admin },
                new Role { Id = 3, Name = RoleNames.Veterinarian },
                new Role { Id = 4, Name = RoleNames.Client }
            );
        }
    }
}
