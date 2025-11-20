using System;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Infrastructure.NHibernate
{
    /// <summary>
    /// NHibernate helper that builds a Configuration from mapping files and runs SchemaExport.
    /// It attempts a primary connection string and falls back to LocalDB (attach MDF) on failure.
    /// </summary>
    public static class NHibernateHelper
    {
        private static ISessionFactory? _sessionFactory;

        public static Configuration BuildConfiguration(string connectionString)
        {
            var cfg = new Configuration();

            // Basic SQL Server dialect and driver settings suitable for SQL Server 2022
            cfg.DataBaseIntegration(db =>
            {
                db.Dialect<global::NHibernate.Dialect.MsSql2012Dialect>();
                db.BatchSize = 20;
                db.ConnectionString = connectionString;
                // keywords auto import - leave default
                db.IsolationLevel = System.Data.IsolationLevel.ReadCommitted;
                db.Timeout = 30;
            });

            // Load mappings from the Mappings folder
            var baseDir = AppContext.BaseDirectory;
            var repoRoot = Directory.GetCurrentDirectory();

            // Build an expanded set of candidate paths. When running from a different project
            // (e.g. WebSpeedmatch) the mappings live in the solution root under
            // Infrastructure/NHibernate/Mappings, so walk up a few parent levels to find them.
            var candidates = new System.Collections.Generic.List<string>();

            // Common direct candidates
            candidates.Add(Path.Combine(baseDir, "Infrastructure", "NHibernate", "Mappings"));
            candidates.Add(Path.Combine(repoRoot, "Infrastructure", "NHibernate", "Mappings"));
            candidates.Add(Path.Combine(baseDir, "Mappings"));
            candidates.Add(Path.Combine(repoRoot, "bin", "Debug", "net8.0", "Infrastructure", "NHibernate", "Mappings"));

            // Walk up from both baseDir and repoRoot a few levels and test for Infrastructure/NHibernate/Mappings
            string[] starts = { baseDir, repoRoot, Path.GetDirectoryName(typeof(NHibernateHelper).Assembly.Location) ?? baseDir };
            foreach (var start in starts)
            {
                var cur = start ?? baseDir;
                for (int up = 0; up < 5; up++)
                {
                    try
                    {
                        var parent = Path.GetDirectoryName(cur) ?? cur;
                        var candidate = Path.Combine(parent, "Infrastructure", "NHibernate", "Mappings");
                        if (!candidates.Contains(candidate)) candidates.Add(candidate);
                        cur = parent;
                    }
                    catch
                    {
                        // ignore and continue
                    }
                }
            }

            Console.WriteLine($"[NHIBERNATE] BaseDir: {baseDir}");
            Console.WriteLine($"[NHIBERNATE] RepoRoot: {repoRoot}");

            var mappingsPath = candidates.FirstOrDefault(Directory.Exists);
            
            if (mappingsPath == null)
            {
                Console.WriteLine($"[NHIBERNATE ERROR] No se encontró la carpeta Mappings. Rutas intentadas:");
                foreach (var path in candidates)
                {
                    Console.WriteLine($"  - {path} (Exists: {Directory.Exists(path)})");
                }
                throw new Exception("No se pudo encontrar la carpeta de Mappings de NHibernate");
            }

            Console.WriteLine($"[NHIBERNATE] Cargando mappings desde: {mappingsPath}");
            
            if (Directory.Exists(mappingsPath))
            {
                var files = Directory.EnumerateFiles(mappingsPath, "*.hbm.xml", SearchOption.TopDirectoryOnly).ToList();
                Console.WriteLine($"[NHIBERNATE] Archivos .hbm.xml encontrados: {files.Count}");
                
                foreach (var file in files)
                {
                    Console.WriteLine($"[NHIBERNATE] Cargando: {Path.GetFileName(file)}");
                    // Load XML with DTD processing allowed (some .hbm.xml include DTD declaration)
                    var settings = new System.Xml.XmlReaderSettings
                    {
                        // Ignore external DTD resolution for offline use
                        DtdProcessing = System.Xml.DtdProcessing.Ignore,
                        XmlResolver = null
                    };

                    using (var stream = File.OpenRead(file))
                    using (var reader = System.Xml.XmlReader.Create(stream, settings))
                    {
                        cfg.AddXmlReader(reader);
                    }
                }
            }

            return cfg;
        }

        public static void CreateSchemaWithConfiguration(Configuration cfg)
        {
            // Usar SchemaUpdate en lugar de SchemaExport para preservar datos
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);
        }

        /// <summary>
        /// Obtiene una sesión NHibernate para operaciones de base de datos.
        /// Intenta conectar a SQL Server Express, si falla usa LocalDB
        /// </summary>
        public static ISession GetSession()
        {
            if (_sessionFactory == null)
            {
                try
                {
                    // Intentar SQL Server Express primero
                    var primaryConn = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=SpeedMatchDB;Integrated Security=True;Connect Timeout=30;";
                    var cfg = BuildConfiguration(primaryConn);
                    
                    // Intentar crear el schema si no existe (solo en primera ejecución)
                    try
                    {
                        CreateSchemaWithConfiguration(cfg);
                    }
                    catch
                    {
                        // Si falla crear schema, probablemente ya existe
                    }
                    
                    _sessionFactory = cfg.BuildSessionFactory();
                }
                catch
                {
                    // Fallback a LocalDB si SQL Server Express no está disponible
                    var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
                    Directory.CreateDirectory(dataDir);
                    var mdfPath = Path.Combine(dataDir, "SpeedMatchDB.mdf");
                    
                    var dbName = "SpeedMatchDB_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    var localDbConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};AttachDbFilename={mdfPath};Integrated Security=True;Connect Timeout=30;";
                    var cfg = BuildConfiguration(localDbConn);
                    
                    try
                    {
                        CreateSchemaWithConfiguration(cfg);
                    }
                    catch
                    {
                        // Si falla crear schema, probablemente ya existe
                    }
                    
                    _sessionFactory = cfg.BuildSessionFactory();
                }
            }

            return _sessionFactory.OpenSession();
        }

        /// <summary>
        /// Try to create schema using a primary connection string; on failure try LocalDB with MDF attach.
        /// </summary>
        public static void TryCreateSchemaWithFallback(string primaryConnectionString, string mdfPath)
        {
            try
            {
                Console.WriteLine("Trying primary connection (SQL Server)...");
                var cfg = BuildConfiguration(primaryConnectionString);
                CreateSchemaWithConfiguration(cfg);
                Console.WriteLine("Schema created using primary connection.");
                
                // Guardar la sesión factory para uso posterior
                _sessionFactory = cfg.BuildSessionFactory();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Primary creation failed: {ex.Message}\n{ex}");
            }

            // Fallback to LocalDB
            try
            {
                Console.WriteLine("Falling back to LocalDB (creating/attaching MDF)...");
                var dir = Path.GetDirectoryName(mdfPath) ?? ".";
                Directory.CreateDirectory(dir);

                // If file exists try to delete; if locked, create a new timestamped file name to avoid attach conflict
                var useMdf = mdfPath;
                if (File.Exists(mdfPath))
                {
                    try
                    {
                        File.Delete(mdfPath);
                        Console.WriteLine("Removed existing MDF file to recreate a fresh DB.");
                    }
                    catch (Exception)
                    {
                        useMdf = Path.Combine(dir, $"ProjectDatabase_{DateTime.Now:yyyyMMddHHmmss}.mdf");
                        Console.WriteLine($"Existing MDF locked; will create new MDF file: {useMdf}");
                    }
                }

                // Build LocalDB connection string (AttachDBFilename). Use Integrated Security
                var dbName = Path.GetFileNameWithoutExtension(useMdf) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var dirForFiles = Path.GetDirectoryName(useMdf) ?? ".";
                var useLdf = Path.Combine(dirForFiles, Path.GetFileNameWithoutExtension(useMdf) + "_log.ldf");

                // Ensure LocalDB database exists by connecting to master and creating the DB files if needed
                try
                {
                    var masterConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;";
                    using (var cn = new global::System.Data.SqlClient.SqlConnection(masterConn))
                    {
                        cn.Open();
                        using (var cmd = cn.CreateCommand())
                        {
                            // If DB exists drop it to ensure clean schema
                            cmd.CommandText = $"IF DB_ID(N'{dbName}') IS NOT NULL BEGIN ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]; END";
                            cmd.ExecuteNonQuery();

                            // Create database with specified file paths
                            cmd.CommandText = $"CREATE DATABASE [{dbName}] ON (NAME = N'{dbName}', FILENAME = '{useMdf}') LOG ON (NAME = N'{dbName}_log', FILENAME = '{useLdf}')";
                            cmd.ExecuteNonQuery();
                        }
                        cn.Close();
                    }
                    Console.WriteLine($"Created LocalDB database '{dbName}' with files: {useMdf}, {useLdf}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not create LocalDB database files: {ex.Message}");
                    // proceed and let SchemaExport attempt attach; if it fails we'll bubble up
                }

                var localDbConn = $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};AttachDbFilename={useMdf};Integrated Security=True;Connect Timeout=30;";
                var cfg = BuildConfiguration(localDbConn);
                CreateSchemaWithConfiguration(cfg);
                
                // Guardar la sesión factory para uso posterior
                _sessionFactory = cfg.BuildSessionFactory();
                
                Console.WriteLine($"Schema created in LocalDB MDF: {useMdf}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocalDB fallback failed: {ex.Message}\n{ex}");
                throw;
            }
        }
    }
}
