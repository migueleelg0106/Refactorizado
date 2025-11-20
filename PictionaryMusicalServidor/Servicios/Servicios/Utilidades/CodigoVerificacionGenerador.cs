using System;

namespace PictionaryMusicalServidor.Servicios.Servicios.Utilidades
{
    /// <summary>
    /// Clase utilitaria para generar codigos de verificacion numericos aleatorios.
    /// Genera codigos seguros usando Random con sincronizacion para uso concurrente.
    /// </summary>
    internal static class CodigoVerificacionGenerador
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Genera un codigo de verificacion numerico aleatorio de la longitud especificada.
        /// El codigo generado no contiene ceros iniciales y es thread-safe.
        /// </summary>
        /// <param name="longitud">Longitud del codigo a generar (por defecto 6 digitos).</param>
        /// <returns>Codigo de verificacion numerico como cadena.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Se lanza si longitud es menor o igual a 0.</exception>
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
