using System;

namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Response DTO para login y registro.
    /// 
    /// Contiene:
    /// - El token JWT (para incluir en headers Authorization: Bearer {token})
    /// - Datos del usuario autenticado (para mostrar en frontend)
    /// 
    /// Responsabilidades:
    /// - Transportar respuesta desde controlador hacia cliente
    /// - Ocultar campos sensibles (no incluye password)
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Token JWT para autenticarse en futuras solicitudes.
        /// Formato: jwt_header.jwt_payload.jwt_signature
        /// Uso en client: Authorization: Bearer {Token}
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// ID del usuario autenticado (para identificar en próximas llamadas)
        /// </summary>
        public required long UsuarioId { get; set; }

        /// <summary>
        /// Nombre del usuario autenticado
        /// </summary>
        public required string Nombre { get; set; }

        /// <summary>
        /// Email del usuario autenticado
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Tipo de plan del usuario (Gratuito o Premium)
        /// </summary>
        public required string TipoPlan { get; set; }

        /// <summary>
        /// Timestamp de autenticación (útil para cliente)
        /// </summary>
        public DateTime FechaLogin { get; set; } = DateTime.UtcNow;
    }
}
