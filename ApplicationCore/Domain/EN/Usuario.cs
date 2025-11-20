using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Usuario
    {
        public virtual long Id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Email { get; set; }
        public virtual string Pass { get; set; }
        public virtual int LikesRecibidos { get; set; }
        public virtual int LikesEnviados { get; set; }
        public virtual int NumMatchs { get; set; }
        public virtual Genero Genero { get; set; }
        public virtual string? GeneroOtro { get; set; }
        public virtual bool Baneado { get; set; }
        public virtual DateTime? FechaNacimiento { get; set; }
        public virtual DateTime? FechaUltimoLogin { get; set; }
        public virtual string? Habitos { get; set; }
        public virtual string? Comportamientos { get; set; }
        public virtual string? Hobbies { get; set; }
        public virtual string? Descripcion { get; set; }
        public virtual Plan TipoPlan { get; set; }
        public virtual int Superlikes { get; set; }
        public virtual int SuperlikesDisponibles { get; set; }

        // FK a Preferencias
        public virtual long? PreferenciasId { get; set; }

        // Relations
        public virtual ISet<Foto> Fotos { get; set; } = new HashSet<Foto>();
        public virtual ISet<Ubicacion> Ubicacion { get; set; } = new HashSet<Ubicacion>();
        public virtual Preferencias? Preferencias { get; set; }
        public virtual ISet<Match> MatchesEnviados { get; set; } = new HashSet<Match>();
        public virtual ISet<Match> MatchesRecibidos { get; set; } = new HashSet<Match>();
        public virtual ISet<Notificacion> NotificacionesRecibidas { get; set; } = new HashSet<Notificacion>();
    }
}
