using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class InicioSesion : Window
    {
        public InicioSesion()
        {
            InitializeComponent();

            IInicioSesionService inicioSesionService = new InicioSesionService();
            ICambioContrasenaService cambioContrasenaService = new CambioContrasenaService();
            IVerificarCodigoDialogService verificarCodigoDialogService = new VerificarCodigoDialogService();
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService =
                new RecuperacionCuentaDialogService(verificarCodigoDialogService);

            var vistaModelo = new InicioSesionVistaModelo(
                inicioSesionService,
                cambioContrasenaService,
                recuperacionCuentaDialogService)
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
    }
}
