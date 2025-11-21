using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PictionaryMusicalCliente.Properties.Langs;
using log4net;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Genera nombres aleatorios para los invitados respetando la cultura solicitada.
    /// </summary>
    public static class NombreInvitadoGenerador
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object _sync = new();
        private static readonly Random _random = new();

        /// <summary>
        /// Obtiene un nombre de invitado aleatorio acorde a la cultura proporcionada.
        /// </summary>
        /// <param name="cultura">Cultura a utilizar. Si es null se usa la actual.</param>
        /// <param name="nombresExcluidos">Lista de nombres que no deben repetirse.</param>
        /// <returns>Nombre de invitado localizado.</returns>
        public static string Generar(
            CultureInfo cultura,
            IEnumerable<string> nombresExcluidos = null)
        {
            CultureInfo culturaEfectiva = cultura ?? CultureInfo.CurrentUICulture;

            string opciones = ObtenerOpciones(culturaEfectiva);

            if (string.IsNullOrWhiteSpace(opciones))
            {
                _logger.ErrorFormat("No se encontraron nombres de invitados para cultura: {0}",
                    culturaEfectiva);
                return null;
            }

            var nombres = opciones
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(nombre => nombre.Trim())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .ToArray();

            if (nombres.Length == 0)
            {
                _logger.Warn("La lista de nombres parseada está vacía.");
                return null;
            }

            HashSet<string> nombresNoDisponibles = nombresExcluidos != null
                ? new HashSet<string>(
                    nombresExcluidos.Where(n => !string.IsNullOrWhiteSpace(n)),
                    StringComparer.OrdinalIgnoreCase)
                : null;

            string[] nombresDisponibles = nombresNoDisponibles == null
                ? nombres
                : nombres.Where(nombre => !nombresNoDisponibles.Contains(nombre)).ToArray();

            if (nombresDisponibles.Length == 0)
            {
                _logger.Info("Todos los nombres disponibles ya han sido utilizados.");
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
                _logger.WarnFormat("Falta recurso 'invitadoNombres' en {0}, usando Invariant.",
                    cultura);
                opciones = Lang.ResourceManager.GetString(
                    "invitadoNombres",
                    CultureInfo.InvariantCulture);
            }

            return opciones;
        }
    }
}