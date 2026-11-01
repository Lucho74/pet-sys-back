using Microsoft.EntityFrameworkCore;
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
        public DbSet<Species> Species { get; set; }
        public DbSet<Breed> Breeds { get; set; }

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

            modelBuilder.Entity<Client>().Property(c => c.Dni).HasMaxLength(20).IsRequired();

            modelBuilder.Entity<Pet>().Property(p => p.Name).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<Consultation>().Property(c => c.Description).HasMaxLength(500).IsRequired();

            // Restrict: no se puede borrar una especie/raza en uso, ni una especie que todavía tiene razas
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany(s => s.Pets)
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany(b => b.Pets)
                .HasForeignKey(p => p.BreedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Breed>()
                .HasOne(b => b.Species)
                .WithMany(s => s.Breeds)
                .HasForeignKey(b => b.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // CI_AI: "Hurón", "huron" y "HURON" se consideran el mismo nombre (índice único y búsquedas), pero se guarda como se escribió
            modelBuilder.Entity<Species>().Property(s => s.Name).HasMaxLength(50).IsRequired().UseCollation("Latin1_General_CI_AI");
            modelBuilder.Entity<Species>().HasIndex(s => s.Name).IsUnique();

            modelBuilder.Entity<Breed>().Property(b => b.Name).HasMaxLength(50).IsRequired().UseCollation("Latin1_General_CI_AI");
            modelBuilder.Entity<Breed>().HasIndex(b => new { b.SpeciesId, b.Name }).IsUnique();

            modelBuilder.Entity<Species>().HasData(
                new Species { Id = 1, Name = "Perro" },
                new Species { Id = 2, Name = "Gato" },
                new Species { Id = 3, Name = "Ave" },
                new Species { Id = 4, Name = "Conejo" },
                new Species { Id = 5, Name = "Roedor" },
                new Species { Id = 6, Name = "Reptil" },
                new Species { Id = 7, Name = "Otra" });

            modelBuilder.Entity<Breed>().HasData(
                new Breed { Id = 1, SpeciesId = 1, Name = "No especificada" },
                new Breed { Id = 2, SpeciesId = 1, Name = "Mestizo" },
                new Breed { Id = 3, SpeciesId = 1, Name = "Labrador Retriever" },
                new Breed { Id = 4, SpeciesId = 1, Name = "Golden Retriever" },
                new Breed { Id = 5, SpeciesId = 1, Name = "Caniche" },
                new Breed { Id = 6, SpeciesId = 1, Name = "Bulldog Francés" },
                new Breed { Id = 7, SpeciesId = 1, Name = "Pastor Alemán" },
                new Breed { Id = 8, SpeciesId = 1, Name = "Beagle" },
                new Breed { Id = 9, SpeciesId = 1, Name = "Dachshund" },
                new Breed { Id = 10, SpeciesId = 1, Name = "Yorkshire Terrier" },
                new Breed { Id = 11, SpeciesId = 1, Name = "Border Collie" },
                new Breed { Id = 12, SpeciesId = 1, Name = "Pitbull" },
                new Breed { Id = 13, SpeciesId = 1, Name = "Chihuahua" },
                new Breed { Id = 14, SpeciesId = 1, Name = "Boxer" },
                new Breed { Id = 15, SpeciesId = 1, Name = "Rottweiler" },
                new Breed { Id = 16, SpeciesId = 1, Name = "Shih Tzu" },
                new Breed { Id = 17, SpeciesId = 2, Name = "No especificada" },
                new Breed { Id = 18, SpeciesId = 2, Name = "Mestizo" },
                new Breed { Id = 19, SpeciesId = 2, Name = "Europeo" },
                new Breed { Id = 20, SpeciesId = 2, Name = "Siamés" },
                new Breed { Id = 21, SpeciesId = 2, Name = "Persa" },
                new Breed { Id = 22, SpeciesId = 2, Name = "Maine Coon" },
                new Breed { Id = 23, SpeciesId = 2, Name = "Bengalí" },
                new Breed { Id = 24, SpeciesId = 2, Name = "Angora" },
                new Breed { Id = 25, SpeciesId = 2, Name = "Sphynx" },
                new Breed { Id = 26, SpeciesId = 2, Name = "British Shorthair" },
                new Breed { Id = 27, SpeciesId = 3, Name = "No especificada" },
                new Breed { Id = 28, SpeciesId = 3, Name = "Canario" },
                new Breed { Id = 29, SpeciesId = 3, Name = "Cacatúa" },
                new Breed { Id = 30, SpeciesId = 3, Name = "Periquito" },
                new Breed { Id = 31, SpeciesId = 3, Name = "Loro" },
                new Breed { Id = 32, SpeciesId = 3, Name = "Agapornis" },
                new Breed { Id = 33, SpeciesId = 3, Name = "Ninfa" },
                new Breed { Id = 34, SpeciesId = 4, Name = "No especificada" },
                new Breed { Id = 35, SpeciesId = 4, Name = "Mestizo" },
                new Breed { Id = 36, SpeciesId = 4, Name = "Belier" },
                new Breed { Id = 37, SpeciesId = 4, Name = "Rex" },
                new Breed { Id = 38, SpeciesId = 4, Name = "Cabeza de León" },
                new Breed { Id = 39, SpeciesId = 4, Name = "Enano Holandés" },
                new Breed { Id = 40, SpeciesId = 5, Name = "No especificada" },
                new Breed { Id = 41, SpeciesId = 5, Name = "Hámster" },
                new Breed { Id = 42, SpeciesId = 5, Name = "Cobayo" },
                new Breed { Id = 43, SpeciesId = 5, Name = "Chinchilla" },
                new Breed { Id = 44, SpeciesId = 5, Name = "Rata" },
                new Breed { Id = 45, SpeciesId = 5, Name = "Ratón" },
                new Breed { Id = 46, SpeciesId = 5, Name = "Jerbo" },
                new Breed { Id = 47, SpeciesId = 6, Name = "No especificada" },
                new Breed { Id = 48, SpeciesId = 6, Name = "Tortuga" },
                new Breed { Id = 49, SpeciesId = 6, Name = "Iguana" },
                new Breed { Id = 50, SpeciesId = 6, Name = "Gecko" },
                new Breed { Id = 51, SpeciesId = 6, Name = "Serpiente" },
                new Breed { Id = 52, SpeciesId = 7, Name = "No especificada" });
        }
    }
}
