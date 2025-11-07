using System;
using System.Globalization;
using System.Threading;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.ClienteServicios.Idiomas
{
    public sealed class LocalizacionServicio : ILocalizacionServicio
    {
        private static readonly Lazy<LocalizacionServicio> _instancia =
            new(() => new LocalizacionServicio());

        private LocalizacionServicio()
        {
        }

        public static LocalizacionServicio Instancia => _instancia.Value;

        public event EventHandler IdiomaActualizado;

        public CultureInfo CulturaActual { get; private set; }

        public void EstablecerIdioma(string codigoIdioma)
        {
            if (string.IsNullOrWhiteSpace(codigoIdioma))
            {
                return;
            }

            EstablecerCultura(new CultureInfo(codigoIdioma));
        }

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
