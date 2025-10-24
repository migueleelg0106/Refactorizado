using System.Collections.Generic;
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
    public partial class CreacionCuenta : Window
    {
        private readonly CreacionCuentaVistaModelo _vistaModelo;

        public CreacionCuenta()
        {
            InitializeComponent();

            var codigoVerificacionServicio = new CodigoVerificacionServicio();
            var cuentaServicio = new CuentaServicio();
            IAvatarServicio avatarServicio = new AvatarServicio();
            var seleccionarAvatarServicio = new SeleccionAvatarDialogoServicio(avatarServicio);
            var verificarCodigoDialogoServicio = new VerificacionCodigoDialogoServicio();

            _vistaModelo = new CreacionCuentaVistaModelo(
                codigoVerificacionServicio,
                cuentaServicio,
                seleccionarAvatarServicio,
                verificarCodigoDialogoServicio,
                avatarServicio);

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
            ControlVisual.RestablecerEstadoCampo(campoTextoUsuario);
            ControlVisual.RestablecerEstadoCampo(campoTextoNombre);
            ControlVisual.RestablecerEstadoCampo(campoTextoApellido);
            ControlVisual.RestablecerEstadoCampo(campoTextoCorreo);
            ControlVisual.RestablecerEstadoCampo(bloqueContrasenaContrasena);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Usuario)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoUsuario);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Nombre)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Apellido)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoApellido);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Correo)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoCorreo);
            }

            if (camposInvalidos.Contains(nameof(_vistaModelo.Contrasena)))
            {
                ControlVisual.MarcarCampoInvalido(bloqueContrasenaContrasena);
            }
        }
    }
}
