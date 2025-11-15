using System;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    internal static class CodigoVerificacionGenerador
    {
        private static readonly Random _random = new Random();

        public static string GenerarCodigo(int longitud = 6)
        {
            if (longitud <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(longitud));
            }

            lock (_random)
            {
                int limiteSuperior = (int)Math.Pow(10, longitud) - 1;
                int limiteInferior = (int)Math.Pow(10, longitud - 1);
                int numero = _random.Next(limiteInferior, limiteSuperior);
                return numero.ToString();
            }
        }
    }
}
