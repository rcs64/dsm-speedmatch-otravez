using System;

namespace ApplicationCore.Domain.EN
{
    public class Match
    {
        public virtual long Id { get; set; }
        public virtual bool LikeEmisor { get; set; }
        public virtual bool LikeReceptor { get; set; }
        public virtual bool EsSuperlike { get; set; }
        public virtual DateTime FechaInicio { get; set; }
        public virtual DateTime? FechaLikeEmisor { get; set; }
        public virtual DateTime? FechaMatch { get; set; }

        public virtual long EmisorId { get; set; }
        public virtual long ReceptorId { get; set; }
        public virtual long? NotificacionId { get; set; }

        public virtual Usuario Emisor { get; set; }
        public virtual Usuario Receptor { get; set; }
        public virtual Notificacion? Notificacion { get; set; }
    }
}
