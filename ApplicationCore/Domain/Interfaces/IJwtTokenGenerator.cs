using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para generar y validar tokens JWT.
    /// 
    /// JWT (JSON Web Token) estructura:
    /// header.payload.signature
    /// 
    /// Ejemplo:
    /// eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
    /// eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.
    /// SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Genera un token JWT para un usuario autenticado.
        /// </summary>
        /// <param name="usuario">Usuario para el cual generar el token</param>
        /// <returns>Token JWT como string (incluye header, payload y signature)</returns>
        /// <exception cref="ArgumentException">Si usuario es nulo</exception>
        /// <exception cref="InvalidOperationException">Si hay error al generar el token</exception>
        string GenerarToken(Usuario usuario);

        /// <summary>
        /// Valida un token JWT y extrae los claims.
        /// Útil para verificar que el token no está expirado y es válido.
        /// </summary>
        /// <param name="token">Token JWT a validar</param>
        /// <returns>true si el token es válido, false en caso contrario</returns>
        bool ValidarToken(string token);
    }
}
