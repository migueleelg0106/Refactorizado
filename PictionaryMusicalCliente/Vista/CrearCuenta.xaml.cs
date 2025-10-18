using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class CrearCuenta : Window
    {
        private readonly CrearCuentaVistaModelo _vistaModelo;

        public CrearCuenta()
        {
            InitializeComponent();

            var codigoVerificacionService = new CodigoVerificacionService();
            var cuentaService = new CuentaService();
            var seleccionarAvatarService = new SeleccionarAvatarDialogService();
            var verificarCodigoDialogService = new VerificarCodigoDialogService();

            _vistaModelo = new CrearCuentaVistaModelo(
                codigoVerificacionService,
                cuentaService,
                seleccionarAvatarService,
                verificarCodigoDialogService);

            _vistaModelo.CerrarAccion = Close;
            _vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            _vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = _vistaModelo;
        }

        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _vistaModelo.Contrasena = passwordBox.Password;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoApellido);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoCorreo);
            ControlVisualHelper.RestablecerEstadoCampo(bloqueContrasena);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Usuario)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Nombre)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Apellido)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoApellido);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Correo)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCorreo);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Contrasena)))
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueContrasena);
            }
        }
    }
}
