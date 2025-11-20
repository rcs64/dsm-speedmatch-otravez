using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Notificacion> GetByFilters(NotificacionReadFilter filtros)
        {
            if (filtros == null)
                return GetAll();

            // Construir query HQL dinámicamente
            string hql = "FROM Notificacion n WHERE 1=1";
            var parameters = new Dictionary<string, object>();

            // Filtrar por receptor
            if (filtros.ReceptorId.HasValue)
            {
                hql += " AND n.Receptor.Id = :receptorId";
                parameters["receptorId"] = filtros.ReceptorId.Value;
            }

            // Filtrar por rango mínimo de likes
            if (filtros.LikesMinimo.HasValue)
            {
                hql += " AND n.Likes >= :likesMinimo";
                parameters["likesMinimo"] = filtros.LikesMinimo.Value;
            }

            // Filtrar por rango máximo de likes
            if (filtros.LikesMaximo.HasValue)
            {
                hql += " AND n.Likes <= :likesMaximo";
                parameters["likesMaximo"] = filtros.LikesMaximo.Value;
            }

            // Filtrar por contenido del mensaje (búsqueda parcial)
            if (!string.IsNullOrWhiteSpace(filtros.MensajeBusqueda))
            {
                hql += " AND n.Mensaje LIKE :mensaje";
                parameters["mensaje"] = "%" + filtros.MensajeBusqueda + "%";
            }

            // Ordenar según parámetros
            if (!string.IsNullOrWhiteSpace(filtros.OrdenarPor))
            {
                var direccion = filtros.Direccion?.ToUpper() == "DESC" ? "DESC" : "ASC";
                // Validar que el campo existe en la entidad
                var camposValidos = new[] { "Id", "Likes", "Mensaje" };
                var campo = camposValidos.Contains(filtros.OrdenarPor, StringComparer.OrdinalIgnoreCase) 
                    ? filtros.OrdenarPor 
                    : "Id";
                hql += $" ORDER BY n.{campo} {direccion}";
            }
            else
            {
                // Por defecto orden descendente por ID
                hql += " ORDER BY n.Id DESC";
            }

            // Crear la query HQL
            var query = _session.CreateQuery(hql);

            // Asignar parámetros
            foreach (var param in parameters)
            {
                query.SetParameter(param.Key, param.Value);
            }

            // Obtener resultados
            var resultados = query.List<Notificacion>();

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
    }
}
