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
        private readonly ILocalizacionServicio _localizacionServicio;

        public LocalizacionContexto()
            : this(LocalizacionServicio.Instancia)
        {
        }

        public LocalizacionContexto(ILocalizacionServicio localizacionServicio)
        {
            _localizacionServicio = localizacionServicio ?? throw new ArgumentNullException(nameof(localizacionServicio));
            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                _localizacionServicio,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServicioEnIdiomaActualizado);
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

        private void LocalizacionServicioEnIdiomaActualizado(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}
