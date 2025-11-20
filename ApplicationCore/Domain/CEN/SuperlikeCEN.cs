using System;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// Capa de Negocio: Superlikes Premium
    /// 
    /// Responsabilidades:
    /// - Validar lógica de superlikes
    /// - Gestionar SuperlikesDisponibles
    /// - Verificar permisos (solo Premium)
    /// - Operaciones CRUD específicas de superlikes
    /// </summary>
    public class SuperlikeCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IMatchRepository _matchRepo;
        private readonly IUnitOfWork _uow;

        public SuperlikeCEN(
            IUsuarioRepository usuarioRepo,
            IMatchRepository matchRepo,
            IUnitOfWork uow)
        {
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _matchRepo = matchRepo ?? throw new ArgumentNullException(nameof(matchRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Valida si un usuario puede hacer superlike
        /// </summary>
        public bool PuedeHacerSuperlike(long usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    return false;

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    return false;

                return usuario.TipoPlan == Plan.Premium && usuario.SuperlikesDisponibles > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene la cantidad de superlikes disponibles para un usuario
        /// </summary>
        public int ObtenerSuperlikes(long usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                if (usuario.TipoPlan != Plan.Premium)
                    return 0; // Los usuarios no-premium no tienen superlikes

                return usuario.SuperlikesDisponibles;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resta 1 superlike disponible
        /// Usado por CP después de hacer superlike
        /// </summary>
        public void RestarSuperlike(long usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                if (usuario.TipoPlan != Plan.Premium)
                    throw new InvalidOperationException(
                        $"Solo usuarios Premium pueden usar superlikes. Usuario es {usuario.TipoPlan}");

                if (usuario.SuperlikesDisponibles <= 0)
                    throw new InvalidOperationException(
                        $"Usuario {usuarioId} no tiene superlikes disponibles");

                // Restar superlike
                usuario.SuperlikesDisponibles--;

                // Guardar
                _usuarioRepo.Modify(usuario);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al restar superlike: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Suma superlikes disponibles (compra o regalo)
        /// </summary>
        public void AñadirSuperlikes(long usuarioId, int cantidad)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                if (cantidad <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                if (usuario.TipoPlan != Plan.Premium)
                    throw new InvalidOperationException(
                        $"Solo usuarios Premium pueden tener superlikes. Usuario es {usuario.TipoPlan}");

                // Añadir superlikes
                usuario.SuperlikesDisponibles += cantidad;

                // Guardar
                _usuarioRepo.Modify(usuario);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al añadir superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene la cantidad de superlikes RECIBIDOS por un usuario
        /// </summary>
        public int ContarSuperlikes(long usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                // Contar matches donde este usuario es receptor y es superlike
                var superlikes = _matchRepo.GetByUsuario(usuarioId)
                    .Where(m => m.Receptor.Id == usuarioId && m.EsSuperlike)
                    .Count();

                return superlikes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al contar superlikes recibidos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inicializa superlikes para un usuario cuando se vuelve Premium
        /// </summary>
        public void InicializarSuperlikes(long usuarioId, int cantidadInicial)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                if (cantidadInicial < 0)
                    throw new InvalidOperationException("La cantidad no puede ser negativa");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                if (usuario.TipoPlan != Plan.Premium)
                    throw new InvalidOperationException(
                        "Solo usuarios Premium pueden tener superlikes iniciales");

                // Inicializar
                usuario.SuperlikesDisponibles = cantidadInicial;

                // Guardar
                _usuarioRepo.Modify(usuario);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al inicializar superlikes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene estadísticas de uso de superlikes
        /// </summary>
        public SuperlikeEstadisticas ObtenerEstadisticas(long usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new InvalidOperationException("El ID del usuario es inválido");

                var usuario = _usuarioRepo.GetById(usuarioId);
                if (usuario == null)
                    throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

                int superlikliteralesRecibidos = ContarSuperlikes(usuarioId);

                return new SuperlikeEstadisticas
                {
                    UsuarioId = usuarioId,
                    TipoPlan = usuario.TipoPlan,
                    SuperlikesDisponibles = usuario.SuperlikesDisponibles,
                    SuperlikesUsados = usuario.Superlikes,
                    SuperlikesRecibidos = superlikliteralesRecibidos,
                    LikesRecibidos = usuario.LikesRecibidos,
                    PuntajeEquivalentePorSuperlikes = superlikliteralesRecibidos * 2 // 1 superlike = 2 likes
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Estadísticas de superlikes de un usuario
        /// </summary>
        public class SuperlikeEstadisticas
        {
            public long UsuarioId { get; set; }
            public Plan TipoPlan { get; set; }
            public int SuperlikesDisponibles { get; set; }
            public int SuperlikesUsados { get; set; }
            public int SuperlikesRecibidos { get; set; }
            public int LikesRecibidos { get; set; }
            public int PuntajeEquivalentePorSuperlikes { get; set; }
        }
    }
}
