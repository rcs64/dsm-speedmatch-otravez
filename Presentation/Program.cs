using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Interfaces;
using ApplicationCore.Domain.Repositories;
using Infrastructure;
using Infrastructure.NHibernate;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NHSession = NHibernate.ISession;

var builder = WebApplication.CreateBuilder(args);

// Cargar JwtSettings desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JwtSettings no configurado en appsettings.json");
}

// Configurar autenticación JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretBytes = Encoding.UTF8.GetBytes(jwtSettings.Secret);
        var key = new SymmetricSecurityKey(secretBytes);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Registrar servicios
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator>(_ => new JwtTokenGenerator(jwtSettings));

// NHibernate
var nhSession = NHibernateHelper.GetSession();
var uow = new UnitOfWork(nhSession);

builder.Services.AddScoped<ISession>(_ => nhSession);
builder.Services.AddScoped<IUnitOfWork>(_ => uow);

// Repositorios
builder.Services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(nhSession));
builder.Services.AddScoped<IMatchRepository>(_ => new MatchRepository(nhSession));

// CENs
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<AuthCEN>();

// CPs
builder.Services.AddScoped<AuthCP>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("✅ SpeedMatch API corriendo en https://localhost:5001");
Console.WriteLine("📝 Endpoints disponibles:");
Console.WriteLine("  POST   /api/auth/login");
Console.WriteLine("  POST   /api/auth/register");
Console.WriteLine("  POST   /api/auth/logout");
Console.WriteLine("  GET    /api/auth/verify");

app.Run();
