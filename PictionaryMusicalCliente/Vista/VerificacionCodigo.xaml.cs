using System.Windows;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VerificacionCodigo : Window
    {
        public VerificacionCodigo()
        {
            InitializeComponent();
        }

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
