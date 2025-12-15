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
            Loaded += AlCargarInicioSesion;
            Closed += AlCerrarInicioSesion;
        }

        private void AlCargarInicioSesion(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
                App.MusicaManejador.ReproducirEnBucle("inicio_sesion_musica.mp3");
            }
        }

        private void AlCambiarContrasena(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vistaModelo && sender is 
                PasswordBox cajaContrasena)
            {
                vistaModelo.EstablecerContrasena(cajaContrasena.Password);
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

        private void AlCerrarInicioSesion(object sender, EventArgs e)
        {
            Loaded -= AlCargarInicioSesion;
            Closed -= AlCerrarInicioSesion;
        }

        private void AlHacerClicEnBotonAudio(object sender, RoutedEventArgs e)
        {
            bool estaSilenciado = App.MusicaManejador.AlternarSilencio();
            string rutaImagen = estaSilenciado ? "Audio_Apagado.png" : "Audio_Encendido.png";
            imagenBotonAudio.Source = new BitmapImage(
                new Uri($"/Recursos/{rutaImagen}", UriKind.Relative));
        }
    }
}