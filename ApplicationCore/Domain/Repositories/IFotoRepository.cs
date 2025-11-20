using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;

namespace ApplicationCore.Domain.Repositories
{
    public interface IFotoRepository : IRepository<Foto, long>
    {
        IEnumerable<Foto> GetByFilters(FotoReadFilter filtros);
    }
}
