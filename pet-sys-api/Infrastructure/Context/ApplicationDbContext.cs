using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using Domain.Entities;

namespace Infrastructure.Context
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Admin>("Admin")
                .HasValue<Client>("Client")
                .HasValue<Veterinarian>("Veterinarian")
                .HasValue<SysAdmin>("SysAdmin");

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

            modelBuilder.Entity<Consultation>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Pet>()
                .Property(p => p.BirthDate)
                .HasColumnType("date");

            // Stored as JSON in one nvarchar column. The ValueComparer is what makes EF notice
            // changes to the list itself — without it the list is compared by reference and an
            // in-place edit is silently never saved.
            modelBuilder.Entity<Veterinarian>()
                .Property(v => v.WorkShifts)
                .HasConversion(
                    shifts => JsonSerializer.Serialize(shifts, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<List<WorkShift>>(json, (JsonSerializerOptions?)null),
                    new ValueComparer<List<WorkShift>>(
                        (left, right) => left!.SequenceEqual(right!),
                        shifts => shifts.Aggregate(0, (hash, shift) => HashCode.Combine(hash, shift.GetHashCode())),
                        shifts => shifts.ToList()))
                .HasMaxLength(1000);

            modelBuilder.Entity<User>().Property(u => u.FullName).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Password).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Phone).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<Client>().Property(c => c.Dni).HasMaxLength(20).IsRequired();

            modelBuilder.Entity<Pet>().Property(p => p.Name).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Pet>().Property(p => p.Specie).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Pet>().Property(p => p.Breed).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<Consultation>().Property(c => c.Description).HasMaxLength(500).IsRequired();
        }
    }
}
