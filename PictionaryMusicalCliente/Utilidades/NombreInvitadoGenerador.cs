using log4net;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PictionaryMusicalCliente.Utilidades
{
    /// <summary>
    /// Genera nombres aleatorios para los invitados respetando la cultura solicitada.
    /// </summary>
    public class NombreInvitadoGenerador : INombreInvitadoGenerador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NombreInvitadoGenerador));
        private readonly object _sync = new();
        private readonly Random _random = new();

        /// <inheritdoc />
        public string Generar(CultureInfo cultura, IEnumerable<string> nombresExcluidos = null)
        {
            CultureInfo culturaEfectiva = cultura ?? CultureInfo.CurrentUICulture;
            string recursoNombres = ObtenerRecursoNombres(culturaEfectiva);

            if (string.IsNullOrWhiteSpace(recursoNombres))
            {
                _logger.ErrorFormat("No se encontraron nombres para cultura: {0}", culturaEfectiva);
                return null;
            }

            string[] todosLosNombres = ParsearNombres(recursoNombres);
            if (todosLosNombres.Length == 0)
            {
                _logger.Warn("La lista de nombres parseada esta vacia.");
                return null;
            }

            string[] nombresDisponibles = FiltrarNombresDisponibles(
                todosLosNombres,
                nombresExcluidos);

            if (nombresDisponibles.Length == 0)
            {
                _logger.Info("Todos los nombres disponibles ya han sido utilizados.");
                return null;
            }

            return SeleccionarAleatorio(nombresDisponibles);
        }

        private static string ObtenerRecursoNombres(CultureInfo cultura)
        {
            string opciones = Lang.ResourceManager.GetString("invitadoNombres", cultura);

            if (string.IsNullOrWhiteSpace(opciones) && !cultura.Equals(CultureInfo.InvariantCulture))
            {
                _logger.WarnFormat("Falta recurso 'invitadoNombres' en {0}, usando Invariant.",
                    cultura);
                return Lang.ResourceManager.GetString("invitadoNombres", CultureInfo.InvariantCulture);
            }

            return opciones;
        }

        private static string[] ParsearNombres(string cadenaOpciones)
        {
            return cadenaOpciones
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(nombre => nombre.Trim())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .ToArray();
        }

        private static string[] FiltrarNombresDisponibles(
            string[] todos,
            IEnumerable<string> excluidos)
        {
            if (excluidos == null) return todos;

            var setExcluidos = new HashSet<string>(
                excluidos.Where(n => !string.IsNullOrWhiteSpace(n)),
                StringComparer.OrdinalIgnoreCase);

            return todos.Where(n => !setExcluidos.Contains(n)).ToArray();
        }

        private string SeleccionarAleatorio(string[] opciones)
        {
            lock (_sync)
            {
                int indice = _random.Next(opciones.Length);
                return opciones[indice];
            }
        }
    }
}