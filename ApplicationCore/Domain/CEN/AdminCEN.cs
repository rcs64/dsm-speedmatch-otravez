using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class AdminCEN
    {
        private readonly IAdminRepository _repo;
        private readonly IUnitOfWork _uow;

        public AdminCEN(IAdminRepository repo, IUnitOfWork uow)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Admin Crear(string email, string pass)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("El email es requerido");

            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("La contraseña es requerida");

            var admin = new Admin
            {
                Email = email,
                Pass = pass
            };

            _repo.New(admin);
            _uow.SaveChanges();

            return admin;
        }

        public void Modificar(long id, string email, string pass)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de admin es inválido");

            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("El email es requerido");

            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("La contraseña es requerida");

            var admin = _repo.GetById(id);
            if (admin == null)
                throw new InvalidOperationException($"Admin con ID {id} no encontrado");

            admin.Email = email;
            admin.Pass = pass;

            _repo.Modify(admin);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de admin es inválido");

            var admin = _repo.GetById(id);
            if (admin == null)
                throw new InvalidOperationException($"Admin con ID {id} no encontrado");

            _repo.Destroy(admin);
            _uow.SaveChanges();
        }

        public IEnumerable<Admin> DameTodos() => _repo.GetAll();

        public Admin? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _repo.GetById(id);
        }
    }
}

// inicializar un pedido con estado "pendiente" ya sin tener que pasarselo por ejemplo
// asignar cosas sin tener que pasarselas por parametro es una modificacion
// tambien pueden ser precondiciones para modeificar o borrar objetos

// custom es cambiar un atributo o aumentar el estado

// cp es crear un y cambiar el estado de otro (dos objetos a la vez)

// filtro por usuario para ver pedidos de un usuario concreto
// todo lo propio de un perfil debe estar filtrado