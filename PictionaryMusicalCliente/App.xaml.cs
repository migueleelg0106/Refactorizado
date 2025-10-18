using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using PictionaryMusicalCliente.Properties;
using PictionaryMusicalCliente.Servicios.Idiomas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var localizacionService = LocalizacionService.Instancia;
            string codigoIdioma = Settings.Default.idiomaCodigo;

            try
            {
                localizacionService.EstablecerIdioma(codigoIdioma);
            }
            catch (CultureNotFoundException)
            {
                localizacionService.EstablecerCultura(CultureInfo.CurrentUICulture);
            }

            base.OnStartup(e);
        }
    }
}
