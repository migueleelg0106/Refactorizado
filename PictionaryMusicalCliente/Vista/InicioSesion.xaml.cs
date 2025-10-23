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

namespace PictionaryMusicalCliente
{
    public partial class InicioSesion : Window
    {
        public InicioSesion()
        {
            Resources["Localizacion"] = new Utilidades.Idiomas.LocalizacionContexto();
            InitializeComponent();


            IInicioSesionServicio inicioSesionService = new InicioSesionServicio();
            ICambioContrasenaServicio cambioContrasenaService = new CambioContrasenaServicio();
            IVerificarCodigoDialogoServicio verificarCodigoDialogService = new VerificarCodigoDialogoServicio();
            IRecuperacionCuentaServicio recuperacionCuentaDialogService =
                new RecuperacionCuentaDialogoServicio(verificarCodigoDialogService);
            ILocalizacionServicio localizacionService = LocalizacionServicio.Instancia;

            var vistaModelo = new InicioSesionVistaModelo(
                inicioSesionService,
                cambioContrasenaService,
                recuperacionCuentaDialogService,
                localizacionService)
            {
                AbrirCrearCuenta = () =>
                {
                    var ventana = new CrearCuenta();
                    ventana.ShowDialog();
                },
                IniciarSesionInvitado = () =>
                {
                    var ventana = new UnirsePartidaInvitado();
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
            ControlVisual.RestablecerEstadoCampo(bloqueTextoUsuario);
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
                        ControlVisual.MarcarCampoInvalido(bloqueTextoUsuario);
                        break;
                    case InicioSesionVistaModelo.CampoContrasena:
                        ControlVisual.MarcarCampoInvalido(bloqueContrasenaContrasena);
                        break;
                }
            }
        }
    }
}
