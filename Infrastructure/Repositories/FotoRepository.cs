using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class FotoRepository : GenericRepository<Foto>, IFotoRepository
    {
        public FotoRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Foto> GetByFilters(FotoReadFilter filtros)
        {
            if (filtros == null)
                return GetAll();

            // Construir query HQL dinámicamente
            string hql = "FROM Foto f WHERE 1=1";
            var parameters = new Dictionary<string, object>();

            // Filtrar por usuario
            if (filtros.UsuarioId.HasValue)
            {
                hql += " AND f.Usuario.Id = :usuarioId";
                parameters["usuarioId"] = filtros.UsuarioId.Value;
            }

            // Filtrar por URL parcial
            if (!string.IsNullOrWhiteSpace(filtros.UrlBusqueda))
            {
                hql += " AND f.Url LIKE :url";
                parameters["url"] = "%" + filtros.UrlBusqueda + "%";
            }

            // Filtrar por rango de IDs
            if (filtros.IdDesde.HasValue)
            {
                hql += " AND f.Id >= :idDesde";
                parameters["idDesde"] = filtros.IdDesde.Value;
            }

            if (filtros.IdHasta.HasValue)
            {
                hql += " AND f.Id <= :idHasta";
                parameters["idHasta"] = filtros.IdHasta.Value;
            }

            // Ordenar según parámetros
            if (!string.IsNullOrWhiteSpace(filtros.OrdenarPor))
            {
                var direccion = filtros.Direccion?.ToUpper() == "DESC" ? "DESC" : "ASC";
                hql += $" ORDER BY f.{filtros.OrdenarPor} {direccion}";
            }
            else
            {
                // Por defecto orden descendente por ID (más recientes primero)
                hql += " ORDER BY f.Id DESC";
            }

            // Crear la query HQL
            var query = _session.CreateQuery(hql);

            // Asignar parámetros
            foreach (var param in parameters)
            {
                query.SetParameter(param.Key, param.Value);
            }

            // Obtener resultados
            var resultados = query.List<Foto>();

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
