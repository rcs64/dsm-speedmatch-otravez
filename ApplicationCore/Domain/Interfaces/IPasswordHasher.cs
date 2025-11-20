namespace ApplicationCore.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para operaciones de hash y verificación de contraseñas.
    /// Implementación: BCrypt (seguro, lento por diseño, resistente a ataques GPU)
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Genera un hash seguro de una contraseña en plaintext.
        /// </summary>
        /// <param name="password">Contraseña sin encriptar (máx 72 caracteres para BCrypt)</param>
        /// <returns>Hash de la contraseña (contiene salt incluido)</returns>
        /// <exception cref="ArgumentException">Si la contraseña es nula o vacía</exception>
        string HashPassword(string password);

        /// <summary>
        /// Verifica si una contraseña en plaintext coincide con un hash almacenado.
        /// </summary>
        /// <param name="password">Contraseña sin encriptar a verificar</param>
        /// <param name="hash">Hash almacenado en la BD</param>
        /// <returns>true si coinciden, false en caso contrario</returns>
        /// <exception cref="ArgumentException">Si hash es nulo o inválido</exception>
        bool VerifyPassword(string password, string hash);
    }
}
