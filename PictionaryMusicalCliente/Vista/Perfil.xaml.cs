using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Dialogos;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class Perfil : Window
    {
        public Perfil()
        {
            InitializeComponent();

            IPerfilServicio perfilServicio = new PerfilServicio();
            IAvatarServicio avatarServicio = new AvatarServicio();
            ISeleccionarAvatarServicio seleccionarAvatarServicio = new SeleccionAvatarDialogoServicio(avatarServicio);
            ICambioContrasenaServicio cambioContrasenaServicio = new CambioContrasenaServicio();
            IVerificacionCodigoDialogoServicio verificarCodigoDialogoServicio = new VerificacionCodigoDialogoServicio();
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio =
                new RecuperacionCuentaDialogoServicio(verificarCodigoDialogoServicio);

            var vistaModelo = new PerfilVistaModelo(
                perfilServicio,
                seleccionarAvatarServicio,
                cambioContrasenaServicio,
                recuperacionCuentaDialogoServicio,
                avatarServicio)
            {
                CerrarAccion = Close
            };

            vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;

            DataContext = vistaModelo;
        }

        private async void Perfil_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilVistaModelo vistaModelo)
            {
                await vistaModelo.CargarPerfilAsync().ConfigureAwait(true);
            }
        }

        private void PopupRedSocial_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup && popup.Child is Border border && border.Child is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            }
        }

        private void RedSocialTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is ToggleButton toggle)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape)
                {
                    toggle.IsChecked = false;
                    e.Handled = true;
                }
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoNombre);
            ControlVisual.RestablecerEstadoCampo(campoTextoApellido);

            if (camposInvalidos == null)
            {
                return;
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Nombre)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoNombre);
            }

            if (camposInvalidos.Contains(nameof(PerfilVistaModelo.Apellido)))
            {
                ControlVisual.MarcarCampoInvalido(campoTextoApellido);
            }
        }
    }
}
