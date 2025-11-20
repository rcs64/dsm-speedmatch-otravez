using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class FotoCEN
    {
        private readonly IFotoRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public FotoCEN(IFotoRepository repo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Foto Crear(string url, long usuarioId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("La URL es requerida");

            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario con ID {usuarioId} no encontrado");

            var foto = new Foto
            {
                Url = url,
                Usuario = usuario,
                UsuarioId = usuarioId
            };

            _repo.New(foto);
            _uow.SaveChanges();

            return foto;
        }

        public void Modificar(long id, string url)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de foto es inválido");

            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("La URL es requerida");

            var foto = _repo.GetById(id);
            if (foto == null)
                throw new InvalidOperationException($"Foto con ID {id} no encontrada");

            foto.Url = url;
            _repo.Modify(foto);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de foto es inválido");

            var foto = _repo.GetById(id);
            if (foto == null)
                throw new InvalidOperationException($"Foto con ID {id} no encontrada");

            _repo.Destroy(foto);
            _uow.SaveChanges();
        }

        public IEnumerable<Foto> DameTodos() => _repo.GetAll();

        public Foto? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _repo.GetById(id);
        }

        public IEnumerable<Foto> DamePorFiltros(FotoReadFilter filtros)
        {
            if (filtros == null)
                return DameTodos();

            return _repo.GetByFilters(filtros);
        }
    }
}
