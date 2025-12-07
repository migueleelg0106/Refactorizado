using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.InicioSesion;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana de registro para nuevos usuarios.
    /// </summary>
    public partial class CreacionCuenta : Window
    {
        private readonly CreacionCuentaVistaModelo _vistaModelo;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public CreacionCuenta()
        {
        }

        /// <summary>
        /// Inicializa la ventana inyectando el ViewModel con sus dependencias resueltas.
        /// </summary>
        /// <param name="vistaModelo">Logica de negocio para el registro.</param>
        public CreacionCuenta(CreacionCuentaVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                throw new ArgumentNullException(nameof(vistaModelo));
            }

            InitializeComponent();

            _vistaModelo = vistaModelo;
            ConfigurarInteracciones();

            DataContext = _vistaModelo;
        }

        private void ConfigurarInteracciones()
        {
            _vistaModelo.CerrarAccion = Close;
            _vistaModelo.MostrarCamposInvalidos = MarcarCamposInvalidos;
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
            ControlVisual.RestablecerEstadoCampo(campoContrasenaContrasena);

            if (camposInvalidos == null) return;

            if (camposInvalidos.Contains(nameof(_vistaModelo.Usuario)))
                ControlVisual.MarcarCampoInvalido(campoTextoUsuario);

            if (camposInvalidos.Contains(nameof(_vistaModelo.Nombre)))
                ControlVisual.MarcarCampoInvalido(campoTextoNombre);

            if (camposInvalidos.Contains(nameof(_vistaModelo.Apellido)))
                ControlVisual.MarcarCampoInvalido(campoTextoApellido);

            if (camposInvalidos.Contains(nameof(_vistaModelo.Correo)))
                ControlVisual.MarcarCampoInvalido(campoTextoCorreo);

            if (camposInvalidos.Contains(nameof(_vistaModelo.Contrasena)))
                ControlVisual.MarcarCampoInvalido(campoContrasenaContrasena);
        }
    }
}