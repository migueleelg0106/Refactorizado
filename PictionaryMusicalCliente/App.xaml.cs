using System.Windows;
using System.Globalization;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();

            var localizacionServicio = LocalizacionServicio.Instancia;
            string codigoIdioma = Settings.Default.idiomaCodigo;

            try
            {
                localizacionServicio.EstablecerIdioma(codigoIdioma);
            }
            catch (CultureNotFoundException)
            {
                localizacionServicio.EstablecerCultura(CultureInfo.CurrentUICulture);
            }

            base.OnStartup(e);
        }
    }
}
