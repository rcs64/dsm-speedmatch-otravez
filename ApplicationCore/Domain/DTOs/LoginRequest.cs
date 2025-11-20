namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Request DTO para login.
    /// 
    /// Responsabilidades:
    /// - Validar formato de datos (decoradores de validación)
    /// - Transportar datos entre cliente (HTTP POST) y controlador
    /// - NO contiene lógica de negocio
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email del usuario
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña en plaintext (será validada contra el hash en BD)
        /// </summary>
        public required string Password { get; set; }
    }
}
