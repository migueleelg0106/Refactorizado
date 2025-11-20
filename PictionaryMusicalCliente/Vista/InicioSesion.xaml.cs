using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Dialogos;
using PictionaryMusicalCliente.ClienteServicios.Idiomas;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana principal de acceso a la aplicacion.
    /// </summary>
    public partial class InicioSesion : Window
    {
        private readonly MusicaManejador _servicioMusica;

        /// <summary>
        /// Inicializa la ventana de inicio de sesion y configura dependencias.
        /// </summary>
        public InicioSesion()
        {
            Resources["Localizacion"] = new Utilidades.Idiomas.LocalizacionContexto();
            InitializeComponent();

            _servicioMusica = new MusicaManejador();
            _servicioMusica.ReproducirEnBucle("inicio_sesion_musica.mp3");

            IInicioSesionServicio inicioSesionServicio = new InicioSesionServicio();
            ICambioContrasenaServicio cambioContrasenaServicio = new CambioContrasenaServicio();
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio =
                new VerificacionCodigoDialogoServicio();
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio =
                new RecuperacionCuentaDialogoServicio(verificarCodigoDialogoServicio);
            ILocalizacionServicio localizacionServicio = LocalizacionServicio.Instancia;

            var vistaModelo = new InicioSesionVistaModelo(
                inicioSesionServicio,
                cambioContrasenaServicio,
                recuperacionCuentaDialogoServicio,
                localizacionServicio,
                () => new SalasServicio())
            {
                AbrirCrearCuenta = () =>
                {
                    var ventana = new CreacionCuenta();
                    ventana.ShowDialog();
                },
                CerrarAccion = Close
            };

            vistaModelo.MostrarIngresoInvitado = vistaModeloInvitado =>
            {
                if (vistaModeloInvitado == null)
                {
                    return;
                }

                var ventana = new IngresoPartidaInvitado(vistaModeloInvitado)
                {
                    Owner = this
                };

                ventana.ShowDialog();
            };

            vistaModelo.AbrirVentanaJuegoInvitado = (sala, salasServicio, nombreInvitado) =>
            {
                if (sala == null || salasServicio == null)
                {
                    return;
                }

                _servicioMusica.Detener();

                var ventanaJuego = new VentanaJuego(
                    sala,
                    salasServicio,
                    esInvitado: true,
                    nombreJugador: nombreInvitado,
                    accionAlCerrar: () =>
                    {
                        var inicioSesion = new InicioSesion();
                        inicioSesion.Show();
                    });

                ventanaJuego.Show();
                Close();
            };

            vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            vistaModelo.InicioSesionCompletado = _ =>
            {
                var ventanaPrincipal = new VentanaPrincipal();
                ventanaPrincipal.Show();
            };

            DataContext = vistaModelo;
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is InicioSesionVistaModelo vistaModelo &&
                sender is PasswordBox passwordBox)
            {
                vistaModelo.EstablecerContrasena(passwordBox.Password);
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(campoContrasenaContrasena);

            if (camposInvalidos == null)
            {
                return;
            }

            foreach (string campo in camposInvalidos)
            {
                switch (campo)
                {
                    case nameof(InicioSesionVistaModelo.Identificador):
                        ControlVisual.MarcarCampoInvalido(campoTextoUsuario);
                        break;
                    case InicioSesionVistaModelo.CampoContrasena:
                        ControlVisual.MarcarCampoInvalido(campoContrasenaContrasena);
                        break;
                }
            }
        }

        private void InicioSesion_Cerrado(object sender, System.EventArgs e)
        {
            _servicioMusica.Detener();
            _servicioMusica.Dispose();
        }

        private void BotonAudio_Click(object sender, RoutedEventArgs e)
        {
            bool estaSilenciado = _servicioMusica.AlternarSilencio();

            if (estaSilenciado)
            {
                imagenBotonAudio.Source = new BitmapImage(
                    new Uri("/Recursos/Audio_Apagado.png", UriKind.Relative));
            }
            else
            {
                imagenBotonAudio.Source = new BitmapImage(
                    new Uri("/Recursos/Audio_Encendido.png", UriKind.Relative));
            }
        }
    }
}