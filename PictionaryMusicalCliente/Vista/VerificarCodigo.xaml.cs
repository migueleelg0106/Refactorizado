using System.Windows;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    public partial class VerificarCodigo : Window
    {
        public VerificarCodigo()
        {
            InitializeComponent();
        }

        public void ConfigurarVistaModelo(VerificarCodigoVistaModelo vistaModelo)
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
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoCodigoVerificacion);
            }
            else
            {
                ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoCodigoVerificacion);
            }
        }
    }
}
