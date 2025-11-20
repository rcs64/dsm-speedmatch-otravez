using System;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using Infrastructure;
using Infrastructure.Repositories;
using NHibernate;

namespace InitializeDb
{
    public class TestFiltrosCompleto
    {
        public static void Ejecutar()
        {
            var session = Infrastructure.NHibernate.NHibernateHelper.GetSession();
            
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         CREANDO DATOS DE PRUEBA PARA FILTROS                     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝\n");

            var uow = new UnitOfWork(session);
            var usuarioRepo = new UsuarioRepository(session);
            var matchRepo = new MatchRepository(session);
            var notificacionRepo = new NotificacionRepository(session);
            var fotoRepo = new FotoRepository(session);
            var ubicacionRepo = new UbicacionRepository(session);

            var usuarioCEN = new UsuarioCEN(usuarioRepo, uow);
            var matchCEN = new MatchCEN(matchRepo, usuarioRepo, uow);
            var notificacionCEN = new NotificacionCEN(notificacionRepo, uow);
            var fotoCEN = new FotoCEN(fotoRepo, usuarioRepo, uow);
            var ubicacionCEN = new UbicacionCEN(ubicacionRepo, usuarioRepo, uow);

            try
            {
                // Generar timestamp único para evitar duplicados
                var timestamp = DateTime.Now.Ticks;
                
                // Crear usuarios de prueba
                var usuario1 = usuarioCEN.Crear("Ana García", $"ana.filtro{timestamp}@test.com", "pass123", Plan.Premium, 10);
                usuario1.FechaNacimiento = DateTime.Now.AddYears(-25);
                usuario1.Genero = Genero.Mujer;
                session.Update(usuario1);

                var usuario2 = usuarioCEN.Crear("Carlos López", $"carlos.filtro{timestamp}@test.com", "pass123", Plan.Gratuito, 5);
                usuario2.FechaNacimiento = DateTime.Now.AddYears(-30);
                usuario2.Genero = Genero.Hombre;
                session.Update(usuario2);

                var usuario3 = usuarioCEN.Crear("María Pérez", $"maria.filtro{timestamp}@test.com", "pass123", Plan.Premium, 15);
                usuario3.FechaNacimiento = DateTime.Now.AddYears(-22);
                usuario3.Genero = Genero.Mujer;
                session.Update(usuario3);

                var usuario4 = usuarioCEN.Crear("Juan Martínez", $"juan.filtro{timestamp}@test.com", "pass123", Plan.Gratuito, 8);
                usuario4.FechaNacimiento = DateTime.Now.AddYears(-28);
                usuario4.Genero = Genero.Hombre;
                session.Update(usuario4);

                var usuario5 = usuarioCEN.Crear("Laura Sánchez", $"laura.filtro{timestamp}@test.com", "pass123", Plan.Premium, 20);
                usuario5.FechaNacimiento = DateTime.Now.AddYears(-35);
                usuario5.Genero = Genero.Mujer;
                usuario5.Baneado = true;
                session.Update(usuario5);

                session.Flush();
                Console.WriteLine("✓ Creados 5 usuarios de prueba");

                // Crear ubicaciones
                ubicacionCEN.Crear(40.416775, -3.703790, usuario1.Id); // Madrid
                ubicacionCEN.Crear(41.385064, 2.173404, usuario2.Id);  // Barcelona
                ubicacionCEN.Crear(40.420000, -3.700000, usuario3.Id); // Madrid cerca
                ubicacionCEN.Crear(41.390000, 2.180000, usuario4.Id);  // Barcelona cerca
                ubicacionCEN.Crear(40.450000, -3.750000, usuario5.Id); // Madrid lejos

                Console.WriteLine("✓ Creadas 5 ubicaciones");

                // Crear fotos
                fotoCEN.Crear("https://ejemplo.com/fotos/ana1.jpg", usuario1.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/ana2.jpg", usuario1.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/carlos1.jpg", usuario2.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/maria1.jpg", usuario3.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/maria2.jpg", usuario3.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/maria3.jpg", usuario3.Id);
                fotoCEN.Crear("https://ejemplo.com/fotos/juan1.jpg", usuario4.Id);
                fotoCEN.Crear("https://otro-servidor.com/laura1.jpg", usuario5.Id);

                Console.WriteLine("✓ Creadas 8 fotos");

                // Crear matches
                var match1 = matchCEN.CrearMatchMutuo(usuario1.Id, usuario2.Id);
                match1.EsSuperlike = true;
                session.Update(match1);

                matchCEN.Crear(usuario1, usuario3, true);
                matchCEN.CrearMatchMutuo(usuario3.Id, usuario4.Id);
                matchCEN.Crear(usuario2, usuario4, true);

                session.Flush();
                Console.WriteLine("✓ Creados 4 matches");

                // Crear notificaciones
                notificacionCEN.Crear(usuario1, "¡Tienes un nuevo match!");
                notificacionCEN.Crear(usuario1, "María te ha dado like");
                
                var notif3 = notificacionCEN.Crear(usuario2, "¡Match confirmado con Ana!");
                notif3.Likes = 5;
                session.Update(notif3);

                var notif4 = notificacionCEN.Crear(usuario3, "Juan te ha enviado un superlike");
                notif4.Likes = 10;
                session.Update(notif4);

                var notif5 = notificacionCEN.Crear(usuario4, "Tienes 3 nuevos likes");
                notif5.Likes = 15;
                session.Update(notif5);

                session.Flush();
                Console.WriteLine("✓ Creadas 5 notificaciones\n");

                // EJECUTAR PRUEBAS
                EjecutarPruebas(usuarioCEN, matchCEN, notificacionCEN, fotoCEN, session);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.ResetColor();
            }
            finally
            {
                session?.Dispose();
            }
        }

        private static void EjecutarPruebas(UsuarioCEN usuarioCEN, MatchCEN matchCEN, 
            NotificacionCEN notificacionCEN, FotoCEN fotoCEN, ISession session)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              EJECUTANDO PRUEBAS DE FILTROS                        ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝\n");

            int totalPruebas = 0;
            int exitosas = 0;
            int fallidas = 0;

            // Obtener usuarios creados en esta sesión (los últimos 5 por nombre)
            var usuarioAna = session.Query<Usuario>().Where(u => u.Nombre == "Ana García").FirstOrDefault();
            var usuarioCarlos = session.Query<Usuario>().Where(u => u.Nombre == "Carlos López").FirstOrDefault();
            var usuarioMaria = session.Query<Usuario>().Where(u => u.Nombre == "María Pérez").FirstOrDefault();
            var usuarioJuan = session.Query<Usuario>().Where(u => u.Nombre == "Juan Martínez").FirstOrDefault();
            var usuarioLaura = session.Query<Usuario>().Where(u => u.Nombre == "Laura Sánchez").FirstOrDefault();

            // PRUEBAS UsuarioReadFilter
            Console.WriteLine("▶ PRUEBAS: UsuarioReadFilter\n");

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Filtro edad 20-26 años", () =>
            {
                var filtro = new UsuarioReadFilter { EdadMinima = 20, EdadMaxima = 26 };
                var resultado = usuarioCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Filtro edad 20-26 años → {resultado.Count} usuarios");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Filtro género Mujer", () =>
            {
                var filtro = new UsuarioReadFilter { Genero = Genero.Mujer, SoloNoBaneados = true };
                var resultado = usuarioCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Género Mujer (no baneadas) → {resultado.Count} usuarias");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Filtro ubicación Madrid", () =>
            {
                var filtro = new UsuarioReadFilter 
                { 
                    Latitud = 40.416775, 
                    Longitud = -3.703790, 
                    RadioMetros = 5000 
                };
                var resultado = usuarioCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Ubicación Madrid (5km) → {resultado.Count} usuarios");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Solo Premium", () =>
            {
                var filtro = new UsuarioReadFilter { SoloPremium = true, SoloNoBaneados = true };
                var resultado = usuarioCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Solo Premium → {resultado.Count} usuarios");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Búsqueda nombre 'María'", () =>
            {
                var filtro = new UsuarioReadFilter { NombreBusqueda = "María" };
                var resultado = usuarioCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Búsqueda 'María' → {resultado.Count} resultado(s)");
            });

            // PRUEBAS MatchReadFilter
            Console.WriteLine("\n▶ PRUEBAS: MatchReadFilter\n");

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Matches por emisor", () =>
            {
                if (usuarioAna != null)
                {
                    var filtro = new MatchReadFilter { EmisorId = usuarioAna.Id };
                    var resultado = matchCEN.DamePorFiltros(filtro).ToList();
                    Console.WriteLine($"  ✓ TEST: Matches de Ana (emisor) → {resultado.Count} matches");
                }
                else
                {
                    throw new Exception("Usuario Ana no encontrado");
                }
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Solo matches confirmados", () =>
            {
                var filtro = new MatchReadFilter { SoloConfirmados = true };
                var resultado = matchCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Matches confirmados → {resultado.Count} matches");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Solo superlikes", () =>
            {
                var filtro = new MatchReadFilter { EsSuperlike = true };
                var resultado = matchCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Superlikes → {resultado.Count} superlike(s)");
            });

            // PRUEBAS NotificacionReadFilter
            Console.WriteLine("\n▶ PRUEBAS: NotificacionReadFilter\n");

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Notificaciones por receptor", () =>
            {
                if (usuarioAna != null)
                {
                    var filtro = new NotificacionReadFilter { ReceptorId = usuarioAna.Id };
                    var resultado = notificacionCEN.DamePorFiltros(filtro).ToList();
                    Console.WriteLine($"  ✓ TEST: Notificaciones de Ana → {resultado.Count} notificaciones");
                }
                else
                {
                    throw new Exception("Usuario Ana no encontrado");
                }
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Filtro por rango de likes", () =>
            {
                var filtro = new NotificacionReadFilter { LikesMinimo = 5, LikesMaximo = 15 };
                var resultado = notificacionCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Notificaciones 5-15 likes → {resultado.Count} notificaciones");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Búsqueda por mensaje", () =>
            {
                var filtro = new NotificacionReadFilter { MensajeBusqueda = "match" };
                var resultado = notificacionCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Mensaje 'match' → {resultado.Count} notificaciones");
            });

            // PRUEBAS FotoReadFilter
            Console.WriteLine("\n▶ PRUEBAS: FotoReadFilter\n");

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Fotos por usuario", () =>
            {
                if (usuarioMaria != null)
                {
                    var filtro = new FotoReadFilter { UsuarioId = usuarioMaria.Id };
                    var resultado = fotoCEN.DamePorFiltros(filtro).ToList();
                    Console.WriteLine($"  ✓ TEST: Fotos de María → {resultado.Count} fotos");
                }
                else
                {
                    throw new Exception("Usuario María no encontrado");
                }
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Búsqueda por URL", () =>
            {
                var filtro = new FotoReadFilter { UrlBusqueda = "ejemplo.com" };
                var resultado = fotoCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: URL 'ejemplo.com' → {resultado.Count} fotos");
            });

            RunTest(ref totalPruebas, ref exitosas, ref fallidas, "Paginación de fotos", () =>
            {
                var filtro = new FotoReadFilter { Limite = 3, Offset = 0, OrdenarPor = "Id", Direccion = "DESC" };
                var resultado = fotoCEN.DamePorFiltros(filtro).ToList();
                Console.WriteLine($"  ✓ TEST: Paginación (3 fotos) → {resultado.Count} fotos");
            });

            // RESUMEN
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    RESUMEN DE PRUEBAS                             ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Total de pruebas:      {totalPruebas,3}                                       ║");
            Console.WriteLine($"║ Pruebas exitosas:      {exitosas,3}                                       ║");
            Console.WriteLine($"║ Pruebas fallidas:      {fallidas,3}                                       ║");
            var porcentaje = totalPruebas > 0 ? (exitosas * 100.0 / totalPruebas) : 0;
            Console.WriteLine($"║ Porcentaje éxito:      {porcentaje:F2}%                                 ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝\n");

            if (fallidas > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  ALGUNAS PRUEBAS FALLARON");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ TODAS LAS PRUEBAS PASARON EXITOSAMENTE");
                Console.ResetColor();
            }
        }

        private static void RunTest(ref int total, ref int exitosas, ref int fallidas, string nombre, Action test)
        {
            total++;
            try
            {
                test();
                exitosas++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ TEST FALLÓ: {nombre} - {ex.Message}");
                fallidas++;
            }
        }
    }
}
