using System.Windows;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Perfil;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para el ingreso y validacion de codigos de verificacion (correo/recuperacion).
    /// </summary>
    public partial class VerificacionCodigo : Window
    {
        /// <summary>
        /// Inicializa la ventana.
        /// </summary>
        public VerificacionCodigo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configura el contexto de datos y los delegados de interaccion visual.
        /// </summary>
        /// <param name="vistaModelo">La logica de negocio para la verificacion.</param>
        public void ConfigurarVistaModelo(VerificacionCodigoVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return;
            }

            vistaModelo.MarcarCodigoInvalido = MarcarCodigoInvalido;
            DataContext = vistaModelo;
        }

        private void MarcarCodigoInvalido(bool invalido)
        {
            if (invalido)
            {
                ControlVisual.MarcarCampoInvalido(campoTextoCodigoVerificacion);
            }
            else
            {
                ControlVisual.RestablecerEstadoCampo(campoTextoCodigoVerificacion);
            }
        }
    }
}