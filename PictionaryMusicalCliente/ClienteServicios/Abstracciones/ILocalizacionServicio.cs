using System;
using System.Globalization;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de localizacion e idiomas.
    /// </summary>
    public interface ILocalizacionServicio
    {
        /// <summary>
        /// Evento que se dispara cuando el idioma se actualiza.
        /// </summary>
        event EventHandler IdiomaActualizado;

        /// <summary>
        /// Obtiene la cultura actual del cliente.
        /// </summary>
        CultureInfo CulturaActual { get; }

        /// <summary>
        /// Establece el idioma de la aplicacion.
        /// </summary>
        /// <param name="codigoIdioma">Codigo del idioma a establecer.</param>
        void EstablecerIdioma(string codigoIdioma);

        /// <summary>
        /// Establece la cultura de la aplicacion.
        /// </summary>
        /// <param name="cultura">Cultura a establecer.</param>
        void EstablecerCultura(CultureInfo cultura);
    }
}
