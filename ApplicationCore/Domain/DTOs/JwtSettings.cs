namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Configuración de JWT para la aplicación.
    /// 
    /// Se carga desde appsettings.json en la sección "JwtSettings"
    /// 
    /// Ejemplo appsettings.json:
    /// {
    ///   "JwtSettings": {
    ///     "Secret": "una-clave-muy-larga-y-segura-de-almenos-32-caracteres",
    ///     "Issuer": "SpeedMatch",
    ///     "Audience": "SpeedMatchClients",
    ///     "ExpirationMinutes": 60
    ///   }
    /// }
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Clave secreta para firmar y verificar tokens.
        /// CRÍTICO: Debe tener al menos 32 caracteres (256 bits).
        /// NUNCA debe estar en el código fuente, solo en appsettings.json (que NO va a repo).
        /// En producción: usar Azure Key Vault o similar.
        /// </summary>
        public required string Secret { get; set; }

        /// <summary>
        /// Emisor del token (identificador de quién genera el token)
        /// Típicamente: nombre de la aplicación o dominio
        /// Ejemplo: "SpeedMatch"
        /// </summary>
        public required string Issuer { get; set; }

        /// <summary>
        /// Audiencia del token (a quién va dirigido)
        /// Típicamente: nombre de la aplicación cliente
        /// Ejemplo: "SpeedMatchClients"
        /// </summary>
        public required string Audience { get; set; }

        /// <summary>
        /// Minutos hasta que el token expire.
        /// Recomendado: 60 minutos (1 hora) para acceso
        /// Más largo: 1440 minutos (24 horas) para refresh tokens
        /// </summary>
        public int ExpirationMinutes { get; set; } = 60;
    }
}
