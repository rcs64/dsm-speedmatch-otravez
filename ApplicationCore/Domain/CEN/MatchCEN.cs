using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.DTOs;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MatchCEN
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IUnitOfWork _uow;

        public MatchCEN(IMatchRepository matchRepo, IUsuarioRepository usuarioRepo, IUnitOfWork uow)
        {
            _matchRepo = matchRepo ?? throw new ArgumentNullException(nameof(matchRepo));
            _usuarioRepo = usuarioRepo ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Crea un match simple (like de un usuario)
        /// </summary>
        public Match Crear(Usuario emisor, Usuario receptor, bool likeEmisor)
        {
            if (emisor == null)
                throw new InvalidOperationException("El emisor es requerido");

            if (receptor == null)
                throw new InvalidOperationException("El receptor es requerido");

            if (emisor.Id <= 0 || receptor.Id <= 0)
                throw new InvalidOperationException("Los IDs de los usuarios son inválidos");

            var match = new Match
            {
                Emisor = emisor,
                Receptor = receptor,
                EmisorId = emisor.Id,
                ReceptorId = receptor.Id,
                LikeEmisor = likeEmisor,
                LikeReceptor = false,
                FechaLikeEmisor = DateTime.Now
            };

            _matchRepo.New(match);
            _uow.SaveChanges();

            return match;
        }

        /// <summary>
        /// Crea un Match MUTUO transaccional verificando que ambos usuarios se dieron like
        /// Incrementa contadores de ambos usuarios de forma atómica
        /// </summary>
        public Match CrearMatchMutuo(long emisorId, long receptorId)
        {
            // Validaciones iniciales
            if (emisorId <= 0 || receptorId <= 0)
                throw new InvalidOperationException("Los IDs de los usuarios son inválidos");

            if (emisorId == receptorId)
                throw new InvalidOperationException("Un usuario no puede hacer match consigo mismo");

            // Obtener ambos usuarios
            var emisor = _usuarioRepo.GetById(emisorId);
            if (emisor == null)
                throw new InvalidOperationException($"Usuario emisor {emisorId} no encontrado");

            var receptor = _usuarioRepo.GetById(receptorId);
            if (receptor == null)
                throw new InvalidOperationException($"Usuario receptor {receptorId} no encontrado");

            // Verificar que ambos usuarios no estén baneados
            if (emisor.Baneado)
                throw new InvalidOperationException($"El usuario emisor {emisorId} está baneado");

            if (receptor.Baneado)
                throw new InvalidOperationException($"El usuario receptor {receptorId} está baneado");

            // Buscar si ya existe un match entre estos dos usuarios
            var matchExistente = _matchRepo.GetByUsuario(emisorId)
                .FirstOrDefault(m => 
                    (m.Emisor.Id == emisorId && m.Receptor.Id == receptorId) ||
                    (m.Emisor.Id == receptorId && m.Receptor.Id == emisorId));

            if (matchExistente != null && matchExistente.LikeEmisor && matchExistente.LikeReceptor)
                throw new InvalidOperationException("Ya existe un match mutuo entre estos usuarios");

            try
            {
                // TRANSACCION: Crear/actualizar match y incrementar contadores
                Match matchFinal;

                if (matchExistente != null)
                {
                    // Ya existe un like anterior, confirmarlo como mutuo
                    matchExistente.LikeReceptor = true;
                    matchExistente.FechaMatch = DateTime.Now;
                    _matchRepo.Modify(matchExistente);
                    matchFinal = matchExistente;
                }
                else
                {
                    // Crear nuevo match mutuo
                    matchFinal = new Match
                    {
                        Emisor = emisor,
                        Receptor = receptor,
                        EmisorId = emisorId,
                        ReceptorId = receptorId,
                        LikeEmisor = true,
                        LikeReceptor = true,
                        FechaLikeEmisor = DateTime.Now,
                        FechaMatch = DateTime.Now
                    };
                    _matchRepo.New(matchFinal);
                }

                // Incrementar contadores en ambos usuarios (solo si el match es nuevo o se completa)
                if (matchFinal.LikeEmisor && matchFinal.LikeReceptor)
                {
                    emisor.NumMatchs++;
                    receptor.NumMatchs++;

                    _usuarioRepo.Modify(emisor);
                    _usuarioRepo.Modify(receptor);
                }

                // Guardar todo de forma transaccional
                _uow.SaveChanges();

                return matchFinal;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al crear match mutuo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un match existente y realiza el incremento de contadores si se completa el match
        /// </summary>

        public void Modificar(long id, bool likeReceptor)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID del match es inválido");

            var match = _matchRepo.GetById(id);
            if (match == null)
                throw new InvalidOperationException($"Match con ID {id} no encontrado");

            var eraMatchMutuo = match.LikeEmisor && match.LikeReceptor;
            
            match.LikeReceptor = likeReceptor;
            if (likeReceptor && match.LikeEmisor)
            {
                // Ambos han dado like, es un match mutuo
                match.FechaMatch = DateTime.Now;

                // Incrementar contadores si no era mutuo antes
                if (!eraMatchMutuo)
                {
                    match.Emisor.NumMatchs++;
                    match.Receptor.NumMatchs++;
                    _usuarioRepo.Modify(match.Emisor);
                    _usuarioRepo.Modify(match.Receptor);
                }
            }

            _matchRepo.Modify(match);
            _uow.SaveChanges();
        }

        public void Eliminar(long id)
        {
            if (id <= 0)
                throw new InvalidOperationException("El ID del match es inválido");

            var match = _matchRepo.GetById(id);
            if (match == null)
                throw new InvalidOperationException($"Match con ID {id} no encontrado");

            _matchRepo.Destroy(match);
            _uow.SaveChanges();
        }

        public IEnumerable<Match> DameTodos() => _matchRepo.GetAll();

        public Match? DamePorId(long id)
        {
            if (id <= 0)
                return null;

            return _matchRepo.GetById(id);
        }

        public IEnumerable<Match> DamePorUsuario(long usuarioId)
        {
            if (usuarioId <= 0)
                return new List<Match>();

            return _matchRepo.GetByUsuario(usuarioId);
        }

        public IEnumerable<Match> DamePorFiltros(MatchReadFilter filtros)
        {
            if (filtros == null)
                return DameTodos();

            return _matchRepo.GetByFilters(filtros);
        }
    }
}
