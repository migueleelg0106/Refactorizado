using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para el ingreso y validacion de codigos de verificacion 
    /// (correo/recuperacion).
    /// </summary>
    public partial class VerificacionCodigo : Window
    {
        public VerificacionCodigo()
        {
            InitializeComponent();
            DataContextChanged += AlCambiarContextoDeDatosVerificacionCodigo;
        }

        private void AlCambiarContextoDeDatosVerificacionCodigo(
            object remitente, 
            DependencyPropertyChangedEventArgs argumentosEvento)
        {
            if (argumentosEvento.NewValue is VerificacionCodigoVistaModelo vistaModelo)
            {
                vistaModelo.MarcarCodigoInvalido = MarcarCodigoInvalido;
            }
        }

        private void MarcarCodigoInvalido(bool invalido)
        {
            if (invalido)
            {
                ControlVisual.MarcarCampoInvalido(campoTextoCodigoVerificacion);
            }
            else
            {
                ControlVisual.RestablecerEstadoCampo(
                    campoTextoCodigoVerificacion);
            }
        }
    }
}