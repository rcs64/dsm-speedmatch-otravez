using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using NHibernate;
using NHibernate.Cfg;

namespace InitializeDb.Examples
{
    /// <summary>
    /// Ejemplos de uso de los CENs con el CRUD completamente implementado.
    /// Estos ejemplos muestran cómo crear, leer, modificar y eliminar entidades.
    /// </summary>
    public class CRUDExamples
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

        public CRUDExamples(ISession session)
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
        }

        /// <summary>
        /// Ejemplo 1: CRUD de Usuarios
        /// </summary>
        public void EjemploUsuarios()
        {
            Console.WriteLine("\n=== EJEMPLO 1: CRUD DE USUARIOS ===\n");

            try
            {
                // CREATE - Crear usuarios
                Console.WriteLine("1. CREAR usuarios...");
                var usuario1 = _usuarioCEN.Crear(
                    nombre: "Juan Pérez",
                    email: "juan@speedmatch.com",
                    pass: "Password123!",
                    tipoPlan: Plan.Premium
                );
                Console.WriteLine($"✓ Usuario creado: {usuario1.Nombre} (ID: {usuario1.Id})");

                var usuario2 = _usuarioCEN.Crear(
                    nombre: "María García",
                    email: "maria@speedmatch.com",
                    pass: "Password456!",
                    tipoPlan: Plan.Gratuito
                );
                Console.WriteLine($"✓ Usuario creado: {usuario2.Nombre} (ID: {usuario2.Id})");

                // READ - Consultar usuarios
                Console.WriteLine("\n2. CONSULTAR usuarios...");
                var todos = _usuarioCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de usuarios: {todos.Count}");

                var porId = _usuarioCEN.DamePorId(usuario1.Id);
                Console.WriteLine($"✓ Usuario por ID: {porId?.Nombre}");

                var porEmail = _usuarioCEN.DamePorEmail("juan@speedmatch.com");
                Console.WriteLine($"✓ Usuario por email: {porEmail?.Email}");

                // UPDATE - Modificar usuario
                Console.WriteLine("\n3. MODIFICAR usuario...");
                _usuarioCEN.Modificar(usuario1.Id, "Juan Carlos Pérez", "juan.carlos@speedmatch.com");
                var usuarioModificado = _usuarioCEN.DamePorId(usuario1.Id);
                Console.WriteLine($"✓ Usuario modificado: {usuarioModificado?.Nombre} ({usuarioModificado?.Email})");

                // Operaciones de negocio
                Console.WriteLine("\n4. OPERACIONES DE NEGOCIO...");
                _usuarioCEN.DarLike(usuario1.Id);
                var usuarioConLike = _usuarioCEN.DamePorId(usuario1.Id);
                Console.WriteLine($"✓ Likes recibidos: {usuarioConLike?.LikesRecibidos}");

                _usuarioCEN.Banear(usuario2.Id);
                var usuarioBaneado = _usuarioCEN.DamePorId(usuario2.Id);
                Console.WriteLine($"✓ Usuario baneado: {usuarioBaneado?.Baneado}");

                _usuarioCEN.Desbanear(usuario2.Id);
                var usuarioDesbaneado = _usuarioCEN.DamePorId(usuario2.Id);
                Console.WriteLine($"✓ Usuario desbaneado: {usuarioDesbaneado?.Baneado}");

                // DELETE - Comentado para no eliminar datos
                // _usuarioCEN.Eliminar(usuario2.Id);
                // Console.WriteLine("✓ Usuario eliminado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 2: CRUD de Fotos
        /// </summary>
        public void EjemploFotos()
        {
            Console.WriteLine("\n=== EJEMPLO 2: CRUD DE FOTOS ===\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count == 0)
                {
                    Console.WriteLine("No hay usuarios. Crea usuarios primero.");
                    return;
                }

                var usuario = usuarios[0];

                // CREATE
                Console.WriteLine("1. CREAR fotos...");
                var foto1 = _fotoCEN.Crear("https://example.com/foto1.jpg", usuario.Id);
                Console.WriteLine($"✓ Foto creada (ID: {foto1.Id})");

                var foto2 = _fotoCEN.Crear("https://example.com/foto2.jpg", usuario.Id);
                Console.WriteLine($"✓ Foto creada (ID: {foto2.Id})");

                // READ
                Console.WriteLine("\n2. CONSULTAR fotos...");
                var todas = _fotoCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de fotos: {todas.Count}");

                // UPDATE
                Console.WriteLine("\n3. MODIFICAR foto...");
                _fotoCEN.Modificar(foto1.Id, "https://example.com/foto1_actualizada.jpg");
                var fotoModificada = _fotoCEN.DamePorId(foto1.Id);
                Console.WriteLine($"✓ Foto modificada: {fotoModificada?.Url}");

                // DELETE - Comentado
                // _fotoCEN.Eliminar(foto2.Id);
                // Console.WriteLine("✓ Foto eliminada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 3: CRUD de Matches
        /// </summary>
        public void EjemploMatches()
        {
            Console.WriteLine("\n=== EJEMPLO 3: CRUD DE MATCHES ===\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count < 2)
                {
                    Console.WriteLine("Se necesitan al menos 2 usuarios para crear matches.");
                    return;
                }

                var usuario1 = usuarios[0];
                var usuario2 = usuarios[1];

                // CREATE
                Console.WriteLine("1. CREAR match...");
                var match = _matchCEN.Crear(usuario1, usuario2, likeEmisor: true);
                Console.WriteLine($"✓ Match creado (ID: {match.Id})");
                Console.WriteLine($"  - Emisor: {usuario1.Nombre}");
                Console.WriteLine($"  - Receptor: {usuario2.Nombre}");
                Console.WriteLine($"  - Like Emisor: {match.LikeEmisor}");

                // READ
                Console.WriteLine("\n2. CONSULTAR matches...");
                var todosMatches = _matchCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de matches: {todosMatches.Count}");

                var matchesPorUsuario = _matchCEN.DamePorUsuario(usuario1.Id).ToList();
                Console.WriteLine($"✓ Matches del usuario {usuario1.Nombre}: {matchesPorUsuario.Count}");

                // UPDATE - Completar match mutuo
                Console.WriteLine("\n3. MODIFICAR match (receptor acepta)...");
                _matchCEN.Modificar(match.Id, likeReceptor: true);
                var matchModificado = _matchCEN.DamePorId(match.Id);
                Console.WriteLine($"✓ Match actualizado:");
                Console.WriteLine($"  - Like Receptor: {matchModificado?.LikeReceptor}");
                Console.WriteLine($"  - Fecha Match (mutuo): {matchModificado?.FechaMatch}");

                // DELETE - Comentado
                // _matchCEN.Eliminar(match.Id);
                // Console.WriteLine("✓ Match eliminado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 4: CRUD de Notificaciones
        /// </summary>
        public void EjemploNotificaciones()
        {
            Console.WriteLine("\n=== EJEMPLO 4: CRUD DE NOTIFICACIONES ===\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count == 0)
                {
                    Console.WriteLine("No hay usuarios. Crea usuarios primero.");
                    return;
                }

                var usuario = usuarios[0];

                // CREATE
                Console.WriteLine("1. CREAR notificaciones...");
                var notif1 = _notificacionCEN.Crear(usuario, "¡Te hizo un like!");
                Console.WriteLine($"✓ Notificación creada (ID: {notif1.Id})");

                var notif2 = _notificacionCEN.Crear(usuario, "¡Es un match!");
                Console.WriteLine($"✓ Notificación creada (ID: {notif2.Id})");

                // READ
                Console.WriteLine("\n2. CONSULTAR notificaciones...");
                var todas = _notificacionCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de notificaciones: {todas.Count}");

                // UPDATE
                Console.WriteLine("\n3. MODIFICAR notificación...");
                _notificacionCEN.Modificar(notif1.Id, "¡Tienes un nuevo like!");
                var notifModificada = _notificacionCEN.DamePorId(notif1.Id);
                Console.WriteLine($"✓ Notificación modificada: {notifModificada?.Mensaje}");

                // DELETE - Comentado
                // _notificacionCEN.Eliminar(notif2.Id);
                // Console.WriteLine("✓ Notificación eliminada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 5: CRUD de Preferencias
        /// </summary>
        public void EjemploPreferencias()
        {
            Console.WriteLine("\n=== EJEMPLO 5: CRUD DE PREFERENCIAS ===\n");

            try
            {
                // CREATE
                Console.WriteLine("1. CREAR preferencias...");
                var pref1 = _preferenciasCEN.Crear(
                    orientacion: OrientacionSexual.Heterosexual,
                    conocer: PrefConocer.Mujer,
                    orientacionMostrar: true
                );
                Console.WriteLine($"✓ Preferencias creadas (ID: {pref1.Id})");
                Console.WriteLine($"  - Orientación: {pref1.Orientacion}");
                Console.WriteLine($"  - Prefiere conocer: {pref1.Conocer}");

                // READ
                Console.WriteLine("\n2. CONSULTAR preferencias...");
                var todas = _preferenciasCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de preferencias: {todas.Count}");

                // UPDATE
                Console.WriteLine("\n3. MODIFICAR preferencias...");
                _preferenciasCEN.Modificar(
                    pref1.Id,
                    orientacion: OrientacionSexual.Bisexual,
                    conocer: PrefConocer.Todos,
                    orientacionMostrar: false
                );
                var prefModificada = _preferenciasCEN.DamePorId(pref1.Id);
                Console.WriteLine($"✓ Preferencias modificadas:");
                Console.WriteLine($"  - Orientación: {prefModificada?.Orientacion}");
                Console.WriteLine($"  - Prefiere conocer: {prefModificada?.Conocer}");

                // DELETE - Comentado
                // _preferenciasCEN.Eliminar(pref1.Id);
                // Console.WriteLine("✓ Preferencias eliminadas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 6: CRUD de Ubicaciones
        /// </summary>
        public void EjemploUbicaciones()
        {
            Console.WriteLine("\n=== EJEMPLO 6: CRUD DE UBICACIONES ===\n");

            try
            {
                var usuarios = _usuarioCEN.DameTodos().ToList();
                if (usuarios.Count == 0)
                {
                    Console.WriteLine("No hay usuarios. Crea usuarios primero.");
                    return;
                }

                var usuario = usuarios[0];

                // CREATE
                Console.WriteLine("1. CREAR ubicaciones...");
                var ubi1 = _ubicacionCEN.Crear(40.4168, -3.7038, usuario.Id); // Madrid
                Console.WriteLine($"✓ Ubicación creada (ID: {ubi1.Id}) - Madrid (40.42, -3.70)");

                var ubi2 = _ubicacionCEN.Crear(41.3851, 2.1734, usuario.Id); // Barcelona
                Console.WriteLine($"✓ Ubicación creada (ID: {ubi2.Id}) - Barcelona (41.39, 2.17)");

                // READ
                Console.WriteLine("\n2. CONSULTAR ubicaciones...");
                var todas = _ubicacionCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de ubicaciones: {todas.Count}");

                // UPDATE
                Console.WriteLine("\n3. MODIFICAR ubicación...");
                _ubicacionCEN.Modificar(ubi1.Id, 40.5200, -3.7500);
                var ubiModificada = _ubicacionCEN.DamePorId(ubi1.Id);
                Console.WriteLine($"✓ Ubicación modificada: ({ubiModificada?.Lat}, {ubiModificada?.Lon})");

                // DELETE - Comentado
                // _ubicacionCEN.Eliminar(ubi2.Id);
                // Console.WriteLine("✓ Ubicación eliminada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejemplo 7: CRUD de Administradores
        /// </summary>
        public void EjemploAdmins()
        {
            Console.WriteLine("\n=== EJEMPLO 7: CRUD DE ADMINISTRADORES ===\n");

            try
            {
                // CREATE
                Console.WriteLine("1. CREAR administradores...");
                var admin1 = _adminCEN.Crear("admin@speedmatch.com", "AdminPass123!");
                Console.WriteLine($"✓ Admin creado (ID: {admin1.Id}) - {admin1.Email}");

                // READ
                Console.WriteLine("\n2. CONSULTAR administradores...");
                var todos = _adminCEN.DameTodos().ToList();
                Console.WriteLine($"✓ Total de administradores: {todos.Count}");

                // UPDATE
                Console.WriteLine("\n3. MODIFICAR administrador...");
                _adminCEN.Modificar(admin1.Id, "admin.actualizado@speedmatch.com", "NewAdminPass456!");
                var adminModificado = _adminCEN.DamePorId(admin1.Id);
                Console.WriteLine($"✓ Admin modificado: {adminModificado?.Email}");

                // DELETE - Comentado
                // _adminCEN.Eliminar(admin1.Id);
                // Console.WriteLine("✓ Admin eliminado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Ejecutar todos los ejemplos
        /// </summary>
        public void EjecutarTodos()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         EJEMPLOS DE USO - CRUD COMPLETAMENTE IMPLEMENTADO  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            EjemploUsuarios();
            EjemploFotos();
            EjemploMatches();
            EjemploNotificaciones();
            EjemploPreferencias();
            EjemploUbicaciones();
            EjemploAdmins();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    EJEMPLOS COMPLETADOS ✓                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }
    }
}
