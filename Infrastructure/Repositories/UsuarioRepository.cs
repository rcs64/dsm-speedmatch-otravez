using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.DTOs;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Usuario> GetByEmail(string email) // cambiar a hql
        {
            if (string.IsNullOrWhiteSpace(email))
                return Enumerable.Empty<Usuario>();

            return _session.CreateQuery("FROM Usuario u WHERE u.Email = :email")
                .SetParameter("email", email)
                .List<Usuario>();
                //.Where(u => u.Email == email)
                //.ToList(); //cambiar a hql para todos los filtros
        }

        public IEnumerable<Usuario> GetByFilters(UsuarioReadFilter filtros)
        {
            if (filtros == null)
                return GetAll();

            // Construir query HQL dinámicamente
            string hql = "FROM Usuario u WHERE 1=1";
            var parameters = new Dictionary<string, object>();

            // Filtrar por edad mínima
            if (filtros.EdadMinima.HasValue)
            {
                var fechaMaxNacimiento = DateTime.Now.AddYears(-filtros.EdadMinima.Value);
                hql += " AND u.FechaNacimiento <= :fechaMaxNacimiento";
                parameters["fechaMaxNacimiento"] = fechaMaxNacimiento;
            }

            // Filtrar por edad máxima
            if (filtros.EdadMaxima.HasValue)
            {
                var fechaMinNacimiento = DateTime.Now.AddYears(-filtros.EdadMaxima.Value - 1);
                hql += " AND u.FechaNacimiento >= :fechaMinNacimiento";
                parameters["fechaMinNacimiento"] = fechaMinNacimiento;
            }

            // Filtrar por género
            if (filtros.Genero.HasValue)
            {
                hql += " AND u.Genero = :genero";
                parameters["genero"] = filtros.Genero.Value;
            }

            // Filtrar por plan Premium
            if (filtros.SoloPremium.HasValue && filtros.SoloPremium.Value)
            {
                hql += " AND u.TipoPlan = :tipoPlan";
                parameters["tipoPlan"] = ApplicationCore.Domain.Enums.Plan.Premium;
            }

            // Filtrar solo no baneados
            if (filtros.SoloNoBaneados.HasValue && filtros.SoloNoBaneados.Value)
            {
                hql += " AND u.Baneado = false";
            }

            // Buscar por nombre (búsqueda parcial)
            if (!string.IsNullOrWhiteSpace(filtros.NombreBusqueda))
            {
                hql += " AND u.Nombre LIKE :nombre";
                parameters["nombre"] = "%" + filtros.NombreBusqueda + "%";
            }

            // Ordenar según parámetros
            if (!string.IsNullOrWhiteSpace(filtros.OrdenarPor))
            {
                var direccion = filtros.Direccion?.ToUpper() == "DESC" ? "DESC" : "ASC";
                hql += $" ORDER BY u.{filtros.OrdenarPor} {direccion}";
            }
            else
            {
                // Por defecto orden descendente por NumMatchs
                hql += " ORDER BY u.NumMatchs DESC";
            }

            // Crear la query HQL
            var query = _session.CreateQuery(hql);

            // Asignar parámetros
            foreach (var param in parameters)
            {
                query.SetParameter(param.Key, param.Value);
            }

            // Obtener resultados
            var resultados = query.List<Usuario>();

            // Filtrar por ubicación (radio en metros) usando fórmula de Haversine
            // Esto se hace en memoria porque la fórmula es compleja para HQL
            if (filtros.Latitud.HasValue && filtros.Longitud.HasValue && filtros.RadioMetros.HasValue)
            {
                resultados = resultados
                    .Where(u => u.Ubicacion != null && 
                               u.Ubicacion.Any() &&
                               CalcularDistanciaEnMetros(
                                   filtros.Latitud.Value, 
                                   filtros.Longitud.Value,
                                   u.Ubicacion.First().Lat, 
                                   u.Ubicacion.First().Lon) <= filtros.RadioMetros.Value)
                    .ToList();
            }

            // Aplicar paginación
            if (filtros.Offset.HasValue && filtros.Offset.Value > 0)
            {
                resultados = resultados.Skip(filtros.Offset.Value).ToList();
            }

            if (filtros.Limite.HasValue && filtros.Limite.Value > 0)
            {
                resultados = resultados.Take(filtros.Limite.Value).ToList();
            }

            return resultados;
        }

        /// <summary>
        /// Calcula la distancia entre dos puntos geográficos usando la fórmula de Haversine
        /// </summary>
        /// <returns>Distancia en metros</returns>
        private double CalcularDistanciaEnMetros(double lat1, double lon1, double lat2, double lon2)
        {
            const double radioTierraMetros = 6371000; // Radio de la Tierra en metros

            var lat1Rad = GradosARadianes(lat1);
            var lat2Rad = GradosARadianes(lat2);
            var deltaLatRad = GradosARadianes(lat2 - lat1);
            var deltaLonRad = GradosARadianes(lon2 - lon1);

            var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radioTierraMetros * c;
        }

        private double GradosARadianes(double grados)
        {
            return grados * Math.PI / 180.0;
        }
    }
}
