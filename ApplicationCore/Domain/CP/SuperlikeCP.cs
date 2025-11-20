using System;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// Caso de Uso: Superlike Premium
    /// 
    /// Flujo:
    /// 1. Usuario Premium tiene SuperlikesDisponibles > 0
    /// 2. Usuario Premium da SUPERLIKE a otro usuario
    /// 3. Se consume 1 superlike disponible
    /// 4. Se crea un Match especial
    /// 5. El receptor recibe DOBLE conteo de likes (LikesRecibidos += 2 en lugar de +1)
    /// 6. Se notifica al receptor con mensaje especial
    /// 
    /// Diferencia con Like Normal:
    /// - Normal: Usuario da like → LikesRecibidos += 1
    /// - Superlike: Usuario Premium da superlike → LikesRecibidos += 2
    /// 
    /// Responsabilidades:
    /// - Validar que emisor es Premium
    /// - Validar que tiene SuperlikesDisponibles > 0
    /// - Crear Match marcado como Superlike
    /// - Restar 1 de SuperlikesDisponibles
    /// - Incrementar LikesRecibidos en +2 (DOBLE)
    /// - Notificar al receptor
    /// - Garantizar atomicidad
    /// </summary>
    public class SuperlikeCP
    {
        private readonly MatchCEN _matchCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly NotificacionCEN _notificacionCEN;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IMatchRepository _matchRepo;
        private readonly IUnitOfWork _uow;

        public SuperlikeCP(
            MatchCEN matchCEN,
            UsuarioCEN usuarioCEN,
            NotificacionCEN notificacionCEN,
            IUsuarioRepository usuarioRepo,
            IMatchRepository matchRepo,
            IUnitOfWork uow)
        {
            _matchCEN = matchCEN ?? throw new ArgumentNullException(nameof(matchCEN));
            _usuarioCEN = usuarioCEN ?? throw new ArgumentNullException(nameof(usuarioCEN));
            _notificacionCEN = notificacionCEN ?? throw new ArgumentNullException(nameof(notificacionCEN));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _matchRepo = matchRepo ?? throw new ArgumentNullException(nameof(matchRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Ejecuta un SUPERLIKE: usuario Premium da superlike a otro usuario
        /// 
        /// Precondiciones:
        /// - Emisor es Premium (TipoPlan == Plan.Premium)
        /// - Emisor tiene SuperlikesDisponibles > 0
        /// - Receptor existe y NO está baneado
        /// - Emisor NO está baneado
        /// - No existe match previo entre ellos
        /// 
        /// Acciones:
        /// 1. Validar emisor es Premium
        /// 2. Validar tiene superlike disponible
        /// 3. Validar usuarios
        /// 4. Validar no hay match previo
        /// 5. Crear Match
        /// 6. Restar SuperlikesDisponibles
        /// 7. Incrementar LikesRecibidos del receptor EN +2 (DOBLE)
        /// 8. Crear notificación especial
        /// 9. Guardar todo transaccionalmente
        /// 
        /// Postcondiciones:
        /// - Match creado
        /// - Emisor.SuperlikesDisponibles - 1
        /// - Receptor.LikesRecibidos + 2
        /// - Notificación enviada
        /// - TODO guardado en BD
        /// </summary>
        public Match Superlike(long emisorId, long receptorId)
        {
            try
            {
                // VALIDACION 1: IDs válidos
                if (emisorId <= 0 || receptorId <= 0)
                    throw new InvalidOperationException("Los IDs de los usuarios son inválidos");

                // VALIDACION 2: No auto-superlike
                if (emisorId == receptorId)
                    throw new InvalidOperationException("Un usuario no puede superlikear a sí mismo");

                // VALIDACION 3: Obtener y validar emisor
                var emisor = _usuarioCEN.DamePorId(emisorId);
                if (emisor == null)
                    throw new InvalidOperationException($"Usuario emisor {emisorId} no encontrado");

                // VALIDACION 4: Verificar que emisor sea PREMIUM
                if (emisor.TipoPlan != Plan.Premium)
                    throw new InvalidOperationException(
                        $"Solo usuarios Premium pueden hacer superlike. Usuario {emisorId} es {emisor.TipoPlan}");

                // VALIDACION 5: Verificar que tiene superlikes disponibles
                if (emisor.SuperlikesDisponibles <= 0)
                    throw new InvalidOperationException(
                        $"Usuario Premium {emisorId} no tiene superlikes disponibles. Disponibles: {emisor.SuperlikesDisponibles}");

                // VALIDACION 6: Obtener y validar receptor
                var receptor = _usuarioCEN.DamePorId(receptorId);
                if (receptor == null)
                    throw new InvalidOperationException($"Usuario receptor {receptorId} no encontrado");

                // VALIDACION 7: Verificar que ninguno esté baneado
                if (emisor.Baneado)
                    throw new InvalidOperationException($"El usuario emisor {emisorId} está baneado");

                if (receptor.Baneado)
                    throw new InvalidOperationException($"El usuario receptor {receptorId} está baneado");

                // VALIDACION 8: Verificar que no exista match previo
                var matchExistente = _matchRepo.GetByUsuario(emisorId)
                    .FirstOrDefault(m => 
                        (m.Emisor.Id == emisorId && m.Receptor.Id == receptorId) ||
                        (m.Emisor.Id == receptorId && m.Receptor.Id == emisorId));

                if (matchExistente != null)
                    throw new InvalidOperationException(
                        $"Ya existe un match entre usuario {emisorId} y usuario {receptorId}");

                // ========== TRANSACCION COMIENZA ==========

                // PASO 1: Crear Match con Superlike
                var nuevoMatch = new Match
                {
                    Emisor = emisor,
                    Receptor = receptor,
                    LikeEmisor = true,
                    LikeReceptor = false,
                    FechaInicio = DateTime.Now,
                    FechaMatch = null,
                    EsSuperlike = true  // Marcar como superlike
                };

                _matchRepo.New(nuevoMatch);

                // PASO 2: Restar 1 de SuperlikesDisponibles del emisor
                emisor.SuperlikesDisponibles--;
                _usuarioRepo.Modify(emisor);

                // PASO 3: INCREMENTAR DOBLE (+=2) los LikesRecibidos del receptor
                // Este es el diferencial del superlike
                receptor.LikesRecibidos += 2;
                _usuarioRepo.Modify(receptor);

                // PASO 4: Crear notificación especial para el receptor
                string mensajeEspecial = $"⭐ ¡{emisor.Nombre} te envió un SUPERLIKE! ⭐ " +
                    $"Te gustó tanto que equivale a 2 likes normales. ¡Tienes que verlo!";

                _notificacionCEN.Crear(receptor, mensajeEspecial);

                // PASO 5: Guardar todo en una sola transacción
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========

                return nuevoMatch;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al ejecutar superlike: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Compra/Extiende superlikes para un usuario Premium
        /// Aumenta SuperlikesDisponibles
        /// 
        /// Precondiciones:
        /// - Usuario es Premium
        /// - cantidad > 0
        /// 
        /// Acciones:
        /// 1. Validar usuario existe y es Premium
        /// 2. Validar cantidad > 0
        /// 3. Aumentar SuperlikesDisponibles
        /// 4. Guardar
        /// 
        /// Postcondiciones:
        /// - Usuario.SuperlikesDisponibles + cantidad
        /// - Cambio guardado en BD
        /// </summary>
        public void ComprarSuperlikes(long usuarioId, int cantidad)
        {
            try
            {
                // VALIDACION 1: ID válido
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                // VALIDACION 2: Cantidad válida
                if (cantidad <= 0)
                    throw new InvalidOperationException("La cantidad de superlikes debe ser mayor a 0");

                // VALIDACION 3: Obtener usuario
                var usuario = _usuarioCEN.DamePorId(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                // VALIDACION 4: Verificar que sea Premium
                if (usuario.TipoPlan != Plan.Premium)
                    throw new InvalidOperationException(
                        $"Solo usuarios Premium pueden comprar superlikes. Usuario es {usuario.TipoPlan}");

                // ========== TRANSACCION COMIENZA ==========

                // PASO 1: Aumentar superlikes disponibles
                usuario.SuperlikesDisponibles += cantidad;

                // PASO 2: Guardar
                _usuarioRepo.Modify(usuario);
                _uow.SaveChanges();

                // ========== TRANSACCION COMPLETADA ==========
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al comprar superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene información del estado de superlikes de un usuario
        /// Útil para mostrar en UI
        /// </summary>
        public SuperlikeInfo ObtenerInfoSuperlikes(long usuarioId)
        {
            try
            {
                // VALIDACION: Usuario existe
                var usuario = _usuarioCEN.DamePorId(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                // Retornar información
                return new SuperlikeInfo
                {
                    UsuarioId = usuario.Id,
                    TipoPlan = usuario.TipoPlan,
                    EsPremium = usuario.TipoPlan == Plan.Premium,
                    SuperlikesDisponibles = usuario.SuperlikesDisponibles,
                    SuperlikesUsados = usuario.Superlikes,
                    PuedeHacerSuperlike = usuario.TipoPlan == Plan.Premium && usuario.SuperlikesDisponibles > 0
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener info de superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los superlikes RECIBIDOS por un usuario
        /// Útil para mostrar quién te envió superlikes
        /// </summary>
        public int ContarSuperlikes(long usuarioId)
        {
            try
            {
                // VALIDACION: Usuario existe
                var usuario = _usuarioCEN.DamePorId(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                // Contar superlikes donde este usuario es receptor
                var superlikes = _matchRepo.GetByUsuario(usuarioId)
                    .Where(m => m.Receptor.Id == usuarioId && m.EsSuperlike)
                    .Count();

                return superlikes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al contar superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Información sobre los superlikes de un usuario
        /// </summary>
        public class SuperlikeInfo
        {
            public long UsuarioId { get; set; }
            public Plan TipoPlan { get; set; }
            public bool EsPremium { get; set; }
            public int SuperlikesDisponibles { get; set; }
            public int SuperlikesUsados { get; set; }
            public bool PuedeHacerSuperlike { get; set; }
        }
    }
}
