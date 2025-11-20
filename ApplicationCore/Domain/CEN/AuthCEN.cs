using System;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Interfaces;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// AuthCEN (Authentication Center - Núcleo de Autenticación)
    /// 
    /// Responsabilidades:
    /// - Validación de credenciales contra BD
    /// - Hashing seguro de contraseñas
    /// - Generación de tokens JWT
    /// - Lógica pura de autenticación (sin orquestación)
    /// 
    /// NO hace:
    /// - Crear usuarios (eso es UsuarioCEN)
    /// - Llamar a controladores (eso es AuthCP)
    /// - Gestionar transacciones complejas (eso es AuthCP)
    /// </summary>
    public class AuthCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _uow;

        public AuthCEN(IUsuarioRepository usuarioRepo, IPasswordHasher passwordHasher, IUnitOfWork uow)
        {
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Valida las credenciales (email + password) contra la BD.
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña en plaintext (será hasheada para comparar)</param>
        /// <returns>Usuario si las credenciales son válidas, null en caso contrario</returns>
        /// <exception cref="ArgumentException">Si email o password están vacíos</exception>
        /// <exception cref="InvalidOperationException">Si hay error al verificar la contraseña</exception>
        public Usuario? ValidarCredenciales(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            try
            {
                // Buscar usuario por email
                var usuario = _usuarioRepo.GetByEmail(email).FirstOrDefault();
                if (usuario == null)
                {
                    // Usuario no existe - retornar null (no lanzar excepción por seguridad)
                    // Evita ataques de enumeración (no revelar qué emails existen)
                    return null;
                }

                // Usuario existe, verificar contraseña
                if (_passwordHasher.VerifyPassword(password, usuario.Pass))
                {
                    return usuario;
                }

                // Contraseña incorrecta
                return null;
            }
            catch (ArgumentException)
            {
                // Si el hash está corrupto, retornar null (login fallido)
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al validar credenciales", ex);
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID (útil para regenerar token si expira)
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Usuario si existe, null en caso contrario</returns>
        public Usuario? ObtenerUsuarioPorId(long usuarioId)
        {
            if (usuarioId <= 0)
                return null;

            return _usuarioRepo.GetById(usuarioId);
        }

        /// <summary>
        /// Actualiza la fecha de último login de un usuario.
        /// Útil para auditoría y tracking de sesiones.
        /// </summary>
        /// <param name="usuario">Usuario a actualizar</param>
        public void ActualizarFechaUltimoLogin(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.FechaUltimoLogin = DateTime.UtcNow;
            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        /// <summary>
        /// Verifica si un usuario está baneado (no debería poder hacer login)
        /// </summary>
        /// <param name="usuario">Usuario a verificar</param>
        /// <returns>true si el usuario está baneado</returns>
        public bool EstaBaneado(Usuario usuario)
        {
            return usuario?.Baneado ?? false;
        }
    }
}
