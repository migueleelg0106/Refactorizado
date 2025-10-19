using System.Windows;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para BuscarAmigo.xaml
    /// </summary>
    public partial class BuscarAmigo : Window
    {
        public BuscarAmigo()
        {
            InitializeComponent();
        }

        public void ConfigurarVistaModelo(BuscarAmigoVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return;
            }

            vistaModelo.MarcarUsuarioInvalido = MarcarUsuarioInvalido;
            vistaModelo.MostrarMensaje ??= AvisoHelper.Mostrar;
            vistaModelo.CerrarAccion = Close;

            DataContext = vistaModelo;
        }

        private void MarcarUsuarioInvalido(bool invalido)
        {
            if (invalido)
            {
                ControlVisualHelper.MarcarCampoInvalido(bloqueTextoUsuario);
            }
            else
            {
                ControlVisualHelper.RestablecerEstadoCampo(bloqueTextoUsuario);
            }
        }
    }
}
