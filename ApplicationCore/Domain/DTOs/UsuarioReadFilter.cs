using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.DTOs
{
    /// <summary>
    /// Filtro de lectura para búsqueda avanzada de Usuarios
    /// Permite filtrar por edad, género, ubicación y otros criterios
    /// </summary>
    public class UsuarioReadFilter
    {
        /// <summary>
        /// Edad mínima del usuario (opcional)
        /// </summary>
        public int? EdadMinima { get; set; }

        /// <summary>
        /// Edad máxima del usuario (opcional)
        /// </summary>
        public int? EdadMaxima { get; set; }

        /// <summary>
        /// Género del usuario (opcional)
        /// </summary>
        public Genero? Genero { get; set; }

        /// <summary>
        /// Latitud del punto de referencia para búsqueda por ubicación (opcional)
        /// </summary>
        public double? Latitud { get; set; }

        /// <summary>
        /// Longitud del punto de referencia para búsqueda por ubicación (opcional)
        /// </summary>
        public double? Longitud { get; set; }

        /// <summary>
        /// Radio de búsqueda en metros desde el punto de referencia (opcional)
        /// Por defecto 5000 metros (5 km)
        /// </summary>
        public double? RadioMetros { get; set; } = 5000;

        /// <summary>
        /// Filtrar solo usuarios Premium (opcional)
        /// </summary>
        public bool? SoloPremium { get; set; }

        /// <summary>
        /// Filtrar solo usuarios no baneados (opcional, por defecto true)
        /// </summary>
        public bool? SoloNoBaneados { get; set; } = true;

        /// <summary>
        /// Buscar por nombre (búsqueda parcial) (opcional)
        /// </summary>
        public string? NombreBusqueda { get; set; }

        /// <summary>
        /// Número máximo de resultados (paginación)
        /// </summary>
        public int? Limite { get; set; } = 50;

        /// <summary>
        /// Número de registros a saltar (paginación)
        /// </summary>
        public int? Offset { get; set; } = 0;

        /// <summary>
        /// Campo por el que ordenar (FechaNacimiento, Nombre, NumMatchs)
        /// </summary>
        public string? OrdenarPor { get; set; } = "NumMatchs";

        /// <summary>
        /// Dirección del ordenamiento (ASC o DESC)
        /// </summary>
        public string? Direccion { get; set; } = "DESC";
    }
}
