using System;
using System.Globalization;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Administra la configuracion regional y de idioma de la aplicacion en tiempo de ejecucion.
    /// </summary>
    public interface ILocalizacionServicio
    {
        /// <summary>
        /// Se dispara cuando el idioma de la aplicacion ha cambiado exitosamente.
        /// </summary>
        event EventHandler IdiomaActualizado;

        /// <summary>
        /// Obtiene la cultura configurada actualmente para el formato de recursos y textos.
        /// </summary>
        CultureInfo CulturaActual { get; }

        /// <summary>
        /// Aplica un nuevo idioma a la interfaz basandose en el codigo ISO proporcionado.
        /// </summary>
        /// <param name="codigoIdioma">El codigo del idioma (ej. "es-MX", "en-US").</param>
        void EstablecerIdioma(string codigoIdioma);

        /// <summary>
        /// Asigna directamente un objeto de cultura especifica al hilo actual.
        /// </summary>
        /// <param name="cultura">La informacion de cultura a establecer.</param>
        void EstablecerCultura(CultureInfo cultura);
    }
}