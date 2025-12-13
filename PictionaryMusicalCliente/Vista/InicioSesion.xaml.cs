using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana principal de acceso a la aplicacion.
    /// </summary>
    public partial class InicioSesion : Window
    {
        /// <summary>
        /// Constructor por defecto. VentanaServicio asigna el DataContext.
        /// </summary>
        public InicioSesion()
        {
            InitializeComponent();
            Loaded += InicioSesion_Loaded;
            Closed += InicioSesion_Closed;
        }

        private void InicioSesion_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vm)
            {
                vm.MostrarCamposInvalidos = MarcarCamposInvalidos;
                App.MusicaManejador.ReproducirEnBucle("inicio_sesion_musica.mp3");
            }
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vm && sender is PasswordBox pb)
            {
                vm.EstablecerContrasena(pb.Password);
            }
        }

        private void MarcarCamposInvalidos(IList<string> campos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(campoContrasenaContrasena);

            if (campos == null) return;

            foreach (var campo in campos)
            {
                if (campo == nameof(InicioSesionVistaModelo.Identificador))
                    ControlVisual.MarcarCampoInvalido(campoTextoUsuario);
                else if (campo == InicioSesionVistaModelo.CampoContrasena)
                    ControlVisual.MarcarCampoInvalido(campoContrasenaContrasena);
            }
        }

        private void InicioSesion_Closed(object sender, EventArgs e)
        {
            Loaded -= InicioSesion_Loaded;
            Closed -= InicioSesion_Closed;
        }

        private void BotonAudio_Click(object sender, RoutedEventArgs e)
        {
            bool silenciado = App.MusicaManejador.AlternarSilencio();
            string ruta = silenciado ? "Audio_Apagado.png" : "Audio_Encendido.png";
            imagenBotonAudio.Source = new BitmapImage(
                new Uri($"/Recursos/{ruta}", UriKind.Relative));
        }
    }
}