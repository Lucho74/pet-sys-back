using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Web.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, x =>
        x.MigrationsAssembly("Infrastructure")));

// Add services to the container.

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IConsultationRepository, ConsultationRepository>();

#endregion

#region Services

builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IPetServices, PetServices>();
builder.Services.AddScoped<IConsultationServices, ConsultationServices>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();


#endregion


builder.Services.AddControllers();

// Configurar CORS para permitir cualquier origen, cabecera y método
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
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
