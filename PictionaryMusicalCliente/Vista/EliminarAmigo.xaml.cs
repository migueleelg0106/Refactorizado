using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        public EliminarAmigo(string nombreAmigo, Action<string> amigoEliminado = null)
        {
            if (string.IsNullOrWhiteSpace(nombreAmigo))
            {
                throw new ArgumentException("El nombre del amigo es obligatorio.", nameof(nombreAmigo));
            }

            InitializeComponent();

            IAmigosService amigosService = new AmigosService();

            var vistaModelo = new EliminarAmigoVistaModelo(nombreAmigo, amigosService)
            {
                CerrarAccion = Close,
                AmigoEliminado = amigoEliminado
            };

            vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = vistaModelo;
        }
    }
}
