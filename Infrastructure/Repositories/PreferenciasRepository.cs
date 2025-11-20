using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class PreferenciasRepository : GenericRepository<Preferencias>, IPreferenciasRepository
    {
        public PreferenciasRepository(ISession session) : base(session)
        {
        }
    }
}
