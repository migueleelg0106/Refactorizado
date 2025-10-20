using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        public EliminarAmigo(string nombreAmigo)
            : this(nombreAmigo, AmigosService.Instancia)
        {
        }

        public EliminarAmigo(string nombreAmigo, IAmigosService amigosService)
        {
            InitializeComponent();

            var vistaModelo = new EliminarAmigoVistaModelo(nombreAmigo, amigosService)
            {
                CerrarAccion = Close,
                MostrarMensaje = AvisoHelper.Mostrar
            };

            DataContext = vistaModelo;
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
