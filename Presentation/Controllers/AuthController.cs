using System;
using System.Security.Claims;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// AuthController - Endpoints de autenticación
    /// 
    /// Responsabilidades:
    /// - Recibir requests HTTP (LoginRequest, RegisterRequest)
    /// - Orquestar con AuthCP
    /// - Generar tokens JWT
    /// - Retornar respuestas (LoginResponse)
    /// 
    /// Endpoints:
    /// POST /api/auth/login       → Autenticar usuario existente
    /// POST /api/auth/register    → Crear nuevo usuario y autenticar
    /// POST /api/auth/logout      → Invalidar sesión (client-side)
    /// GET /api/auth/verify       → Verificar que el JWT es válido
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthCP _authCP;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthController(AuthCP authCP, IJwtTokenGenerator jwtTokenGenerator)
        {
            _authCP = authCP ?? throw new ArgumentNullException(nameof(authCP));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        }

        /// <summary>
        /// Endpoint de LOGIN
        /// 
        /// Flow:
        /// 1. Cliente envía: { email, password }
        /// 2. AuthCP.Login() valida credenciales
        /// 3. JwtTokenGenerator genera token
        /// 4. Retorna: { token, usuarioId, nombre, email, tipoPlan }
        /// 
        /// Curl:
        /// curl -X POST http://localhost:5000/api/auth/login \
        ///   -H "Content-Type: application/json" \
        ///   -d '{"email":"user@example.com","password":"password123"}'
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Email y password son requeridos" });

            try
            {
                // Validar credenciales y obtener usuario
                var usuario = _authCP.Login(request.Email, request.Password);

                // Generar token JWT
                var token = _jwtTokenGenerator.GenerarToken(usuario);

                // Preparar respuesta
                var response = new LoginResponse
                {
                    Token = token,
                    UsuarioId = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    TipoPlan = usuario.TipoPlan.ToString(),
                    FechaLogin = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al procesar login", details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de REGISTRO
        /// 
        /// Flow:
        /// 1. Cliente envía: { nombre, email, password, tipoPlan }
        /// 2. AuthCP.Register() crea usuario y hashea password
        /// 3. JwtTokenGenerator genera token automáticamente
        /// 4. Retorna: { token, usuarioId, nombre, email, tipoPlan }
        /// 
        /// Curl:
        /// curl -X POST http://localhost:5000/api/auth/register \
        ///   -H "Content-Type: application/json" \
        ///   -d '{"nombre":"Juan Pérez","email":"juan@example.com","password":"password123","tipoPlan":"Gratuito"}'
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Nombre, email y password son requeridos" });

            try
            {
                // Validar y parsear el plan
                if (!Enum.TryParse<Plan>(request.TipoPlan, out var plan))
                    plan = Plan.Gratuito;

                // Crear usuario y hashear password
                var usuario = _authCP.Register(request.Nombre, request.Email, request.Password, plan);

                // Generar token JWT
                var token = _jwtTokenGenerator.GenerarToken(usuario);

                // Preparar respuesta
                var response = new LoginResponse
                {
                    Token = token,
                    UsuarioId = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    TipoPlan = usuario.TipoPlan.ToString(),
                    FechaLogin = DateTime.UtcNow
                };

                return CreatedAtAction(nameof(Register), response);
            }
            catch (InvalidOperationException ex)
            {
                // Email ya existe
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al procesar registro", details = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de LOGOUT (informativo)
        /// 
        /// Nota: En arquitectura JWT stateless, no hay "logout" real.
        /// El cliente simplemente descarta el token.
        /// Este endpoint es informativo para que el cliente sepa que puede descartar el token.
        /// 
        /// Si necesitas revocación de tokens en el futuro:
        /// - Mantener lista de tokens revocados en caché (Redis)
        /// - Validar cada token contra la lista de revocados
        /// 
        /// Curl:
        /// curl -X POST http://localhost:5000/api/auth/logout \
        ///   -H "Authorization: Bearer eyJhbGciOiJIUzI1NiI..."
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // En JWT stateless, el logout es client-side (descartar token)
            return Ok(new { message = "Logged out successfully. Discard your token." });
        }

        /// <summary>
        /// Endpoint para VERIFICAR TOKEN
        /// 
        /// Útil para:
        /// - Comprobar que el token es válido
        /// - Obtener datos del usuario autenticado
        /// - Renovar token si está a punto de expirar
        /// 
        /// Curl:
        /// curl -X GET http://localhost:5000/api/auth/verify \
        ///   -H "Authorization: Bearer eyJhbGciOiJIUzI1NiI..."
        /// </summary>
        [HttpGet("verify")]
        [Authorize]
        public IActionResult Verify()
        {
            try
            {
                // Obtener claims del JWT desde el contexto HTTP
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                var nombreClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var planClaim = User.FindFirst("Plan")?.Value;

                if (string.IsNullOrEmpty(usuarioIdClaim))
                    return Unauthorized(new { error = "Token no válido" });

                return Ok(new
                {
                    usuarioId = usuarioIdClaim,
                    email = emailClaim,
                    nombre = nombreClaim,
                    plan = planClaim,
                    message = "Token is valid"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al verificar token", details = ex.Message });
            }
        }
    }
}
