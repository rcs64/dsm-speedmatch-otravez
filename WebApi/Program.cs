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
using NHibernate;
using NHSession = NHibernate.ISession;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Cargar JwtSettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JwtSettings no configurado en appsettings.json");
}

// Configurar JWT
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

// Registrar NHibernate session y repositorios (cada request tiene su propia sesión)
builder.Services.AddScoped<NHSession>(_ => NHibernateHelper.GetSession());
builder.Services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<NHSession>()));
builder.Services.AddScoped<IUsuarioRepository>(sp => new UsuarioRepository(sp.GetRequiredService<NHSession>()));

// Registrar CENs y CPs con factories explícitas
builder.Services.AddScoped<UsuarioCEN>(sp => 
    new UsuarioCEN(
        sp.GetRequiredService<IUsuarioRepository>(), 
        sp.GetRequiredService<IUnitOfWork>()
    )
);

builder.Services.AddScoped<AuthCEN>(sp => 
    new AuthCEN(
        sp.GetRequiredService<IUsuarioRepository>(),
        sp.GetRequiredService<IPasswordHasher>(),
        sp.GetRequiredService<IUnitOfWork>()
    )
);

builder.Services.AddScoped<AuthCP>(sp => 
    new AuthCP(
        sp.GetRequiredService<AuthCEN>(),
        sp.GetRequiredService<UsuarioCEN>(),
        sp.GetRequiredService<IPasswordHasher>(),
        sp.GetRequiredService<IUsuarioRepository>(),
        sp.GetRequiredService<IUnitOfWork>()
    )
);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("\n✅ SpeedMatch API corriendo en http://localhost:5000");
Console.WriteLine("📝 Endpoints:");
Console.WriteLine("  POST /api/auth/login");
Console.WriteLine("  POST /api/auth/register");
Console.WriteLine("  POST /api/auth/logout");
Console.WriteLine("  GET  /api/auth/verify\n");

app.Run("http://localhost:5000");
