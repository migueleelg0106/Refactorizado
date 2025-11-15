using System;
using System.ComponentModel;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;

namespace PictionaryMusicalCliente.Utilidades.Idiomas
{
    public class LocalizacionContexto : INotifyPropertyChanged
    {
        public LocalizacionContexto()
            : this(LocalizacionServicio.Instancia)
        {
        }

        public LocalizacionContexto(ILocalizacionServicio localizacionServicio)
        {
            if (localizacionServicio == null)
            {
                throw new ArgumentNullException(nameof(localizacionServicio));
            }

            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                localizacionServicio,
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
