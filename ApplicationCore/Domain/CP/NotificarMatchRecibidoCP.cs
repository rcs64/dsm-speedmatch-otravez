using System;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// Caso de Uso: Notificar Match Recibido
    /// 
    /// Flujo:
    /// 1. Usuario A ha dado like a Usuario B (Match iniciado)
    /// 2. Usuario B debe ser notificado de que recibió un like
    /// 3. Si Usuario B también da like, se crea Match Mutuo
    /// 
    /// Responsabilidades:
    /// - Validar que exista el match pendiente
    /// - Validar que el usuario receptor sea válido
    /// - Crear notificación del match recibido
    /// - Si ya existe match mutuo, actualizar notificación
    /// - Garantizar atomicidad
    /// 
    /// Diferencia con IniciarMatchCP:
    /// - IniciarMatchCP: Usuario A da like a Usuario B (iniciador)
    /// - NotificarMatchRecibidoCP: Usuario B es notificado (receptor)
    /// </summary>
    public class NotificarMatchRecibidoCP
    {
        private readonly MatchCEN _matchCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NotificacionCEN _notificacionCEN;
        private readonly IUnitOfWork _uow;

        public NotificarMatchRecibidoCP(
            MatchCEN matchCEN,
            UsuarioCEN usuarioCEN,
            NotificacionCEN notificacionCEN,
            IUnitOfWork uow)
        {
            _matchCEN = matchCEN ?? throw new ArgumentNullException(nameof(matchCEN));
            _usuarioCEN = usuarioCEN ?? throw new ArgumentNullException(nameof(usuarioCEN));
            _notificacionCEN = notificacionCEN ?? throw new ArgumentNullException(nameof(notificacionCEN));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Notifica al usuario receptor de un match recibido
        /// 
        /// Precondiciones:
        /// - Match existe entre emisor y receptor
        /// - Match.LikeEmisor = true (fue iniciado)
        /// - Receptor existe y es válido
        /// 
        /// Acciones:
        /// 1. Obtener el match
        /// 2. Validar que sea un like recibido (no enviado)
        /// 3. Crear notificación personalizada
        /// 4. Si es match mutuo, crear notificación especial
        /// 5. Guardar transaccionalmente
        /// 
        /// Postcondiciones:
        /// - Usuario receptor notificado
        /// - Si match mutuo, notificación especial creada
        /// - TODO guardado en BD
        /// </summary>
        public void NotificarMatchRecibido(long matchId, long receptorId)
        {
            try
            {
                // VALIDACION 1: IDs válidos
                if (matchId <= 0 || receptorId <= 0)
                    throw new InvalidOperationException("Los IDs son inválidos");

                // VALIDACION 2: Obtener el match
                var match = _matchCEN.DamePorId(matchId);
                if (match == null)
                    throw new InvalidOperationException($"Match {matchId} no encontrado");

                // VALIDACION 3: Validar que el usuario sea el receptor
                if (match.Receptor.Id != receptorId)
                    throw new InvalidOperationException(
                        $"El usuario {receptorId} no es el receptor de este match");

                // VALIDACION 4: Validar que sea un like recibido (no enviado por este usuario)
                if (match.LikeEmisor == false)
                    throw new InvalidOperationException(
                        "El emisor aún no ha dado like a este usuario");

                // VALIDACION 5: Obtener usuario receptor
                var receptor = _usuarioCEN.DamePorId(receptorId);
                if (receptor == null)
                    throw new InvalidOperationException($"Usuario receptor {receptorId} no encontrado");

                if (receptor.Baneado)
                    throw new InvalidOperationException($"El usuario {receptorId} está baneado");

                // ========== TRANSACCION COMIENZA ==========

                // Obtener emisor para personalizar notificación
                var emisor = match.Emisor;

                // PASO 1: Crear notificación base
                string mensajeNotificacion = $"¡{emisor.Nombre} te dio un like! 💘";

                // PASO 2: Verificar si es Match Mutuo
                if (match.LikeReceptor == true && match.FechaMatch.HasValue)
                {
                    // Es match mutuo: ambos usuarios se han gustado
                    mensajeNotificacion = $"¡Match! 🎉 ¡Tú y {emisor.Nombre} se gustaron mutuamente!";
                }
                else if (match.LikeReceptor == true)
                {
                    // El receptor ya había dado like antes
                    mensajeNotificacion = $"¡Sí! ¡{emisor.Nombre} también te gustó! 💕 ¡Es un Match!";
                }

                // PASO 3: Crear notificación
                _notificacionCEN.Crear(receptor, mensajeNotificacion);

                // PASO 4: Incrementar estadísticas del receptor
                _usuarioCEN.RecibirLike(receptorId);

                // PASO 5: Guardar todo en una sola transacción
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al notificar match recibido: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Notifica a ambos usuarios cuando se crea un Match Mutuo
        /// 
        /// Flujo:
        /// 1. Usuario A da like a Usuario B (IniciarMatchCP)
        /// 2. Usuario B también da like a Usuario A
        /// 3. Se ejecuta este método para notificar a AMBOS del match mutuo
        /// 
        /// Precondiciones:
        /// - Match existe
        /// - LikeEmisor = true
        /// - LikeReceptor = true
        /// - FechaMatch está establecida
        /// 
        /// Postcondiciones:
        /// - Ambos usuarios notificados del match mutuo
        /// - Estadísticas actualizadas
        /// </summary>
        public void NotificarMatchMutuo(long matchId)
        {
            try
            {
                // VALIDACION 1: ID válido
                if (matchId <= 0)
                    throw new InvalidOperationException("El ID del match es inválido");

                // VALIDACION 2: Obtener el match
                var match = _matchCEN.DamePorId(matchId);
                if (match == null)
                    throw new InvalidOperationException($"Match {matchId} no encontrado");

                // VALIDACION 3: Validar que sea Match Mutuo
                if (!match.LikeEmisor || !match.LikeReceptor || !match.FechaMatch.HasValue)
                    throw new InvalidOperationException(
                        "Este no es un match mutuo válido");

                // VALIDACION 4: Validar usuarios
                var emisor = match.Emisor;
                var receptor = match.Receptor;

                if (emisor.Baneado || receptor.Baneado)
                    throw new InvalidOperationException(
                        "Uno de los usuarios está baneado");

                // ========== TRANSACCION COMIENZA ==========

                // Mensaje del match mutuo
                string mensajeMatchMutuo = $"🎉 ¡MATCH! ¡Tú y {0} se gustaron mutuamente!";

                // PASO 1: Notificar al EMISOR
                _notificacionCEN.Crear(
                    emisor,
                    string.Format(mensajeMatchMutuo, receptor.Nombre)
                );

                // PASO 2: Notificar al RECEPTOR
                _notificacionCEN.Crear(
                    receptor,
                    string.Format(mensajeMatchMutuo, emisor.Nombre)
                );

                // PASO 3: Incrementar estadísticas en ambos
                _usuarioCEN.RecibirMatch(emisor.Id);
                _usuarioCEN.RecibirMatch(receptor.Id);

                // PASO 4: Guardar todo en una sola transacción
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al notificar match mutuo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si hay matches pendientes para un usuario y notifica
        /// 
        /// Caso de uso: Usuario abre la app y queremos mostrarle
        /// qué likes recibió mientras estaba offline
        /// 
        /// Precondiciones:
        /// - Usuario existe
        /// - Puede haber matches pendientes sin notificación
        /// 
        /// Postcondiciones:
        /// - Todas las notificaciones creadas
        /// - Usuario "puesto al día"
        /// </summary>
        public int NotificarMatchesPendientes(long usuarioId)
        {
            try
            {
                // VALIDACION 1: ID válido
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                // VALIDACION 2: Usuario existe
                var usuario = _usuarioCEN.DamePorId(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                // VALIDACION 3: Usuario no está baneado
                if (usuario.Baneado)
                    throw new InvalidOperationException($"El usuario {usuarioId} está baneado");

                int notificacionesCreadas = 0;

                // ========== TRANSACCION COMIENZA ==========

                // PASO 1: Obtener todos los matches donde este usuario es receptor
                var matchesPendientes = _matchCEN.DamePorUsuario(usuarioId)
                    .Where(m => m.Receptor.Id == usuarioId && m.LikeEmisor && !m.LikeReceptor)
                    .ToList();

                // PASO 2: Para cada match, crear notificación
                foreach (var match in matchesPendientes)
                {
                    var emisor = match.Emisor;
                    string mensaje = $"¡{emisor.Nombre} te dio un like! 💘";

                    _notificacionCEN.Crear(usuario, mensaje);
                    notificacionesCreadas++;
                }

                // PASO 3: Si hay matches mutuos, notificar
                var matchesMutuos = _matchCEN.DamePorUsuario(usuarioId)
                    .Where(m => m.LikeEmisor && m.LikeReceptor && m.FechaMatch.HasValue)
                    .ToList();

                foreach (var match in matchesMutuos)
                {
                    var otro = match.Emisor.Id == usuarioId ? match.Receptor : match.Emisor;
                    string mensaje = $"🎉 ¡MATCH! ¡Tú y {otro.Nombre} se gustaron mutuamente!";

                    _notificacionCEN.Crear(usuario, mensaje);
                    notificacionesCreadas++;
                }

                // PASO 4: Guardar todo
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========

                return notificacionesCreadas;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al notificar matches pendientes: {ex.Message}", ex);
            }
        }
    }
}
