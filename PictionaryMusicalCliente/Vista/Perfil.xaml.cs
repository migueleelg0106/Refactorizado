using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana de gestion del perfil de usuario.
    /// </summary>
    public partial class Perfil : Window
    {
        private readonly PerfilVistaModelo _vistaModelo;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicacion debe usar el constructor que recibe dependencias.
        /// </summary>
        public Perfil()
        {
        }

        /// <summary>
        /// Inicializa la ventana inyectando el ViewModel configurado.
        /// </summary>
        /// <param name="vistaModelo">Logica de negocio del perfil.</param>
        public Perfil(PerfilVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));

            InitializeComponent();

            ConfigurarInteraccion();
            DataContext = _vistaModelo;
        }

        private void ConfigurarInteraccion()
        {
            _vistaModelo.CerrarAccion = Close;
            _vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
        }

        private async void Perfil_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await _vistaModelo.CargarPerfilAsync().ConfigureAwait(true);
        }

        private void PopupRedSocial_Opened(object sender, EventArgs e)
        {
            if (sender is Popup popup &&
                popup.Child is Border border &&
                border.Child is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            }
        }

        private void RedSocialTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox &&
                textBox.Tag is ToggleButton toggle &&
                (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape))
            {
                toggle.IsChecked = false;
                e.Handled = true;
            }
        }

        private void MarcarCamposInvalidos(IList<string> camposInvalidos)
        {
            ControlVisual.RestablecerEstadoCampo(campoTextoNombre);
            ControlVisual.RestablecerEstadoCampo(campoTextoApellido);

            if (camposInvalidos == null) return;

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