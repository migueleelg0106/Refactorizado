using System;

namespace Servicios.Servicios.Utilidades
{
    internal static class TokenGenerator
    {
        public static string GenerarToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
