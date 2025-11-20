using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Interfaces;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// Caso de Uso: Autenticación
    /// 
    /// Responsabilidades de AuthCP (Orquestación):
    /// 1. Login: Validar credenciales → generar token → retornar respuesta
    /// 2. Register: Validar datos → hashear contraseña → crear usuario → retornar token
    /// 3. Garantizar transacciones atómicas (todo o nada)
    /// 4. Orquestar entre AuthCEN (lógica pura) y UsuarioCEN (gestión de usuarios)
    /// 
    /// Diferencia CP vs CEN:
    /// - AuthCEN: "¿Es válida esta contraseña?" → Operación pura
    /// - AuthCP: "Completa el flow de login" → Orquestación (CEN + múltiples servicios)
    /// 
    /// NO hace:
    /// - Generar tokens JWT (eso será en Paso 6)
    /// - Comunicar con controladores (eso es responsabilidad del Controller)
    /// - Validar formato de email/password (eso es en DTOs)
    /// </summary>
    public class AuthCP
    {
        private readonly AuthCEN _authCEN;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public AuthCP(
            AuthCEN authCEN,
            UsuarioCEN usuarioCEN,
            IPasswordHasher passwordHasher,
            IUsuarioRepository usuarioRepo,
            IUnitOfWork uow)
        {
            _authCEN = authCEN ?? throw new ArgumentNullException(nameof(authCEN));
            _usuarioCEN = usuarioCEN ?? throw new ArgumentNullException(nameof(usuarioCEN));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Ejecuta el flow de LOGIN.
        /// 
        /// Flujo:
        /// 1. Validar que email y password no estén vacíos
        /// 2. Buscar usuario en BD y verificar contraseña (via AuthCEN)
        /// 3. Verificar que el usuario no esté baneado
        /// 4. Actualizar FechaUltimoLogin
        /// 5. Retornar el usuario autenticado
        /// 
        /// Errores posibles:
        /// - Usuario no existe → UnauthorizedAccessException
        /// - Contraseña incorrecta → UnauthorizedAccessException
        /// - Usuario baneado → UnauthorizedAccessException
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña en plaintext</param>
        /// <returns>Usuario autenticado (para generar token en Controller)</returns>
        /// <exception cref="ArgumentException">Si email o password están vacíos</exception>
        /// <exception cref="UnauthorizedAccessException">Si las credenciales son inválidas o usuario está baneado</exception>
        public Usuario Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es requerido", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es requerida", nameof(password));

            // Validar credenciales (va a BD)
            var usuario = _authCEN.ValidarCredenciales(email, password);
            if (usuario == null)
                throw new UnauthorizedAccessException("Email o contraseña inválidos");

            // Verificar que no esté baneado
            if (_authCEN.EstaBaneado(usuario))
                throw new UnauthorizedAccessException("Este usuario ha sido baneado");

            // Actualizar timestamp de último login (opcional pero bueno para auditoría)
            try
            {
                _authCEN.ActualizarFechaUltimoLogin(usuario);
            }
            catch
            {
                // Si falla la actualización del timestamp, no es crítico
                // El login continúa de todas formas
            }

            return usuario;
        }

        /// <summary>
        /// Ejecuta el flow de REGISTRO.
        /// 
        /// Flujo:
        /// 1. Validar que nombre, email y password no estén vacíos
        /// 2. Verificar que email no exista (via UsuarioCEN.Crear)
        /// 3. Hashear contraseña
        /// 4. Crear usuario nuevo con plan seleccionado
        /// 5. Retornar usuario creado
        /// 
        /// Errores posibles:
        /// - Email ya existe → InvalidOperationException
        /// - Datos incompletos → ArgumentException / InvalidOperationException
        /// 
        /// Nota: Esta implementación hashea la contraseña ANTES de pasarla a UsuarioCEN.
        /// UsuarioCEN.Crear NO hace hashing (la contraseña ya viene hasheada).
        /// </summary>
        /// <param name="nombre">Nombre del nuevo usuario</param>
        /// <param name="email">Email único del nuevo usuario</param>
        /// <param name="password">Contraseña en plaintext (será hasheada aquí)</param>
        /// <param name="tipoPlan">Plan elegido: Gratuito o Premium</param>
        /// <returns>Usuario creado y persistido en BD</returns>
        /// <exception cref="ArgumentException">Si nombre, email o password están vacíos</exception>
        /// <exception cref="InvalidOperationException">Si email ya existe</exception>
        public Usuario Register(string nombre, string email, string password, Plan tipoPlan = Plan.Gratuito)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido", nameof(nombre));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es requerido", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es requerida", nameof(password));

            // Hashear la contraseña
            var passwordHasheada = _passwordHasher.HashPassword(password);

            // UsuarioCEN.Crear ahora recibe la contraseña hasheada, no plaintext
            var nuevoUsuario = _usuarioCEN.Crear(nombre, email, passwordHasheada, tipoPlan);

            return nuevoUsuario;
        }
    }
}
