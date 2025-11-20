using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Preferencias
    {
        public virtual long Id { get; set; }
        public virtual OrientacionSexual Orientacion { get; set; }
        public virtual string OrientacionOtro { get; set; }
        public virtual bool OrientacionMostrar { get; set; }
        public virtual PrefConocer Conocer { get; set; }
        public virtual string ConocerOtro { get; set; }

        // Reverse relation
        public virtual ISet<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
    }
}
