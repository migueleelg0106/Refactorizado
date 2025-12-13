using PictionaryMusicalCliente.VistaModelo.VentanaPrincipal;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal (Lobby) que gestiona la creacion de partidas y navegacion.
    /// </summary>
    public partial class VentanaPrincipal : Window
    {
        /// <summary>
        /// Constructor por defecto. VentanaServicio asigna el DataContext.
        /// </summary>
        public VentanaPrincipal()
        {
            InitializeComponent();
            Loaded += VentanaPrincipal_Loaded;
            Closed += VentanaPrincipal_Closed;
        }

        private async void VentanaPrincipal_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VentanaPrincipalVistaModelo vm)
            {
                App.MusicaManejador.ReproducirEnBucle("ventana_principal_musica.mp3");
                await vm.InicializarAsync().ConfigureAwait(true);
            }
        }

        private async void VentanaPrincipal_Closed(object sender, EventArgs e)
        {
            Loaded -= VentanaPrincipal_Loaded;
            Closed -= VentanaPrincipal_Closed;

            if (DataContext is VentanaPrincipalVistaModelo vm)
            {
                await vm.FinalizarAsync().ConfigureAwait(false);
            }

            App.MusicaManejador.Detener();
        }
    }
}