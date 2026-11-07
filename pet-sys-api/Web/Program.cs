using Application.Interfaces;
using Application.Services;
using Azure.Identity;
using Azure.Storage.Blobs;
using Domain.Constants;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Web.Authorization;
using Web.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, x =>
    {
        x.MigrationsAssembly("Infrastructure");
        x.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretForKey"]!))
        };
        // Un usuario suspendido no puede seguir usando un token emitido antes de la suspensión
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                {
                    context.Fail("Invalid identity.");
                    return;
                }

                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
                {
                    context.Fail("Invalid user identifier.");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                var user = await userRepository.GetByIdAsync(userId);

                if (user == null || user.IsSuspended)
                {
                    context.Fail("User does not exist or is suspended.");
                    return;
                }

                var previousRoleClaims = identity
                    .FindAll(identity.RoleClaimType)
                    .ToList();

                foreach (var claim in previousRoleClaims)
                {
                    identity.RemoveClaim(claim);
                }

                identity.AddClaim(new Claim(identity.RoleClaimType, user.Role.Name));
            }
        };
    });

var authenticatedPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(authenticatedPolicy)
    .SetFallbackPolicy(authenticatedPolicy)
    .AddPolicy(Policies.Staff, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(RoleNames.Veterinarian, RoleNames.Admin, RoleNames.SysAdmin))
    .AddPolicy(Policies.SysAdminOnly, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(RoleNames.SysAdmin))
    .AddPolicy(Policies.ClientOnly, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(RoleNames.Client));

string ServiceUri = builder.Configuration["AzureStorage:ServiceUri"] ?? throw new InvalidOperationException("Falta configurar AzureStorage:ServiceUri."); ;
string ContainerName = builder.Configuration["AzureStorage:ContainerName"] ?? throw new InvalidOperationException("Falta configurar AzureStorage:ContainerName."); ;

builder.Services.AddSingleton<BlobContainerClient>(_ =>
{
    var blobServiceClient = new BlobServiceClient(
            new Uri(ServiceUri),
            new DefaultAzureCredential());

    return blobServiceClient.GetBlobContainerClient(ContainerName);
});


#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IConsultationRepository, ConsultationRepository>();

#endregion

#region Services

builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IPetServices, PetServices>();
builder.Services.AddScoped<IConsultationServices, ConsultationServices>();
builder.Services.AddScoped<IAzureBlobImageStorage, AzureBlobImageStorage>();


#endregion


builder.Services.AddControllers();

// Configurar CORS para permitir cualquier origen, cabecera y m�todo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.AddSecurityDefinition("ApiBearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Acá pegar el token generado al loguearse."
    });

    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiBearerAuth"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitar la política CORS globalmente
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
