using System;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Clase utilitaria para generar tokens unicos de sesion.
    /// Genera tokens seguros usando GUID en formato hexadecimal.
    /// </summary>
    internal static class TokenGenerador
    {
        /// <summary>
        /// Genera un token unico de sesion basado en GUID.
        /// El token generado es una cadena hexadecimal de 32 caracteres sin guiones.
        /// </summary>
        /// <returns>Token unico como cadena hexadecimal.</returns>
        public static string GenerarToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
