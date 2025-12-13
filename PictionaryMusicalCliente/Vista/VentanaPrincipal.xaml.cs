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
            Loaded += AlCargarVentanaPrincipal;
            Closed += AlCerrarVentanaPrincipal;
        }

        private async void AlCargarVentanaPrincipal(object sender, RoutedEventArgs e)
        {
            if (DataContext is VentanaPrincipalVistaModelo vistaModelo)
            {
                App.MusicaManejador.ReproducirEnBucle("ventana_principal_musica.mp3");
                await vistaModelo.InicializarAsync().ConfigureAwait(true);
            }
        }

        private async void AlCerrarVentanaPrincipal(object sender, EventArgs e)
        {
            Loaded -= AlCargarVentanaPrincipal;
            Closed -= AlCerrarVentanaPrincipal;

            if (DataContext is VentanaPrincipalVistaModelo vistaModelo)
            {
                await vistaModelo.FinalizarAsync().ConfigureAwait(false);
            }

            App.MusicaManejador.Detener();
        }
    }
}