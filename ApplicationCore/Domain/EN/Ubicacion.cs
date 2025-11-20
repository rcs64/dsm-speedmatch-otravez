using System;

namespace ApplicationCore.Domain.EN
{
    public class Ubicacion
    {
        public virtual long Id { get; set; }
        public virtual double Lat { get; set; }
        public virtual double Lon { get; set; }
        public virtual long UsuarioId { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
