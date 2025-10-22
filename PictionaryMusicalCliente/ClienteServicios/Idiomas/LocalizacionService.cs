using System;
using System.Globalization;
using System.Threading;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;

namespace PictionaryMusicalCliente.Servicios.Idiomas
{
    public sealed class LocalizacionService : ILocalizacionService
    {
        private static readonly Lazy<LocalizacionService> _instancia =
            new(() => new LocalizacionService());

        private LocalizacionService()
        {
        }

        public static LocalizacionService Instancia => _instancia.Value;

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
