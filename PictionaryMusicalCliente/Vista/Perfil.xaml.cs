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
    public partial class Perfil : Window
    {
        public Perfil()
        {
            InitializeComponent();
            DataContextChanged += AlCambiarContextoDeDatosPerfil;
        }

        private void AlCambiarContextoDeDatosPerfil(
            object remitente, 
            DependencyPropertyChangedEventArgs argumentosEvento)
        {
            if (argumentosEvento.NewValue is PerfilVistaModelo vistaModelo)
            {
                vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
                Loaded += AlCargarPerfilAsync;
                Closed += AlCerrarPerfil;
            }
        }

        private async void AlCargarPerfilAsync(object remitente, RoutedEventArgs argumentosEvento)
        {
            if (DataContext is PerfilVistaModelo vistaModelo)
            {
                await vistaModelo.CargarPerfilAsync().ConfigureAwait(true);
            }
        }

        private void AlCerrarPerfil(object remitente, EventArgs argumentosEvento)
        {
            if (DataContext is PerfilVistaModelo vistaModelo && 
                vistaModelo.RequiereReinicioSesion)
            {
                Application.Current.Shutdown();
            }
        }

        private void AlAbrirPopupRedSocial(object remitente, EventArgs argumentosEvento)
        {
            if (remitente is Popup ventanaEmergente &&
                ventanaEmergente.Child is Border borde &&
                borde.Child is TextBox cajaTexto)
            {
                cajaTexto.Focus();
                cajaTexto.CaretIndex = cajaTexto.Text?.Length ?? 0;
            }
        }

        private void AlPresionarTeclaEnCajaTextoRedSocial(object remitente, KeyEventArgs argumentosEvento)
        {
            if (remitente is TextBox cajaTexto &&
                cajaTexto.Tag is ToggleButton botonAlternar &&
                (argumentosEvento.Key == Key.Enter || argumentosEvento.Key == Key.Return || argumentosEvento.Key == Key.Escape))
            {
                botonAlternar.IsChecked = false;
                argumentosEvento.Handled = true;
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