using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security
{
    /// <summary>
    /// Implementación de generación de JWT usando System.IdentityModel.Tokens.Jwt
    /// 
    /// Estructura del JWT:
    /// 1. Header: { "alg": "HS256", "typ": "JWT" }
    /// 2. Payload: Claims del usuario (id, email, nombre, plan, iat, exp)
    /// 3. Signature: HMAC-SHA256(header + payload, secret)
    /// 
    /// El cliente envía el token así:
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiI...
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));

            if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
                throw new ArgumentException("JwtSettings.Secret no puede estar vacío", nameof(jwtSettings));

            if (_jwtSettings.Secret.Length < 32)
                throw new ArgumentException(
                    "JwtSettings.Secret debe tener al menos 32 caracteres (256 bits)",
                    nameof(jwtSettings));
        }

        /// <summary>
        /// Genera un token JWT para un usuario.
        /// 
        /// Claims incluidos:
        /// - sub (subject): ID del usuario
        /// - email: Email del usuario
        /// - nombre: Nombre del usuario
        /// - plan: Tipo de plan (Gratuito/Premium)
        /// - iat (issued at): Timestamp de creación
        /// - exp (expiration): Timestamp de expiración
        /// - iss (issuer): Quién emitió el token
        /// - aud (audience): Para quién es el token
        /// </summary>
        public string GenerarToken(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            try
            {
                // Convertir secret a bytes
                var secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
                var key = new SymmetricSecurityKey(secretBytes);
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Crear claims (información del usuario en el token)
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("Plan", usuario.TipoPlan.ToString()),
                    new Claim("Baneado", usuario.Baneado.ToString())
                };

                // Crear el JWT
                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: credentials
                );

                // Convertir a string
                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al generar el token JWT", ex);
            }
        }

        /// <summary>
        /// Valida un token JWT.
        /// Verifica: firma, expiración, issuer, audience
        /// </summary>
        public bool ValidarToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
                var key = new SymmetricSecurityKey(secretBytes);

                // Validar con parámetros estrictos
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero  // Sin tolerancia de tiempo
                }, out SecurityToken validatedToken);

                return validatedToken is JwtSecurityToken;
            }
            catch
            {
                // Token inválido, expirado o mal formado
                return false;
            }
        }
    }
}
