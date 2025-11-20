using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Notificacion
    {
        public virtual long Id { get; set; }
        public virtual string Mensaje { get; set; }
        public virtual int Likes { get; set; }
        public virtual long ReceptorId { get; set; }

        public virtual Usuario Receptor { get; set; }
        public virtual ISet<Match> Matches { get; set; } = new HashSet<Match>();
    }
}
