using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class PreferenciasCEN
    {
        private readonly IPreferenciasRepository _repo;
        private readonly IUnitOfWork _uow;

        public PreferenciasCEN(IPreferenciasRepository repo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Preferencias Crear(OrientacionSexual orientacion, PrefConocer conocer, bool orientacionMostrar = false)
        {
            var preferencias = new Preferencias
            {
                Orientacion = orientacion,
                Conocer = conocer,
                OrientacionMostrar = orientacionMostrar
            };

            _repo.New(preferencias);
            _uow.SaveChanges();

            return preferencias;
        }

        public void Modificar(long id, OrientacionSexual orientacion, PrefConocer conocer, bool orientacionMostrar = false)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de preferencias es inválido");

            var preferencias = _repo.GetById(id);
            if (preferencias == null)
                throw new InvalidOperationException($"Preferencias con ID {id} no encontradas");

            preferencias.Orientacion = orientacion;
            preferencias.Conocer = conocer;
            preferencias.OrientacionMostrar = orientacionMostrar;

            _repo.Modify(preferencias);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de preferencias es inválido");

            var preferencias = _repo.GetById(id);
            if (preferencias == null)
                throw new InvalidOperationException($"Preferencias con ID {id} no encontradas");

            _repo.Destroy(preferencias);
            _uow.SaveChanges();
        }

        public IEnumerable<Preferencias> DameTodos() => _repo.GetAll();

        public Preferencias? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _repo.GetById(id);
        }
    }
}
