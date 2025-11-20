using System;
using System.ComponentModel;
using System.Windows;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;

namespace PictionaryMusicalCliente.Utilidades.Idiomas
{
    /// <summary>
    /// Provee un contexto de enlace de datos para recursos de idioma en XAML.
    /// Permite la actualizacion dinamica de cadenas cuando cambia la cultura.
    /// </summary>
    public class LocalizacionContexto : INotifyPropertyChanged
    {
        /// <summary>
        /// Inicializa una nueva instancia usando el servicio de localizacion predeterminado.
        /// </summary>
        public LocalizacionContexto()
            : this(LocalizacionServicio.Instancia)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia con un servicio de localizacion especifico.
        /// </summary>
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

        /// <summary>
        /// Evento que notifica cambios en las propiedades enlazadas (cadenas de texto).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Obtiene el recurso de cadena localizado para la clave especificada.
        /// </summary>
        /// <param name="clave">Clave del recurso en el archivo de recursos.</param>
        /// <returns>Texto localizado o cadena vacia si no se encuentra.</returns>
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