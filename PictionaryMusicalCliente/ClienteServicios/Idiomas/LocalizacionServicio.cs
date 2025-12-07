using System;
using System.Globalization;
using System.Threading;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios.Idiomas
{
    /// <summary>
    /// Administra el cambio y persistencia del idioma de la aplicacion.
    /// </summary>
    public class LocalizacionServicio : ILocalizacionServicio
    {
        /// <summary>
        /// Inicializa una nueva instancia del servicio.
        /// </summary>
        public LocalizacionServicio()
        {
        }

        /// <summary>
        /// Evento que notifica cuando se ha cambiado el idioma.
        /// </summary>
        public event EventHandler IdiomaActualizado;

        /// <summary>
        /// Obtiene la cultura configurada actualmente.
        /// </summary>
        public CultureInfo CulturaActual { get; private set; }

        /// <summary>
        /// Establece el idioma usando un codigo de cultura (ej. "es-MX").
        /// </summary>
        public void EstablecerIdioma(string codigoIdioma)
        {
            if (string.IsNullOrWhiteSpace(codigoIdioma))
            {
                return;
            }

            EstablecerCultura(new CultureInfo(codigoIdioma));
        }

        /// <summary>
        /// Aplica la cultura especificada al hilo actual y guarda la preferencia.
        /// </summary>
        public void EstablecerCultura(CultureInfo cultura)
        {
            if (cultura == null || CulturaActual?.Name == cultura.Name)
            {
                return;
            }

            ActualizarEstadoGlobal(cultura);
            PersistirPreferencia(cultura);
            NotificarCambio();
        }

        private void ActualizarEstadoGlobal(CultureInfo cultura)
        {
            CulturaActual = cultura;
            Lang.Culture = cultura;

            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;

            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;
        }

        private void PersistirPreferencia(CultureInfo cultura)
        {
            if (Settings.Default.idiomaCodigo != cultura.Name)
            {
                Settings.Default.idiomaCodigo = cultura.Name;
                Settings.Default.Save();
            }
        }

        private void NotificarCambio()
        {
            IdiomaActualizado?.Invoke(this, EventArgs.Empty);
        }
    }
}