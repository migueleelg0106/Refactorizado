using System;

namespace Servicios.Servicios.Utilidades
{
    internal static class CodigoVerificacionGenerator
    {
        private static readonly Random Random = new Random();

        public static string GenerarCodigo(int longitud = 6)
        {
            if (longitud <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(longitud));
            }

            lock (Random)
            {
                int max = (int)Math.Pow(10, longitud) - 1;
                int min = (int)Math.Pow(10, longitud - 1);
                int numero = Random.Next(min, max);
                return numero.ToString();
            }
        }
    }
}
