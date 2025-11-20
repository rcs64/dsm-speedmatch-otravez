using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;

namespace ApplicationCore.Domain.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario, long>
    {
        // Add domain-specific synchronous queries here
        IEnumerable<Usuario> GetByEmail(string email);
        IEnumerable<Usuario> GetByFilters(UsuarioReadFilter filtros);
    }
}
