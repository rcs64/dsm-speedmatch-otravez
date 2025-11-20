using System;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// Caso de Uso: Iniciar Match
    /// 
    /// Flujo:
    /// 1. Usuario A ve a Usuario B
    /// 2. Usuario A le da like (inicia el match)
    /// 3. Se crea un Match pendiente de ser aceptado
    /// 4. Se notifica a Usuario B
    /// 
    /// Responsabilidades:
    /// - Validar que ambos usuarios existan y sean válidos
    /// - Verificar que no exista match previo
    /// - Crear nuevo match (LikeEmisor=true, LikeReceptor=false)
    /// - Incrementar LikesEnviados del usuario que da like
    /// - Crear notificación para el usuario que recibe like
    /// - Garantizar atomicidad
    /// </summary>
    public class IniciarMatchCP
    {
        private readonly MatchCEN _matchCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NotificacionCEN _notificacionCEN;
        private readonly IUnitOfWork _uow;

        public IniciarMatchCP(
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
        /// Ejecuta el caso de uso: Iniciar Match
        /// 
        /// Precondiciones:
        /// - Usuario A (emisor) existe y NO está baneado
        /// - Usuario B (receptor) existe y NO está baneado
        /// - No existe match previo entre ambos
        /// 
        /// Acciones:
        /// 1. Validar usuarios
        /// 2. Crear nuevo Match (LikeEmisor=true, LikeReceptor=false)
        /// 3. Incrementar LikesEnviados del emisor
        /// 4. Crear notificación para el receptor
        /// 5. Guardar transaccionalmente
        /// 
        /// Postcondiciones:
        /// - Match creado en estado pendiente
        /// - Emisor.LikesEnviados incrementado
        /// - Receptor notificado
        /// - TODO guardado en BD
        /// </summary>
        public Match Iniciar(long emisorId, long receptorId)
        {
            try
            {
                // VALIDACION 1: IDs válidos
                if (emisorId <= 0 || receptorId <= 0)
                    throw new InvalidOperationException("Los IDs de los usuarios son inválidos");

                // VALIDACION 2: No auto-matching
                if (emisorId == receptorId)
                    throw new InvalidOperationException("Un usuario no puede dar like a sí mismo");

                // VALIDACION 3: Obtener y validar usuarios
                var emisor = _usuarioCEN.DamePorId(emisorId);
                if (emisor == null)
                    throw new InvalidOperationException($"Usuario emisor {emisorId} no encontrado");

                var receptor = _usuarioCEN.DamePorId(receptorId);
                if (receptor == null)
                    throw new InvalidOperationException($"Usuario receptor {receptorId} no encontrado");

                // VALIDACION 4: Verificar que ninguno esté baneado
                if (emisor.Baneado)
                    throw new InvalidOperationException($"El usuario emisor {emisorId} está baneado");

                if (receptor.Baneado)
                    throw new InvalidOperationException($"El usuario receptor {receptorId} está baneado");

                // VALIDACION 5: Verificar que no exista match previo
                var matchExistente = _matchCEN.DamePorUsuario(emisorId)
                    .FirstOrDefault(m => 
                        (m.Emisor.Id == emisorId && m.Receptor.Id == receptorId) ||
                        (m.Emisor.Id == receptorId && m.Receptor.Id == emisorId));

                if (matchExistente != null)
                    throw new InvalidOperationException(
                        "Ya existe un match entre estos usuarios");

                // ========== TRANSACCION COMIENZA ==========

                // PASO 1: Crear nuevo match
                var nuevoMatch = _matchCEN.Crear(emisor, receptor, likeEmisor: true);

                // PASO 2: Incrementar LikesEnviados del emisor
                _usuarioCEN.EnviarLike(emisorId);

                // PASO 3: Crear notificación para el receptor
                _notificacionCEN.Crear(
                    receptor,
                    $"¡{emisor.Nombre} te dio un like! 💘"
                );

                // PASO 4: Guardar todo en una sola transacción
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========

                return nuevoMatch;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al iniciar match: {ex.Message}", ex);
            }
        }
    }
}
