using System;
using System.IO;
using Infrastructure.NHibernate;
using InitializeDb.Tests;
using Infrastructure.Repositories;
using Infrastructure;
using ApplicationCore.Domain.CEN;

namespace InitializeDb
{
    class Program
    {
        static void Main(string[] args)
        {
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

                // Ejecutar suite de pruebas CRUD
                Console.WriteLine("Iniciando pruebas CRUD...\n");
                var session = NHibernateHelper.GetSession();
                var testSuite = new CRUDTestSuite(session);
                testSuite.RunAllTests();

                // Ejecutar pruebas de filtros
                Console.WriteLine("\n\nIniciando pruebas de FILTROS...\n");
                TestFiltrosCompleto.Ejecutar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InitializeDb failed: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}
