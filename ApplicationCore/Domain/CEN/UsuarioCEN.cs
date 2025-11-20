using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class UsuarioCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public UsuarioCEN(IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Usuario Crear(string nombre, string email, string pass, Plan tipoPlan, int likesRecibidos = 0)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre es requerido");
            
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("El email es requerido");
            
            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("La contraseña es requerida");

            // Validar que no exista otro usuario con el mismo email
            var existente = _usuarioRepo.GetByEmail(email).FirstOrDefault();
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un usuario con email {email}");

            // Crear la entidad con los datos recibidos
            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Email = email,
                Pass = pass,
                TipoPlan = tipoPlan,
                LikesRecibidos = likesRecibidos,
                LikesEnviados = 0,
                NumMatchs = 0,
                Baneado = false,
                Superlikes = 0, // Total de superlikes recibidos
                SuperlikesDisponibles = tipoPlan == Plan.Premium ? 10 : 0 // Superlikes disponibles para usar
            };

            // Persistir en la base de datos
            _usuarioRepo.New(nuevoUsuario);
            _uow.SaveChanges();

            return nuevoUsuario;
        }

        public void Modificar(long id, string nombre, string email)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre es requerido");
            
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("El email es requerido");

            // Obtener el usuario existente
            var usuario = _usuarioRepo.GetById(id);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario con ID {id} no encontrado");

            // Validar que no exista otro usuario con el nuevo email (si es diferente)
            if (!usuario.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                var existente = _usuarioRepo.GetByEmail(email).FirstOrDefault();
                if (existente != null)
                    throw new InvalidOperationException($"Ya existe un usuario con email {email}");
            }

            // Actualizar los campos permitidos
            usuario.Nombre = nombre;
            usuario.Email = email;

            // Persistir cambios
            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void DarLike(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            // Lógica de negocio: incrementar likes recibidos
            usuario.LikesRecibidos++;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void EnviarLike(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            // Incrementar likes enviados
            usuario.LikesEnviados++;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void IncrementarMatchs(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            // Incrementar número de matchs
            usuario.NumMatchs++;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void Banear(long usuarioId) //deberia comprobar si el usuario esta ya baneado
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            if (usuario.Baneado)
                throw new InvalidOperationException($"Usuario {usuarioId} ya está baneado");

            usuario.Baneado = true;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void Desbanear(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            usuario.Baneado = false;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        /// <summary>
        /// Registra que este usuario recibió un like de otro usuario
        /// Equivalente a DarLike pero con mejor semántica
        /// </summary>
        public void RecibirLike(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            usuario.LikesRecibidos++;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        /// <summary>
        /// Registra que este usuario ha conseguido un match mutuo
        /// </summary>
        public void RecibirMatch(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            usuario.NumMatchs++;

            _usuarioRepo.Modify(usuario);
            _uow.SaveChanges();
        }

        public void Eliminar(long usuarioId)
        {
            if (usuarioId <= 0)
                throw new InvalidOperationException("El ID de usuario es inválido");

            var usuario = _usuarioRepo.GetById(usuarioId);
            if (usuario == null)
                throw new InvalidOperationException($"Usuario {usuarioId} no encontrado");

            _usuarioRepo.Destroy(usuario);
            _uow.SaveChanges();
        }

        // Consultas
        public IEnumerable<Usuario> DameTodos()
        {
            return _usuarioRepo.GetAll();
        }

        public Usuario? DamePorId(long id)
        {
            if (id <= 0)
                return null;
            
            return _usuarioRepo.GetById(id);
        }

        public Usuario? DamePorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            
            return _usuarioRepo.GetByEmail(email).FirstOrDefault();
        }

        public IEnumerable<Usuario> DamePorFiltros(UsuarioReadFilter filtros)
        {
            if (filtros == null)
                return DameTodos();

            return _usuarioRepo.GetByFilters(filtros);
        }
    }
}
