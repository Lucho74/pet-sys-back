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
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Pets)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Pet)
                .WithMany(p => p.Consultations)
                .HasForeignKey(c => c.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Veterinarian)
                .WithMany(v => v.Consultations)
                .HasForeignKey(c => c.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pet>()
                .Property(p => p.BirthDate)
                .HasColumnType("date");

            modelBuilder.Entity<Consultation>()
                .Property(c => c.Date)
                .HasColumnType("datetime2");

            modelBuilder.Entity<User>().Property(u => u.FullName).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Password).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Phone).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Pet>().Property(p => p.Name).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Pet>().Property(p => p.Specie).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Pet>().Property(p => p.Breed).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<Consultation>().Property(c => c.Description).HasMaxLength(500).IsRequired();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = RoleNames.SysAdmin },
                new Role { Id = 2, Name = RoleNames.Admin },
                new Role { Id = 3, Name = RoleNames.Veterinarian },
                new Role { Id = 4, Name = RoleNames.Client }
            );
        }
    }
}
