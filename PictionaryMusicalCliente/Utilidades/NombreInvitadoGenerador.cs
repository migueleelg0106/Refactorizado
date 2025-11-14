using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Genera nombres aleatorios para los invitados respetando la cultura solicitada.
    /// </summary>
    public static class NombreInvitadoGenerador
    {
        private static readonly object _sync = new();
        private static readonly Random _random = new();

        /// <summary>
        /// Obtiene un nombre de invitado aleatorio acorde a la cultura proporcionada.
        /// </summary>
        /// <param name="cultura">Cultura a utilizar para localizar el nombre. Si es <c>null</c> se utilizará la cultura actual.</param>
        /// <returns>Nombre de invitado localizado.</returns>
        public static string Generar(CultureInfo cultura, IEnumerable<string> nombresExcluidos = null)
        {
            CultureInfo culturaEfectiva = cultura ?? CultureInfo.CurrentUICulture;

            string opciones = ObtenerOpciones(culturaEfectiva);

            var nombres = opciones
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(nombre => nombre.Trim())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .ToArray();

            if (nombres.Length == 0)
            {
                return null;
            }

            HashSet<string> nombresNoDisponibles = nombresExcluidos != null
                ? new HashSet<string>(nombresExcluidos.Where(nombre => !string.IsNullOrWhiteSpace(nombre)),
                    StringComparer.OrdinalIgnoreCase)
                : null;

            string[] nombresDisponibles = nombresNoDisponibles == null
                ? nombres
                : nombres.Where(nombre => !nombresNoDisponibles.Contains(nombre)).ToArray();

            if (nombresDisponibles.Length == 0)
            {
                return null;
            }

            lock (_sync)
            {
                int indice = _random.Next(nombresDisponibles.Length);
                return nombresDisponibles[indice];
            }
        }

        private static string ObtenerOpciones(CultureInfo cultura)
        {
            string opciones = Lang.ResourceManager.GetString("invitadoNombres", cultura);

            if (string.IsNullOrWhiteSpace(opciones) && cultura != CultureInfo.InvariantCulture)
            {
                opciones = Lang.ResourceManager.GetString("invitadoNombres", CultureInfo.InvariantCulture);
            }

            return opciones;
        }
    }
}

