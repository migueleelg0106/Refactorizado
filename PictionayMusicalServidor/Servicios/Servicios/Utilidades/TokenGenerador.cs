using System;

namespace Servicios.Servicios.Utilidades
{
    internal static class TokenGenerador
    {
        public static string GenerarToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
