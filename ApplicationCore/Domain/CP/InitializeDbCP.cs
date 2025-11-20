using System;
using ApplicationCore.Domain.CEN;

namespace ApplicationCore.Domain.CP
{
    public class InitializeDbCP
    {
        private readonly UsuarioCEN _usuarioCEN;
        private readonly PreferenciasCEN _preferenciasCEN;

        public InitializeDbCP(UsuarioCEN usuarioCEN, PreferenciasCEN preferenciasCEN)
        {
            _usuarioCEN = usuarioCEN;
            _preferenciasCEN = preferenciasCEN;
        }

        public void SeedMinimal()
        {
            // Implement idempotent seed using CENs and IUnitOfWork
            throw new NotImplementedException();
        }
    }
}
