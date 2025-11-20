using System;

namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Filtro de lectura para búsqueda avanzada de Notificaciones
    /// Permite filtrar notificaciones por receptor, likes y mensaje
    /// </summary>
    public class NotificacionReadFilter
    {
        /// <summary>
        /// ID del usuario receptor que recibe la notificación (opcional)
        /// </summary>
        public long? ReceptorId { get; set; }

        /// <summary>
        /// Filtrar por rango mínimo de likes (opcional)
        /// </summary>
        public int? LikesMinimo { get; set; }

        /// <summary>
        /// Filtrar por rango máximo de likes (opcional)
        /// </summary>
        public int? LikesMaximo { get; set; }

        /// <summary>
        /// Filtrar por contenido del mensaje (búsqueda parcial) (opcional)
        /// </summary>
        public string? MensajeBusqueda { get; set; }

        /// <summary>
        /// Número máximo de resultados (paginación)
        /// </summary>
        public int? Limite { get; set; } = 50;

        /// <summary>
        /// Número de registros a saltar (paginación)
        /// </summary>
        public int? Offset { get; set; } = 0;

        /// <summary>
        /// Campo por el que ordenar (Id, Likes, Mensaje)
        /// </summary>
        public string? OrdenarPor { get; set; } = "Id";

        /// <summary>
        /// Dirección del ordenamiento (ASC o DESC)
        /// </summary>
        public string? Direccion { get; set; } = "DESC";
    }
}
