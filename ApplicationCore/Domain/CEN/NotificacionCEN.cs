using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class NotificacionCEN
    {
        private readonly INotificacionRepository _repo;
        private readonly IUnitOfWork _uow;

        public NotificacionCEN(INotificacionRepository repo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Notificacion Crear(Usuario receptor, string mensaje)
        {
            if (receptor == null)
                throw new InvalidOperationException("El receptor es requerido");

            if (string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException("El mensaje es requerido");

            if (receptor.Id <= 0)
                throw new InvalidOperationException("El ID del receptor es inválido");

            var notificacion = new Notificacion
            {
                Receptor = receptor,
                ReceptorId = receptor.Id,
                Mensaje = mensaje,
                Likes = 0
            };

            _repo.New(notificacion);
            _uow.SaveChanges();

            return notificacion;
        }

        public void Modificar(long id, string mensaje)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de notificación es inválido");

            if (string.IsNullOrWhiteSpace(mensaje))
                throw new InvalidOperationException("El mensaje es requerido");

            var notificacion = _repo.GetById(id);
            if (notificacion == null)
                throw new InvalidOperationException($"Notificación con ID {id} no encontrada");

            notificacion.Mensaje = mensaje;
            _repo.Modify(notificacion);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de notificación es inválido");

            var notificacion = _repo.GetById(id);
            if (notificacion == null)
                throw new InvalidOperationException($"Notificación con ID {id} no encontrada");

            _repo.Destroy(notificacion);
            _uow.SaveChanges();
        }

        public IEnumerable<Notificacion> DameTodos() => _repo.GetAll();

        public Notificacion? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _repo.GetById(id);
        }

        public IEnumerable<Notificacion> DamePorFiltros(NotificacionReadFilter filtros)
        {
            if (filtros == null)
                return DameTodos();

            return _repo.GetByFilters(filtros);
        }
    }
}
