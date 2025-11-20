using System;
using ApplicationCore.Domain.Interfaces;
using BCrypt.Net;

namespace Infrastructure.Security
{
    /// <summary>
    /// Implementación de hash de contraseñas usando BCrypt.
    /// 
    /// Características de seguridad:
    /// - Salting automático (incluido en el hash)
    /// - Función de derivación lenta (2^12 = 4096 iteraciones por defecto)
    /// - Resistente a ataques GPU/ASIC
    /// - Adaptable: el work factor puede aumentarse con el tiempo
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Work factor para BCrypt. Rango válido: 4-31.
        /// Valor recomendado: 12 (balance entre seguridad y rendimiento)
        /// 12 = ~250ms por hash en CPU moderna
        /// </summary>
        private const int WorkFactor = 12;

        /// <summary>
        /// Genera un hash seguro de una contraseña en plaintext usando BCrypt.
        /// </summary>
        /// <param name="password">Contraseña sin encriptar</param>
        /// <returns>Hash seguro (incluye salt + trabajo codificado)</returns>
        /// <exception cref="ArgumentException">Si la contraseña es nula o vacía</exception>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // BCrypt tiene límite de 72 caracteres
            if (password.Length > 72)
                throw new ArgumentException("La contraseña no puede exceder 72 caracteres", nameof(password));

            try
            {
                // BCrypt.HashPassword genera automáticamente un salt y lo incluye en el resultado
                return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al hashear la contraseña", ex);
            }
        }

        /// <summary>
        /// Verifica si una contraseña en plaintext coincide con un hash almacenado.
        /// </summary>
        /// <param name="password">Contraseña sin encriptar a verificar</param>
        /// <param name="hash">Hash almacenado en la BD</param>
        /// <returns>true si coinciden, false en caso contrario</returns>
        /// <exception cref="ArgumentException">Si el hash es inválido</exception>
        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("El hash no puede estar vacío", nameof(hash));

            try
            {
                // BCrypt extrae el salt del hash y lo utiliza para verificar
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (SaltParseException ex)
            {
                throw new ArgumentException("El hash de contraseña es inválido", nameof(hash), ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al verificar la contraseña", ex);
            }
        }
    }
}
