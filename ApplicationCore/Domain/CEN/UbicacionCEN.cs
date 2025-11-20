using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class UbicacionCEN
    {
        private readonly IUbicacionRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public UbicacionCEN(IUbicacionRepository repo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Ubicacion Crear(double lat, double lon, long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            if (lat < -90 || lat > 90)
                throw new InvalidOperationException("La latitud debe estar entre -90 y 90");

            if (lon < -180 || lon > 180)
                throw new InvalidOperationException("La longitud debe estar entre -180 y 180");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario con ID {usuarioId} no encontrado");

            var ubicacion = new Ubicacion
            {
                Lat = lat,
                Lon = lon,
                Usuario = usuario,
                UsuarioId = usuarioId
            };

            _repo.New(ubicacion);
            _uow.SaveChanges();

            return ubicacion;
        }

        public void Modificar(long id, double lat, double lon)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de ubicación es inválido");

            if (lat < -90 || lat > 90)
                throw new InvalidOperationException("La latitud debe estar entre -90 y 90");

            if (lon < -180 || lon > 180)
                throw new InvalidOperationException("La longitud debe estar entre -180 y 180");

            var ubicacion = _repo.GetById(id);
            if (ubicacion == null)
                throw new InvalidOperationException($"Ubicación con ID {id} no encontrada");

            ubicacion.Lat = lat;
            ubicacion.Lon = lon;

            _repo.Modify(ubicacion);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de ubicación es inválido");

            var ubicacion = _repo.GetById(id);
            if (ubicacion == null)
                throw new InvalidOperationException($"Ubicación con ID {id} no encontrada");

            _repo.Destroy(ubicacion);
            _uow.SaveChanges();
        }

        public IEnumerable<Ubicacion> DameTodos() => _repo.GetAll();

        public Ubicacion? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _repo.GetById(id);
        }
    }
}
