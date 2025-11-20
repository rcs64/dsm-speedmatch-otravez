using System;
using System.Collections.Generic;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// Caso de Uso: Corresponder Match
    /// 
    /// Flujo:
    /// 1. Usuario A inicia un match dando like a Usuario B (ya existe Match)
    /// 2. Usuario B ve el like y decide corresponder (acepta)
    /// 3. CP valida, actualiza estados y coordina efectos secundarios
    /// 
    /// Responsabilidades:
    /// - Validar que ambos usuarios existan y sean válidos
    /// - Verificar que existe un match pendiente
    /// - Completar el match (hacer recíproco el like)
    /// - Incrementar contadores de ambos usuarios
    /// - Crear notificaciones para ambos usuarios
    /// - Garantizar atomicidad (todo o nada)
    /// </summary>
    public class CorresponderMatchCP
    {
        private readonly MatchCEN _matchCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NotificacionCEN _notificacionCEN;
        private readonly IMatchRepository _matchRepo;
        private readonly IUnitOfWork _uow;

        public CorresponderMatchCP(
            MatchCEN matchCEN,
            UsuarioCEN usuarioCEN,
            NotificacionCEN notificacionCEN,
            IMatchRepository matchRepo,
            IUnitOfWork uow)
        {
            _matchCEN = matchCEN ?? throw new ArgumentNullException(nameof(matchCEN));
            _usuarioCEN = usuarioCEN ?? throw new ArgumentNullException(nameof(usuarioCEN));
            _notificacionCEN = notificacionCEN ?? throw new ArgumentNullException(nameof(notificacionCEN));
            _matchRepo = matchRepo ?? throw new ArgumentNullException(nameof(matchRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Ejecuta el caso de uso: Corresponder Match
        /// 
        /// Precondiciones:
        /// - Usuario A existe y NO está baneado
        /// - Usuario B existe y NO está baneado
        /// - Existe un Match pendiente (LikeEmisor=true, LikeReceptor=false)
        /// 
        /// Acciones:
        /// 1. Validar usuarios
        /// 2. Buscar match pendiente entre A y B
        /// 3. Actualizar match a mutuo (LikeReceptor=true)
        /// 4. Incrementar NumMatchs en ambos usuarios
        /// 5. Crear notificaciones para ambos
        /// 6. Guardar todo transaccionalmente
        /// 
        /// Postcondiciones:
        /// - Match es mutuo (FechaMatch ≠ null)
        /// - Ambos usuarios tienen NumMatchs+1
        /// - Ambos usuarios notificados
        /// - TODO guardado en BD
        /// </summary>
        public Match Corresponder(long receptorId, long emisorId)
        {
            try
            {
                // VALIDACION 1: IDs válidos
                if (receptorId <= 0 || emisorId <= 0)
                    throw new InvalidOperationException("Los IDs de los usuarios son inválidos");

                // VALIDACION 2: No auto-matching
                if (receptorId == emisorId)
                    throw new InvalidOperationException("Un usuario no puede hacer match consigo mismo");

                // VALIDACION 3: Obtener y validar usuarios
                var receptor = _usuarioCEN.DamePorId(receptorId);
                if (receptor == null)
                    throw new InvalidOperationException($"Usuario receptor {receptorId} no encontrado");

                var emisor = _usuarioCEN.DamePorId(emisorId);
                if (emisor == null)
                    throw new InvalidOperationException($"Usuario emisor {emisorId} no encontrado");

                // VALIDACION 4: Verificar que ninguno esté baneado
                if (receptor.Baneado)
                    throw new InvalidOperationException($"El usuario receptor {receptorId} está baneado");

                if (emisor.Baneado)
                    throw new InvalidOperationException($"El usuario emisor {emisorId} está baneado");

                // VALIDACION 5: Buscar match pendiente entre estos usuarios
                var matchPendiente = ObtenerMatchPendiente(emisorId, receptorId);
                if (matchPendiente == null)
                    throw new InvalidOperationException(
                        $"No existe un match pendiente entre usuario {emisorId} y usuario {receptorId}");

                // VALIDACION 6: Verificar que el match no sea ya mutuo
                if (matchPendiente.LikeEmisor && matchPendiente.LikeReceptor)
                    throw new InvalidOperationException(
                        "Este match ya es mutuo, no puede corresponder nuevamente");

                // ========== TRANSACCION COMIENZA ==========

                // PASO 1: Actualizar match a mutuo
                matchPendiente.LikeReceptor = true;
                matchPendiente.FechaMatch = DateTime.Now;
                _matchRepo.Modify(matchPendiente);

                // PASO 2: Incrementar contadores en ambos usuarios
                receptor.NumMatchs++;
                emisor.NumMatchs++;
                _usuarioCEN.Modificar(receptor.Id, receptor.Nombre, receptor.Email);
                _usuarioCEN.Modificar(emisor.Id, emisor.Nombre, emisor.Email);

                // PASO 3: Crear notificaciones para ambos usuarios
                var notificacionReceptor = _notificacionCEN.Crear(
                    receptor,
                    $"¡{emisor.Nombre} aceptó tu like! ¡Tienen un match! 🎉"
                );

                var notificacionEmisor = _notificacionCEN.Crear(
                    emisor,
                    $"¡{receptor.Nombre} correspondió tu like! ¡Tienen un match! 🎉"
                );

                // PASO 4: Guardar todo en una sola transacción
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========

                return matchPendiente;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al corresponder match: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un match pendiente entre dos usuarios
        /// 
        /// Busca:
        /// - Match donde emisorId→receptorId Y LikeEmisor=true, LikeReceptor=false
        /// - O también: receptorId→emisorId con la misma condición
        /// </summary>
        private Match? ObtenerMatchPendiente(long emisorId, long receptorId)
        {
            // Buscar todos los matches del emisor
            var matchesDelEmisor = _matchRepo.GetByUsuario(emisorId);

            foreach (var match in matchesDelEmisor)
            {
                // Buscar match donde:
                // - Emisor es emisorId, Receptor es receptorId
                // - LikeEmisor=true (emisor dio like)
                // - LikeReceptor=false (receptor aún no ha correspondido)
                if (match.Emisor.Id == emisorId && 
                    match.Receptor.Id == receptorId &&
                    match.LikeEmisor && 
                    !match.LikeReceptor)
                {
                    return match;
                }

                // También buscar en la dirección inversa (por si acaso)
                if (match.Emisor.Id == receptorId && 
                    match.Receptor.Id == emisorId &&
                    match.LikeEmisor && 
                    !match.LikeReceptor)
                {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene información sobre estado del match entre dos usuarios
        /// Útil para UI/UX: saber qué botones mostrar
        /// </summary>
        public MatchStatus ObtenerEstadoMatch(long usuarioId1, long usuarioId2)
        {
            var matches = _matchRepo.GetByUsuario(usuarioId1);

            foreach (var match in matches)
            {
                if ((match.Emisor.Id == usuarioId1 && match.Receptor.Id == usuarioId2) ||
                    (match.Emisor.Id == usuarioId2 && match.Receptor.Id == usuarioId1))
                {
                    if (match.LikeEmisor && match.LikeReceptor)
                        return MatchStatus.MUTUO;

                    if (match.LikeEmisor)
                        return MatchStatus.PENDIENTE;

                    return MatchStatus.RECHAZADO;
                }
            }

            return MatchStatus.NO_EXISTE;
        }

        /// <summary>
        /// Estados posibles de un match
        /// </summary>
        public enum MatchStatus
        {
            NO_EXISTE,    // No hay match entre estos usuarios
            PENDIENTE,    // Un usuario dio like, el otro no ha respondido
            MUTUO,        // Ambos dieron like
            RECHAZADO     // El receptor rechazó (LikeEmisor=false)
        }
    }
}
