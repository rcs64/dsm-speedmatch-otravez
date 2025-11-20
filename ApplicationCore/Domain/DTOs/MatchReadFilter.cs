using System;

namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Filtro de lectura para búsqueda avanzada de Matches
    /// Permite filtrar matches por emisor, receptor, estado y fechas
    /// </summary>
    public class MatchReadFilter
    {
        /// <summary>
        /// ID del usuario emisor (opcional)
        /// </summary>
        public long? EmisorId { get; set; }

        /// <summary>
        /// ID del usuario receptor (opcional)
        /// </summary>
        public long? ReceptorId { get; set; }

        /// <summary>
        /// Filtrar solo por likes del emisor confirmados (opcional)
        /// </summary>
        public bool? LikeEmisor { get; set; }

        /// <summary>
        /// Filtrar solo por likes del receptor confirmados (opcional)
        /// </summary>
        public bool? LikeReceptor { get; set; }

        /// <summary>
        /// Filtrar solo superlikes (opcional)
        /// </summary>
        public bool? EsSuperlike { get; set; }

        /// <summary>
        /// Filtrar matches creados desde esta fecha (opcional)
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Filtrar matches creados hasta esta fecha (opcional)
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>
        /// Filtrar solo matches confirmados (ambos dieron like)
        /// </summary>
        public bool? SoloConfirmados { get; set; }

        /// <summary>
        /// Número máximo de resultados (paginación)
        /// </summary>
        public int? Limite { get; set; } = 50;

        /// <summary>
        /// Número de registros a saltar (paginación)
        /// </summary>
        public int? Offset { get; set; } = 0;
    }
}
