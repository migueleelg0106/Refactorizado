using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;
using PictionaryMusicalCliente.Servicios.Idiomas;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios;

namespace PictionaryMusicalCliente
{
    public partial class InicioSesion : Window
    {
        private readonly ServicioMusica _servicioMusica;

        public InicioSesion()
        {
            Resources["Localizacion"] = new Utilidades.Idiomas.LocalizacionContexto();
            InitializeComponent();

            _servicioMusica = new ServicioMusica();
            _servicioMusica.ReproducirEnBucle("inicio_sesion_musica.mp3");

            IInicioSesionServicio inicioSesionServicio = new InicioSesionServicio();
            ICambioContrasenaServicio cambioContrasenaServicio = new CambioContrasenaServicio();
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio = new VerificacionCodigoDialogoServicio();
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio =
                new RecuperacionCuentaDialogoServicio(verificarCodigoDialogoServicio);
            ILocalizacionServicio localizacionServicio = LocalizacionServicio.Instancia;

            var vistaModelo = new InicioSesionVistaModelo(
                inicioSesionServicio,
                cambioContrasenaServicio,
                recuperacionCuentaDialogoServicio,
                localizacionServicio)
            {
                AbrirCrearCuenta = () =>
                {
                    var ventana = new CreacionCuenta();
                    ventana.ShowDialog();
                },
                IniciarSesionInvitado = () =>
                {
                    var ventana = new IngresoPartidaInvitado();
                    ventana.ShowDialog();
                },
                CerrarAccion = Close
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
            if (DataContext is InicioSesionVistaModelo vistaModelo && sender is PasswordBox passwordBox)
            {
                vistaModelo.EstablecerContrasena(passwordBox.Password);
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(bloqueContrasenaContrasena);

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
                        ControlVisual.MarcarCampoInvalido(bloqueContrasenaContrasena);
                        break;
                }
            }
        }

        private void InicioSesion_Cerrado(object sender, System.EventArgs e)
        {
            _servicioMusica.Detener();
            _servicioMusica.Dispose();
        }
    }
}
