using System;

namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Filtro de lectura para búsqueda avanzada de Fotos
    /// Permite filtrar fotos por usuario, URL y otros criterios
    /// </summary>
    public class FotoReadFilter
    {
        /// <summary>
        /// ID del usuario propietario de las fotos (opcional)
        /// </summary>
        public long? UsuarioId { get; set; }

        /// <summary>
        /// Buscar por URL parcial (opcional)
        /// </summary>
        public string? UrlBusqueda { get; set; }

        /// <summary>
        /// Filtrar fotos creadas desde este ID (opcional)
        /// </summary>
        public long? IdDesde { get; set; }

        /// <summary>
        /// Filtrar fotos hasta este ID (opcional)
        /// </summary>
        public long? IdHasta { get; set; }

        /// <summary>
        /// Número máximo de resultados (paginación)
        /// </summary>
        public int? Limite { get; set; } = 50;

        /// <summary>
        /// Número de registros a saltar (paginación)
        /// </summary>
        public int? Offset { get; set; } = 0;

        /// <summary>
        /// Campo por el que ordenar (Id, Url)
        /// </summary>
        public string? OrdenarPor { get; set; } = "Id";

        /// <summary>
        /// Dirección del ordenamiento (ASC o DESC)
        /// Por defecto DESC para obtener las más recientes primero
        /// </summary>
        public string? Direccion { get; set; } = "DESC";
    }
}
