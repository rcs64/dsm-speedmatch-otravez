using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;

namespace ApplicationCore.Domain.Repositories
{
    public interface IMatchRepository : IRepository<Match, long>
    {
        IEnumerable<Match> GetByUsuario(long usuarioId);
        IEnumerable<Match> GetByFilters(MatchReadFilter filtros);
    }
}
