using System;
using System.ComponentModel;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Idiomas;

namespace PictionaryMusicalCliente.Utilidades.Idiomas
{
    public class LocalizacionContexto : INotifyPropertyChanged
    {
        private readonly ILocalizacionServicio _localizacionService;

        public LocalizacionContexto()
            : this(LocalizacionServicio.Instancia)
        {
        }

        public LocalizacionContexto(ILocalizacionServicio localizacionService)
        {
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));
            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                _localizacionService,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServiceOnIdiomaActualizado);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string clave]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(clave))
                {
                    return string.Empty;
                }

                return Lang.ResourceManager.GetString(clave, Lang.Culture) ?? string.Empty;
            }
        }

        private void LocalizacionServiceOnIdiomaActualizado(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}
