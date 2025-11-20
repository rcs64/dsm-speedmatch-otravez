using System;

namespace ApplicationCore.Domain.EN
{
    public class Admin
    {
        public virtual long Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Pass { get; set; }
    }
}
