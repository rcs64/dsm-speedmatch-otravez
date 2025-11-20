using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using NHibernate;
using NHibernate.Cfg;

namespace InitializeDb.Tests
{
    /// <summary>
    /// Pruebas completas del CRUD para todas las entidades.
    /// Ejecuta operaciones CRUD y almacena resultados en JSON.
    /// </summary>
    public class CRUDTestSuite
    {
        private readonly ISession _session;
        private readonly IUnitOfWork _uow;

        // CENs
        private readonly UsuarioCEN _usuarioCEN;
        private readonly FotoCEN _fotoCEN;
        private readonly MatchCEN _matchCEN;
        private readonly NotificacionCEN _notificacionCEN;
        private readonly PreferenciasCEN _preferenciasCEN;
        private readonly UbicacionCEN _ubicacionCEN;
        private readonly AdminCEN _adminCEN;

        // CPs
        private readonly IniciarMatchCP _iniciarMatchCP;
        private readonly CorresponderMatchCP _corresponderMatchCP;
        private readonly SuperlikeCP _superlikeCP;

        // Superlikes
        private readonly SuperlikeCEN _superlikeCEN;

        // Resultados de pruebas
        private List<TestResult> _testResults = new List<TestResult>();

        public CRUDTestSuite(ISession session)
        {
            _session = session;
            _uow = new UnitOfWork(session);

            // Inicializar repositorios
            var usuarioRepo = new UsuarioRepository(session);
            var fotoRepo = new FotoRepository(session);
            var matchRepo = new MatchRepository(session);
            var notificacionRepo = new NotificacionRepository(session);
            var preferenciasRepo = new PreferenciasRepository(session);
            var ubicacionRepo = new UbicacionRepository(session);
            var adminRepo = new AdminRepository(session);

            // Inicializar CENs
            _usuarioCEN = new UsuarioCEN(usuarioRepo, _uow);
            _fotoCEN = new FotoCEN(fotoRepo, usuarioRepo, _uow);
            _matchCEN = new MatchCEN(matchRepo, usuarioRepo, _uow);
            _notificacionCEN = new NotificacionCEN(notificacionRepo, _uow);
            _preferenciasCEN = new PreferenciasCEN(preferenciasRepo, _uow);
            _ubicacionCEN = new UbicacionCEN(ubicacionRepo, usuarioRepo, _uow);
            _adminCEN = new AdminCEN(adminRepo, _uow);

            // Inicializar CPs
            _iniciarMatchCP = new IniciarMatchCP(_matchCEN, _usuarioCEN, _notificacionCEN, _uow);
            _corresponderMatchCP = new CorresponderMatchCP(_matchCEN, _usuarioCEN, _notificacionCEN, matchRepo, _uow);
            
            // Inicializar Superlikes
            _superlikeCEN = new SuperlikeCEN(usuarioRepo, matchRepo, _uow);
            _superlikeCP = new SuperlikeCP(_matchCEN, _usuarioCEN, _notificacionCEN, usuarioRepo, matchRepo, _uow);
        }

        public void RunAllTests()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         PRUEBAS CRUD COMPLETAS - TODAS LAS ENTIDADES        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            TestUsuarios();
            TestPreferencias();
            TestUbicaciones();
            TestFotos();
            TestMatches();
            TestIniciarMatch();
            TestCorresponderMatch();
            TestSuperlikes();
            TestAuth();
            TestNotificaciones();
            TestAdmins();

            // Generar JSON
            GenerateJsonReport();
        }

        private void TestUsuarios()
        {
            Console.WriteLine("🧪 Probando UsuarioCEN...\n");

            try
            {
                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando usuarios...");
                var usuario1 = _usuarioCEN.Crear("Juan Pérez", "juan@test.com", "Pass123!", Plan.Premium);
                LogTest("Usuario CREATE", true, $"ID: {usuario1.Id}, Nombre: {usuario1.Nombre}, Superlikes: {usuario1.Superlikes}");

                var usuario2 = _usuarioCEN.Crear("María García", "maria@test.com", "Pass456!", Plan.Gratuito);
                LogTest("Usuario CREATE 2", true, $"ID: {usuario2.Id}, Plan: Gratuito, Superlikes: {usuario2.Superlikes}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando usuarios...");
                var todos = _usuarioCEN.DameTodos().ToList();
                LogTest("Usuario READ ALL", true, $"Total usuarios: {todos.Count}");

                var porId = _usuarioCEN.DamePorId(usuario1.Id);
                LogTest("Usuario READ BY ID", true, $"Encontrado: {porId?.Nombre}");

                var porEmail = _usuarioCEN.DamePorEmail("juan@test.com");
                LogTest("Usuario READ BY EMAIL", true, $"Email encontrado: {porEmail?.Email}");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando usuario...");
                _usuarioCEN.Modificar(usuario1.Id, "Juan Carlos Pérez", "juan.carlos@test.com");
                var usuarioModificado = _usuarioCEN.DamePorId(usuario1.Id);
                LogTest("Usuario UPDATE", true, $"Nuevo nombre: {usuarioModificado?.Nombre}, Email: {usuarioModificado?.Email}");

                // BUSINESS LOGIC
                Console.WriteLine("  ✓ BUSINESS LOGIC: Probando operaciones especiales...");
                _usuarioCEN.DarLike(usuario1.Id);
                var usuarioConLike = _usuarioCEN.DamePorId(usuario1.Id);
                LogTest("Usuario DAR LIKE", true, $"Likes: {usuarioConLike?.LikesRecibidos}");

                _usuarioCEN.Banear(usuario2.Id);
                var usuarioBaneado = _usuarioCEN.DamePorId(usuario2.Id);
                LogTest("Usuario BANEAR", true, $"Baneado: {usuarioBaneado?.Baneado}");

                _usuarioCEN.Desbanear(usuario2.Id);
                var usuarioDesbaneado = _usuarioCEN.DamePorId(usuario2.Id);
                LogTest("Usuario DESBANEAR", true, $"Baneado: {usuarioDesbaneado?.Baneado}");

                Console.WriteLine("  ✅ UsuarioCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Usuario", false, ex.Message);
            }
        }

        private void TestPreferencias()
        {
            Console.WriteLine("🧪 Probando PreferenciasCEN...\n");

            try
            {
                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando preferencias...");
                var pref1 = _preferenciasCEN.Crear(
                    OrientacionSexual.Heterosexual,
                    PrefConocer.Mujer,
                    orientacionMostrar: true
                );
                LogTest("Preferencias CREATE", true, $"ID: {pref1.Id}, Orientación: {pref1.Orientacion}");

                var pref2 = _preferenciasCEN.Crear(
                    OrientacionSexual.Bisexual,
                    PrefConocer.Todos,
                    orientacionMostrar: false
                );
                LogTest("Preferencias CREATE 2", true, $"ID: {pref2.Id}, Conocer: {pref2.Conocer}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando preferencias...");
                var todas = _preferenciasCEN.DameTodos().ToList();
                LogTest("Preferencias READ ALL", true, $"Total preferencias: {todas.Count}");

                var porId = _preferenciasCEN.DamePorId(pref1.Id);
                LogTest("Preferencias READ BY ID", true, $"Encontrada, Orientación: {porId?.Orientacion}");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando preferencias...");
                _preferenciasCEN.Modificar(pref1.Id, OrientacionSexual.Asexual, PrefConocer.Otro, true);
                var prefModificada = _preferenciasCEN.DamePorId(pref1.Id);
                LogTest("Preferencias UPDATE", true, $"Nueva orientación: {prefModificada?.Orientacion}");

                Console.WriteLine("  ✅ PreferenciasCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Preferencias", false, ex.Message);
            }
        }

        private void TestUbicaciones()
        {
            Console.WriteLine("🧪 Probando UbicacionCEN...\n");

            try
            {
                // Obtener usuario y recargar desde BD para asegurar persistencia
                var usuario = _usuarioCEN.DameTodos().FirstOrDefault();
                if (usuario == null)
                {
                    LogTest("Ubicación (SKIP)", false, "No hay usuarios disponibles");
                    return;
                }

                // Recargar usuario para asegurar que está en sesión
                _session.Flush();
                usuario = _usuarioCEN.DamePorId(usuario.Id);
                if (usuario == null)
                {
                    LogTest("Ubicación (ERROR)", false, "Usuario no persisted correctamente");
                    return;
                }

                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando ubicaciones...");
                var ubi1 = _ubicacionCEN.Crear(40.4168, -3.7038, usuario.Id); // Madrid
                LogTest("Ubicación CREATE", true, $"ID: {ubi1.Id}, Coords: ({ubi1.Lat}, {ubi1.Lon})");

                var ubi2 = _ubicacionCEN.Crear(41.3851, 2.1734, usuario.Id); // Barcelona
                LogTest("Ubicación CREATE 2", true, $"ID: {ubi2.Id}, Barcelona");

                // READ
                Console.WriteLine("  ✓ READ: Consultando ubicaciones...");
                var todas = _ubicacionCEN.DameTodos().ToList();
                LogTest("Ubicación READ ALL", true, $"Total ubicaciones: {todas.Count}");

                var porId = _ubicacionCEN.DamePorId(ubi1.Id);
                LogTest("Ubicación READ BY ID", true, $"Coords: ({porId?.Lat}, {porId?.Lon})");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando ubicación...");
                _ubicacionCEN.Modificar(ubi1.Id, 40.5200, -3.7500);
                var ubiModificada = _ubicacionCEN.DamePorId(ubi1.Id);
                LogTest("Ubicación UPDATE", true, $"Nuevas coords: ({ubiModificada?.Lat}, {ubiModificada?.Lon})");

                // DELETE
                Console.WriteLine("  ✓ DELETE: Eliminando ubicación...");
                _ubicacionCEN.Eliminar(ubi2.Id);
                var ubiEliminada = _ubicacionCEN.DamePorId(ubi2.Id);
                LogTest("Ubicación DELETE", true, $"Eliminada, búsqueda retorna: {(ubiEliminada == null ? "null" : "entidad")}");

                Console.WriteLine("  ✅ UbicacionCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Ubicación", false, ex.Message);
            }
        }

        private void TestFotos()
        {
            Console.WriteLine("🧪 Probando FotoCEN...\n");

            try
            {
                // Obtener usuario y recargar desde BD
                var usuario = _usuarioCEN.DameTodos().FirstOrDefault();
                if (usuario == null)
                {
                    LogTest("Foto (SKIP)", false, "No hay usuarios disponibles");
                    return;
                }

                // Recargar usuario para asegurar que está en sesión
                _session.Flush();
                usuario = _usuarioCEN.DamePorId(usuario.Id);
                if (usuario == null)
                {
                    LogTest("Foto (ERROR)", false, "Usuario no persisted correctamente");
                    return;
                }

                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando fotos...");
                var foto1 = _fotoCEN.Crear("https://example.com/foto1.jpg", usuario.Id);
                LogTest("Foto CREATE", true, $"ID: {foto1.Id}, URL: {foto1.Url}");

                var foto2 = _fotoCEN.Crear("https://example.com/foto2.jpg", usuario.Id);
                LogTest("Foto CREATE 2", true, $"ID: {foto2.Id}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando fotos...");
                var todas = _fotoCEN.DameTodos().ToList();
                LogTest("Foto READ ALL", true, $"Total fotos: {todas.Count}");

                var porId = _fotoCEN.DamePorId(foto1.Id);
                LogTest("Foto READ BY ID", true, $"URL: {porId?.Url}");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando foto...");
                _fotoCEN.Modificar(foto1.Id, "https://example.com/foto1_actualizada.jpg");
                var fotoModificada = _fotoCEN.DamePorId(foto1.Id);
                LogTest("Foto UPDATE", true, $"Nueva URL: {fotoModificada?.Url}");

                // DELETE
                Console.WriteLine("  ✓ DELETE: Eliminando foto...");
                _fotoCEN.Eliminar(foto2.Id);
                var fotoEliminada = _fotoCEN.DamePorId(foto2.Id);
                LogTest("Foto DELETE", true, $"Eliminada, búsqueda retorna: {(fotoEliminada == null ? "null" : "entidad")}");

                Console.WriteLine("  ✅ FotoCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Foto", false, ex.Message);
            }
        }

        private void TestMatches()
        {
            Console.WriteLine("🧪 Probando MatchCEN...\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count < 2)
                {
                    LogTest("Match (SKIP)", false, "Se necesitan al menos 2 usuarios");
                    return;
                }

                // Recargar usuarios para asegurar que están en sesión
                _session.Flush();
                var usuario1 = _usuarioCEN.DamePorId(usuarios[0].Id);
                var usuario2 = _usuarioCEN.DamePorId(usuarios[1].Id);
                
                if (usuario1 == null || usuario2 == null)
                {
                    LogTest("Match (ERROR)", false, "Usuarios no persisted correctamente");
                    return;
                }

                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando matches...");
                var match1 = _matchCEN.Crear(usuario1, usuario2, likeEmisor: true);
                LogTest("Match CREATE", true, $"ID: {match1.Id}, Emisor: {usuario1.Id}, Receptor: {usuario2.Id}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando matches...");
                var todos = _matchCEN.DameTodos().ToList();
                LogTest("Match READ ALL", true, $"Total matches: {todos.Count}");

                var porId = _matchCEN.DamePorId(match1.Id);
                LogTest("Match READ BY ID", true, $"Like Emisor: {porId?.LikeEmisor}");

                var porUsuario = _matchCEN.DamePorUsuario(usuario1.Id).ToList();
                LogTest("Match READ BY USUARIO", true, $"Matches de usuario: {porUsuario.Count}");

                // UPDATE - Receptor acepta (match mutuo)
                Console.WriteLine("  ✓ UPDATE: Modificando match (receptor acepta)...");
                _matchCEN.Modificar(match1.Id, likeReceptor: true);
                var matchModificado = _matchCEN.DamePorId(match1.Id);
                LogTest("Match UPDATE (Match Mutuo)", true, $"Like Receptor: {matchModificado?.LikeReceptor}, FechaMatch: {matchModificado?.FechaMatch}");

                Console.WriteLine("  ✅ MatchCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Match", false, ex.Message);
            }
        }

        private void TestIniciarMatch()
        {
            Console.WriteLine("🧪 Probando IniciarMatchCP...\n");

            try
            {
                // Crear dos nuevos usuarios para evitar conflicto con matches anteriores
                Console.WriteLine("  ✓ SETUP: Creando nuevos usuarios para test...");
                var usuarioC = _usuarioCEN.Crear("Carlos López", "carlos@test.com", "Pass789!", Plan.Premium);
                var usuarioD = _usuarioCEN.Crear("Diana Martinez", "diana@test.com", "Pass321!", Plan.Gratuito);
                
                _session.Flush();
                
                var usuarioA = _usuarioCEN.DamePorId(usuarioC.Id);
                var usuarioB = _usuarioCEN.DamePorId(usuarioD.Id);
                
                if (usuarioA == null || usuarioB == null)
                {
                    LogTest("IniciarMatch (ERROR)", false, "Usuarios no persisted correctamente");
                    return;
                }

                // Guardar likes previos
                var likesPreviosA = usuarioA.LikesEnviados;

                // Ejecutar CP: Usuario A inicia match con Usuario B
                Console.WriteLine("  ✓ INICIAR: Usuario A da like a Usuario B...");
                var matchIniciado = _iniciarMatchCP.Iniciar(usuarioA.Id, usuarioB.Id);
                LogTest("IniciarMatch CREAR", true, $"Match ID: {matchIniciado.Id}, LikeEmisor: {matchIniciado.LikeEmisor}, LikeReceptor: {matchIniciado.LikeReceptor}");

                // Verificar que LikesEnviados se incrementó
                var usuarioARecargado = _usuarioCEN.DamePorId(usuarioA.Id);
                var likesPosteriores = usuarioARecargado?.LikesEnviados ?? 0;
                LogTest("IniciarMatch CONTADOR", true, $"LikesEnviados antes: {likesPreviosA}, después: {likesPosteriores}");

                // Verificar que se creó notificación
                var notificaciones = _notificacionCEN.DameTodos().ToList();
                LogTest("IniciarMatch NOTIFICACION", true, $"Total notificaciones: {notificaciones.Count}");

                Console.WriteLine("  ✅ IniciarMatchCP: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("IniciarMatch", false, ex.Message);
            }
        }

        private void TestCorresponderMatch()
        {
            Console.WriteLine("🧪 Probando CorresponderMatchCP...\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count < 2)
                {
                    LogTest("CorresponderMatch (SKIP)", false, "Se necesitan al menos 2 usuarios");
                    return;
                }

                // Recargar usuarios para asegurar que están en sesión
                _session.Flush();
                var usuarioA = _usuarioCEN.DamePorId(usuarios[0].Id);
                var usuarioB = _usuarioCEN.DamePorId(usuarios[1].Id);
                
                if (usuarioA == null || usuarioB == null)
                {
                    LogTest("CorresponderMatch (ERROR)", false, "Usuarios no persisted correctamente");
                    return;
                }

                // Primero: crear un match pendiente si no existe
                var matchesPrevios = _matchCEN.DamePorUsuario(usuarioA.Id).ToList();
                Match? matchPendiente = matchesPrevios.FirstOrDefault(m => 
                    m.LikeEmisor && !m.LikeReceptor && 
                    m.Emisor.Id == usuarioA.Id && m.Receptor.Id == usuarioB.Id);

                if (matchPendiente == null)
                {
                    // Crear match pendiente
                    Console.WriteLine("  ✓ Creando match pendiente...");
                    matchPendiente = _matchCEN.Crear(usuarioA, usuarioB, likeEmisor: true);
                    LogTest("CorresponderMatch SETUP", true, $"Match pendiente creado: ID {matchPendiente.Id}");
                }

                // Guardar contadores previos
                var matchsPreviosA = usuarioA.NumMatchs;
                var matchsPreviosB = usuarioB.NumMatchs;

                // Ejecutar CP: Usuario B corresponde el match de Usuario A
                Console.WriteLine("  ✓ CORRESPONDER: Usuario B acepta el like de Usuario A...");
                var matchCorrespondido = _corresponderMatchCP.Corresponder(usuarioB.Id, usuarioA.Id);
                LogTest("CorresponderMatch ACEPTAR", true, $"Match ID: {matchCorrespondido.Id}, LikeReceptor: {matchCorrespondido.LikeReceptor}, FechaMatch: {matchCorrespondido.FechaMatch}");

                // Verificar que NumMatchs se incrementó en ambos
                var usuarioARecargado = _usuarioCEN.DamePorId(usuarioA.Id);
                var usuarioBRecargado = _usuarioCEN.DamePorId(usuarioB.Id);
                var matchsPosterioresA = usuarioARecargado?.NumMatchs ?? 0;
                var matchsPosterioresB = usuarioBRecargado?.NumMatchs ?? 0;
                LogTest("CorresponderMatch CONTADORES", true, 
                    $"Usuario A - antes: {matchsPreviosA}, después: {matchsPosterioresA} | Usuario B - antes: {matchsPreviosB}, después: {matchsPosterioresB}");

                // Verificar que se crearon notificaciones
                var notificaciones = _notificacionCEN.DameTodos().ToList();
                LogTest("CorresponderMatch NOTIFICACIONES", true, $"Total notificaciones: {notificaciones.Count}");

                Console.WriteLine("  ✅ CorresponderMatchCP: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("CorresponderMatch", false, ex.Message);
            }
        }

        private void TestSuperlikes()
        {
            Console.WriteLine("🧪 Probando Superlikes (SuperlikeCP y SuperlikeCEN)...\n");

            try
            {
                // SETUP: Crear dos usuarios Premium para pruebas
                Console.WriteLine("  ✓ SETUP: Creando usuarios Premium para superlikes...");
                var usuarioPremium1 = _usuarioCEN.Crear("Elena Superlikes", "elena@premium.com", "PremPass123!", Plan.Premium);
                var usuarioPremium2 = _usuarioCEN.Crear("Fernando Superlikes", "fernando@premium.com", "PremPass456!", Plan.Premium);
                
                _session.Flush();
                
                var usuario1 = _usuarioCEN.DamePorId(usuarioPremium1.Id);
                var usuario2 = _usuarioCEN.DamePorId(usuarioPremium2.Id);
                
                if (usuario1 == null || usuario2 == null)
                {
                    LogTest("Superlikes (ERROR)", false, "Usuarios no persisted correctamente");
                    return;
                }

                // TEST 1: Validar que usuario Premium puede hacer superlike
                Console.WriteLine("  ✓ TEST 1: Validación de permisos...");
                bool puedeHacer = _superlikeCEN.PuedeHacerSuperlike(usuario1.Id);
                LogTest("Superlikes VALIDACION PERMISO", true, $"¿Puede hacer superlike? {puedeHacer}");

                // TEST 2: Obtener superlikes disponibles iniciales
                Console.WriteLine("  ✓ TEST 2: Consultando superlikes disponibles...");
                int superluesIniciales = _superlikeCEN.ObtenerSuperlikes(usuario1.Id);
                LogTest("Superlikes OBTENER DISPONIBLES", true, $"Superlikes iniciales: {superluesIniciales}");

                // TEST 3: Crear un superlike
                Console.WriteLine("  ✓ TEST 3: Creando superlike...");
                var matchSuperlike = _superlikeCP.Superlike(usuario1.Id, usuario2.Id);
                LogTest("Superlikes CREAR", true, 
                    $"Match ID: {matchSuperlike.Id}, EsSuperlike: {matchSuperlike.EsSuperlike}, FechaInicio: {matchSuperlike.FechaInicio}");

                // TEST 4: Verificar que SuperlikesDisponibles disminuyó
                Console.WriteLine("  ✓ TEST 4: Verificar consumo de superlike...");
                var usuario1Recargado = _usuarioCEN.DamePorId(usuario1.Id);
                int superlikesPost = usuario1Recargado?.SuperlikesDisponibles ?? 0;
                LogTest("Superlikes CONSUMO", true, 
                    $"Superlikes antes: {superluesIniciales}, después: {superlikesPost}");

                // TEST 5: Verificar que LikesRecibidos del receptor aumentó en 2
                Console.WriteLine("  ✓ TEST 5: Verificar doble contador de likes...");
                var usuario2Recargado = _usuarioCEN.DamePorId(usuario2.Id);
                int likesReceptor = usuario2Recargado?.LikesRecibidos ?? 0;
                LogTest("Superlikes DOBLE CONTADOR", true, 
                    $"LikesRecibidos del receptor: {likesReceptor} (debería ser +2)");

                // TEST 6: Comprar superlikes
                Console.WriteLine("  ✓ TEST 6: Comprando superlikes...");
                var superlikesAntes = _superlikeCEN.ObtenerSuperlikes(usuario1.Id);
                _superlikeCP.ComprarSuperlikes(usuario1.Id, 5);
                var superlileDesDespues = _superlikeCEN.ObtenerSuperlikes(usuario1.Id);
                LogTest("Superlikes COMPRA", true, 
                    $"Superlikes antes: {superlikesAntes}, después de comprar 5: {superlileDesDespues}");

                // TEST 7: Obtener información de superlikes
                Console.WriteLine("  ✓ TEST 7: Obteniendo información de superlikes...");
                var info = _superlikeCP.ObtenerInfoSuperlikes(usuario1.Id);
                LogTest("Superlikes INFO", true, 
                    $"Plan: {info.TipoPlan}, Disponibles: {info.SuperlikesDisponibles}, Puede hacer: {info.PuedeHacerSuperlike}");

                // TEST 8: Contar superlikes recibidos
                Console.WriteLine("  ✓ TEST 8: Contando superlikes recibidos...");
                int superlinesRecibidos = _superlikeCP.ContarSuperlikes(usuario2.Id);
                LogTest("Superlikes RECIBIDOS", true, 
                    $"Usuario 2 ha recibido: {superlinesRecibidos} superlike(s)");

                // TEST 9: Obtener estadísticas
                Console.WriteLine("  ✓ TEST 9: Obteniendo estadísticas...");
                var estadisticas = _superlikeCEN.ObtenerEstadisticas(usuario1.Id);
                LogTest("Superlikes ESTADISTICAS", true, 
                    $"Disponibles: {estadisticas.SuperlikesDisponibles}, Usados: {estadisticas.SuperlikesUsados}, Recibidos: {estadisticas.SuperlikesRecibidos}, Puntaje equivalente: {estadisticas.PuntajeEquivalentePorSuperlikes}");

                // TEST 10: Verificar que usuario no-Premium NO puede hacer superlike
                Console.WriteLine("  ✓ TEST 10: Validación de usuario no-Premium...");
                var usuarioNoPreium = _usuarioCEN.Crear("Gema NoSuperlike", "gema@nopreium.com", "Pass789!", Plan.Gratuito);
                bool puedeHacerNoPreium = _superlikeCEN.PuedeHacerSuperlike(usuarioNoPreium.Id);
                LogTest("Superlikes NO-PREMIUM", true, 
                    $"¿Usuario no-Premium puede hacer superlike? {puedeHacerNoPreium} (debería ser False)");

                Console.WriteLine("  ✅ Superlikes: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Superlikes", false, ex.Message);
            }
        }

        private void TestAuth()
        {
            Console.WriteLine("🧪 Probando AuthCEN y AuthCP...\n");

            try
            {
                // Crear usuario para tests
                Console.WriteLine("  ✓ CREATE: Creando usuario para autenticación...");
                var usuarioAuth = _usuarioCEN.Crear("Auth Test User", "authtest@test.com", "PlainPassword123!", Plan.Premium);
                LogTest("Auth Usuario CREATE", true, $"ID: {usuarioAuth.Id}, Email: {usuarioAuth.Email}");

                // Ahora necesitamos usar AuthCP y AuthCEN para probar
                // Primero, rehashear la contraseña con BCrypt
                var passwordHasher = new Infrastructure.Security.BcryptPasswordHasher();
                var passwordHasheada = passwordHasher.HashPassword("PlainPassword123!");
                usuarioAuth.Pass = passwordHasheada;
                _uow.SaveChanges();
                LogTest("Auth Password Hashing", true, $"Password hasheada correctamente (BCrypt)");

                // Probar AuthCEN
                var authCEN = new ApplicationCore.Domain.CEN.AuthCEN(
                    new UsuarioRepository(_session),
                    passwordHasher,
                    _uow
                );

                // Validar credenciales incorrectas
                var resultIncorrecto = authCEN.ValidarCredenciales("authtest@test.com", "WrongPassword");
                LogTest("Auth ValidarCredenciales INCORRECTO", resultIncorrecto == null ? true : false, 
                    "Rechazó password incorrecto correctamente");

                // Validar credenciales correctas
                var resultCorrecto = authCEN.ValidarCredenciales("authtest@test.com", "PlainPassword123!");
                LogTest("Auth ValidarCredenciales CORRECTO", resultCorrecto != null, 
                    $"Aceptó password correcto: {resultCorrecto?.Email}");

                // Probar AuthCP
                var jwtSettings = new ApplicationCore.Domain.DTOs.JwtSettings
                {
                    Secret = "una-clave-super-secreta-de-almenos-32-caracteres-para-jwt-hs256",
                    Issuer = "SpeedMatch",
                    Audience = "SpeedMatchClients",
                    ExpirationMinutes = 60
                };

                var jwtGenerator = new Infrastructure.Security.JwtTokenGenerator(jwtSettings);
                
                var authCP = new ApplicationCore.Domain.CP.AuthCP(
                    authCEN,
                    _usuarioCEN,
                    passwordHasher,
                    new UsuarioRepository(_session),
                    _uow
                );

                // Probar Login via AuthCP
                Console.WriteLine("  ✓ LOGIN: Autenticando usuario...");
                var usuarioLogado = authCP.Login("authtest@test.com", "PlainPassword123!");
                LogTest("Auth Login", true, $"Usuario logado: {usuarioLogado.Email}");

                // Generar token JWT
                var token = jwtGenerator.GenerarToken(usuarioLogado);
                LogTest("Auth JWT Token Generation", !string.IsNullOrEmpty(token), 
                    $"Token generado (primeros 50 chars): {token?.Substring(0, 50)}...");

                // Validar token
                var tokenValido = token != null && jwtGenerator.ValidarToken(token);
                LogTest("Auth JWT Token Validation", tokenValido, "Token validado correctamente");

                // Registrar nuevo usuario (AuthCP.Register)
                Console.WriteLine("  ✓ REGISTER: Creando nuevo usuario...");
                var usuarioNuevo = authCP.Register("Nuevo Usuario", "nuevouser@test.com", "NewPassword456!", Plan.Gratuito);
                LogTest("Auth Register", true, $"Usuario registrado: {usuarioNuevo.Email}, ID: {usuarioNuevo.Id}");

                var tokenNuevoUsuario = jwtGenerator.GenerarToken(usuarioNuevo);
                LogTest("Auth New User JWT Token", !string.IsNullOrEmpty(tokenNuevoUsuario), 
                    "Token generado para nuevo usuario");

                // Probar que usuario baneado no puede loguearse
                Console.WriteLine("  ✓ BANNED USER: Intentando login con usuario baneado...");
                usuarioNuevo.Baneado = true;
                _uow.SaveChanges();
                
                try
                {
                    authCP.Login("nuevouser@test.com", "NewPassword456!");
                    LogTest("Auth Banned User", false, "No debería permitir login a usuario baneado");
                }
                catch (UnauthorizedAccessException)
                {
                    LogTest("Auth Banned User", true, "Rechazó correctamente login de usuario baneado");
                }
            }
            catch (Exception ex)
            {
                LogTest("Auth General", false, ex.Message);
            }
        }

        private void TestNotificaciones()
        {
            Console.WriteLine("🧪 Probando NotificacionCEN...\n");

            try
            {
                // Obtener usuario y recargar desde BD
                var usuario = _usuarioCEN.DameTodos().FirstOrDefault();
                if (usuario == null)
                {
                    LogTest("Notificación (SKIP)", false, "No hay usuarios disponibles");
                    return;
                }

                // Recargar usuario para asegurar que está en sesión
                _session.Flush();
                usuario = _usuarioCEN.DamePorId(usuario.Id);
                if (usuario == null)
                {
                    LogTest("Notificación (ERROR)", false, "Usuario no persisted correctamente");
                    return;
                }

                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando notificaciones...");
                var notif1 = _notificacionCEN.Crear(usuario, "¡Te hizo un like!");
                LogTest("Notificación CREATE", true, $"ID: {notif1.Id}, Mensaje: {notif1.Mensaje}");

                var notif2 = _notificacionCEN.Crear(usuario, "¡Es un match!");
                LogTest("Notificación CREATE 2", true, $"ID: {notif2.Id}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando notificaciones...");
                var todas = _notificacionCEN.DameTodos().ToList();
                LogTest("Notificación READ ALL", true, $"Total notificaciones: {todas.Count}");

                var porId = _notificacionCEN.DamePorId(notif1.Id);
                LogTest("Notificación READ BY ID", true, $"Mensaje: {porId?.Mensaje}");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando notificación...");
                _notificacionCEN.Modificar(notif1.Id, "¡Tienes un nuevo like!");
                var notifModificada = _notificacionCEN.DamePorId(notif1.Id);
                LogTest("Notificación UPDATE", true, $"Nuevo mensaje: {notifModificada?.Mensaje}");

                // DELETE
                Console.WriteLine("  ✓ DELETE: Eliminando notificación...");
                _notificacionCEN.Eliminar(notif2.Id);
                var notifEliminada = _notificacionCEN.DamePorId(notif2.Id);
                LogTest("Notificación DELETE", true, $"Eliminada, búsqueda retorna: {(notifEliminada == null ? "null" : "entidad")}");

                Console.WriteLine("  ✅ NotificacionCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Notificación", false, ex.Message);
            }
        }

        private void TestAdmins()
        {
            Console.WriteLine("🧪 Probando AdminCEN...\n");

            try
            {
                // Flush para asegurar persistencia previa
                _session.Flush();

                // CREATE
                Console.WriteLine("  ✓ CREATE: Creando admins...");
                var admin1 = _adminCEN.Crear("admin@test.com", "AdminPass123!");
                LogTest("Admin CREATE", true, $"ID: {admin1.Id}, Email: {admin1.Email}");

                var admin2 = _adminCEN.Crear("admin2@test.com", "AdminPass456!");
                LogTest("Admin CREATE 2", true, $"ID: {admin2.Id}");

                // READ
                Console.WriteLine("  ✓ READ: Consultando admins...");
                var todos = _adminCEN.DameTodos().ToList();
                LogTest("Admin READ ALL", true, $"Total admins: {todos.Count}");

                var porId = _adminCEN.DamePorId(admin1.Id);
                LogTest("Admin READ BY ID", true, $"Email: {porId?.Email}");

                // UPDATE
                Console.WriteLine("  ✓ UPDATE: Modificando admin...");
                _adminCEN.Modificar(admin1.Id, "admin.actualizado@test.com", "NewAdminPass789!");
                var adminModificado = _adminCEN.DamePorId(admin1.Id);
                LogTest("Admin UPDATE", true, $"Nuevo email: {adminModificado?.Email}");

                // DELETE
                Console.WriteLine("  ✓ DELETE: Eliminando admin...");
                _adminCEN.Eliminar(admin2.Id);
                var adminEliminado = _adminCEN.DamePorId(admin2.Id);
                LogTest("Admin DELETE", true, $"Eliminado, búsqueda retorna: {(adminEliminado == null ? "null" : "entidad")}");

                Console.WriteLine("  ✅ AdminCEN: OK\n");
            }
            catch (Exception ex)
            {
                LogTest("Admin", false, ex.Message);
            }
        }

        private void LogTest(string testName, bool success, string details)
        {
            var result = new TestResult
            {
                TestName = testName,
                Success = success,
                Details = details,
                Timestamp = DateTime.Now
            };

            _testResults.Add(result);

            string statusIcon = success ? "✅" : "❌";
            Console.WriteLine($"    {statusIcon} {testName}: {details}");
        }

        private void GenerateJsonReport()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         GENERANDO REPORTE JSON CON RESULTADOS              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            var report = new TestReport
            {
                TestDate = DateTime.Now,
                TotalTests = _testResults.Count,
                SuccessfulTests = _testResults.Count(r => r.Success),
                FailedTests = _testResults.Count(r => !r.Success),
                SuccessRate = _testResults.Count > 0 ? (_testResults.Count(r => r.Success) * 100.0 / _testResults.Count) : 0,
                Tests = _testResults
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(report, options);

            // Guardar en archivo
            string filePath = "CRUD_TEST_RESULTS.json";
            System.IO.File.WriteAllText(filePath, json);

            Console.WriteLine($"✅ Reporte guardado en: {filePath}\n");
            Console.WriteLine("Contenido del reporte:\n");
            Console.WriteLine(json);

            // Mostrar resumen
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    RESUMEN DE PRUEBAS                      ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Total de pruebas:        {report.TotalTests,50} ║");
            Console.WriteLine($"║ Pruebas exitosas:        {report.SuccessfulTests,50} ║");
            Console.WriteLine($"║ Pruebas fallidas:        {report.FailedTests,50} ║");
            Console.WriteLine($"║ Porcentaje éxito:        {report.SuccessRate:F2}%{new string(' ', 44)} ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }
    }

    /// <summary>
    /// Clase para almacenar resultados de pruebas
    /// </summary>
    public class TestResult
    {
        [JsonPropertyName("test_name")]
        public string? TestName { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Clase para el reporte general
    /// </summary>
    public class TestReport
    {
        [JsonPropertyName("test_date")]
        public DateTime TestDate { get; set; }

        [JsonPropertyName("total_tests")]
        public int TotalTests { get; set; }

        [JsonPropertyName("successful_tests")]
        public int SuccessfulTests { get; set; }

        [JsonPropertyName("failed_tests")]
        public int FailedTests { get; set; }

        [JsonPropertyName("success_rate_percentage")]
        public double SuccessRate { get; set; }

        [JsonPropertyName("tests")]
        public List<TestResult>? Tests { get; set; }
    }
}
