using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
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

            var codigoVerificacionService = new CodigoVerificacionServicio();
            var cuentaService = new CuentaServicio();
            IAvatarServicio avatarService = new AvatarServicio();
            var seleccionarAvatarService = new SeleccionarAvatarDialogoServicio(avatarService);
            var verificarCodigoDialogService = new VerificarCodigoDialogoServicio();

            _vistaModelo = new CrearCuentaVistaModelo(
                codigoVerificacionService,
                cuentaService,
                seleccionarAvatarService,
                verificarCodigoDialogService,
                avatarService);

            _vistaModelo.CerrarAccion = Close;
            _vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
            _vistaModelo.MostrarMensaje = AvisoAyudante.Mostrar;

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
            ControlVisual.RestablecerEstadoCampo(bloqueTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(bloqueTextoNombre);
            ControlVisual.RestablecerEstadoCampo(bloqueTextoApellido);
            ControlVisual.RestablecerEstadoCampo(bloqueTextoCorreo);
            ControlVisual.RestablecerEstadoCampo(bloqueContrasena);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Usuario)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoUsuario);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Nombre)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Apellido)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoApellido);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Correo)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueTextoCorreo);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Contrasena)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueContrasena);
            }
        }
    }
}
