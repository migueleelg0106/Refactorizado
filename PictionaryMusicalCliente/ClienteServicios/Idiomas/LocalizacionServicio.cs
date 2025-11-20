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
    public sealed class LocalizacionServicio : ILocalizacionServicio
    {
        private static readonly Lazy<LocalizacionServicio> _instancia =
            new(() => new LocalizacionServicio());

        private LocalizacionServicio()
        {
        }

        /// <summary>
        /// Obtiene la instancia unica del servicio (Singleton).
        /// </summary>
        public static LocalizacionServicio Instancia => _instancia.Value;

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
            if (cultura == null)
            {
                return;
            }

            if (CulturaActual?.Name == cultura.Name)
            {
                return;
            }

            CulturaActual = cultura;
            Lang.Culture = cultura;
            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;

            if (Settings.Default.idiomaCodigo != cultura.Name)
            {
                Settings.Default.idiomaCodigo = cultura.Name;
                Settings.Default.Save();
            }

            IdiomaActualizado?.Invoke(this, EventArgs.Empty);
        }
    }
}