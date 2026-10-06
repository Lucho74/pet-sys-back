using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var sourceConnectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Seteá la variable de entorno SUPABASE_CONNECTION_STRING con la cadena de conexión de Postgres (Supabase).");

var destConnectionString = Environment.GetEnvironmentVariable("DEV_SQLSERVER_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Seteá la variable de entorno DEV_SQLSERVER_CONNECTION_STRING con la cadena de conexión de la base de dev en Azure SQL.");

var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(sourceConnectionString)
    .Options;

var destOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(destConnectionString)
    .Options;

using var sourceContext = new ApplicationDbContext(sourceOptions);
using var destContext = new ApplicationDbContext(destOptions);

Console.WriteLine("Aplicando migraciones en la base de destino...");
await destContext.Database.MigrateAsync();

var users = await sourceContext.Users.AsNoTracking().ToListAsync();
var pets = await sourceContext.Pets.AsNoTracking().ToListAsync();
var consultations = await sourceContext.Consultations.AsNoTracking().ToListAsync();

Console.WriteLine($"Leídos: {users.Count} usuarios, {pets.Count} mascotas, {consultations.Count} consultas.");

await CopyWithIdentityInsertAsync(destContext, "Users", () => destContext.Users.AddRange(users));
await CopyWithIdentityInsertAsync(destContext, "Pets", () => destContext.Pets.AddRange(pets));
await CopyWithIdentityInsertAsync(destContext, "Consultations", () => destContext.Consultations.AddRange(consultations));

Console.WriteLine("Copia completa.");

static async Task CopyWithIdentityInsertAsync(ApplicationDbContext context, string tableName, Action addRange)
{
    await using var transaction = await context.Database.BeginTransactionAsync();

    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] ON");

    addRange();
    await context.SaveChangesAsync();

    await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] OFF");

    await transaction.CommitAsync();
}
