namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Request DTO para registro.
    /// 
    /// Responsabilidades:
    /// - Validar formato de datos (decoradores de validación)
    /// - Transportar datos entre cliente (HTTP POST) y controlador
    /// - NO contiene lógica de negocio
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nombre completo del nuevo usuario
        /// </summary>
        public required string Nombre { get; set; }

        /// <summary>
        /// Email único del nuevo usuario
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña en plaintext (será hasheada en AuthCP)
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Plan elegido: "Gratuito" o "Premium"
        /// Default: Gratuito
        /// </summary>
        public string? TipoPlan { get; set; } = "Gratuito";
    }
}
