using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;

namespace ApplicationCore.Domain.Repositories
{
    public interface INotificacionRepository : IRepository<Notificacion, long>
    {
        IEnumerable<Notificacion> GetByFilters(NotificacionReadFilter filtros);
    }
}
