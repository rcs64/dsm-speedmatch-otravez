using System;
using System.IO;
using Infrastructure.NHibernate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicialización de la base de datos: intenta SQL Server Express y cae a LocalDB (MDF) si hace falta
Console.WriteLine("InitializeDb starting...");

// Primary connection string - try local SQL Server named instance first
var primaryConn = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=SpeedMatchDB;Integrated Security=True;Connect Timeout=30;";

var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
Directory.CreateDirectory(dataDir);
var mdfPath = Path.Combine(dataDir, "ProjectDatabase.mdf");

try
{
    NHibernateHelper.TryCreateSchemaWithFallback(primaryConn, mdfPath);
    Console.WriteLine("InitializeDb completado.\n");

    // Ejecutar una comprobación simple de conexión/CRUD
    Console.WriteLine("Iniciando pruebas CRUD...\n");
    using (var session = NHibernateHelper.GetSession())
    {
        // Sesión abierta y cerrada para verificar la conectividad
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error inicializando DB: {ex}");
}

app.Run();
