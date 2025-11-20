using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class MatchRepository : GenericRepository<Match>, IMatchRepository
    {
        public MatchRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<Match> GetByUsuario(long usuarioId)
        {
            if (usuarioId <= 0)
                return Enumerable.Empty<Match>();

            string hql = "FROM Match m WHERE m.Emisor.Id = :usuarioId OR m.Receptor.Id = :usuarioId";
            var query = _session.CreateQuery(hql);
            query.SetParameter("usuarioId", usuarioId);

            return query.List<Match>();
        }

        public IEnumerable<Match> GetByFilters(MatchReadFilter filtros)
        {
            if (filtros == null)
                return GetAll();

            // Construir query HQL dinámicamente
            string hql = "FROM Match m WHERE 1=1";
            var parameters = new Dictionary<string, object>();

            // Filtrar por emisor
            if (filtros.EmisorId.HasValue)
            {
                hql += " AND m.Emisor.Id = :emisorId";
                parameters["emisorId"] = filtros.EmisorId.Value;
            }

            // Filtrar por receptor
            if (filtros.ReceptorId.HasValue)
            {
                hql += " AND m.Receptor.Id = :receptorId";
                parameters["receptorId"] = filtros.ReceptorId.Value;
            }

            // Filtrar por like del emisor
            if (filtros.LikeEmisor.HasValue)
            {
                hql += " AND m.LikeEmisor = :likeEmisor";
                parameters["likeEmisor"] = filtros.LikeEmisor.Value;
            }

            // Filtrar por like del receptor
            if (filtros.LikeReceptor.HasValue)
            {
                hql += " AND m.LikeReceptor = :likeReceptor";
                parameters["likeReceptor"] = filtros.LikeReceptor.Value;
            }

            // Filtrar por superlike
            if (filtros.EsSuperlike.HasValue)
            {
                hql += " AND m.EsSuperlike = :esSuperlike";
                parameters["esSuperlike"] = filtros.EsSuperlike.Value;
            }

            // Filtrar por fecha desde
            if (filtros.FechaDesde.HasValue)
            {
                hql += " AND m.FechaInicio >= :fechaDesde";
                parameters["fechaDesde"] = filtros.FechaDesde.Value;
            }

            // Filtrar por fecha hasta
            if (filtros.FechaHasta.HasValue)
            {
                hql += " AND m.FechaInicio <= :fechaHasta";
                parameters["fechaHasta"] = filtros.FechaHasta.Value;
            }

            // Filtrar solo matches confirmados (ambos dieron like)
            if (filtros.SoloConfirmados.HasValue && filtros.SoloConfirmados.Value)
            {
                hql += " AND m.LikeEmisor = true AND m.LikeReceptor = true";
            }

            // Ordenar por fecha de creación descendente
            hql += " ORDER BY m.FechaInicio DESC";

            // Crear la query HQL
            var query = _session.CreateQuery(hql);

            // Asignar parámetros
            foreach (var param in parameters)
            {
                query.SetParameter(param.Key, param.Value);
            }

            // Obtener resultados
            var resultados = query.List<Match>();

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
