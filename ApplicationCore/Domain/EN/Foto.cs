using System;

namespace ApplicationCore.Domain.EN
{
    public class Foto
    {
        public virtual long Id { get; set; }
        public virtual string Url { get; set; }
        public virtual long UsuarioId { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
