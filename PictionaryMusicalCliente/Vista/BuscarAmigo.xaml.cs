using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

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

            IAmigosService amigosService = new AmigosService();

            var vistaModelo = new BuscarAmigoVistaModelo(amigosService)
            {
                CerrarAccion = Close
            };

            vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;
            vistaModelo.MarcarUsuarioInvalido = MarcarUsuarioInvalido;

            DataContext = vistaModelo;
        }

        private void MarcarUsuarioInvalido(bool invalido)
        {
            if (invalido)
            {
                ControlVisualHelper.MarcarCampoInvalido(campoUsuario);
            }
            else
            {
                ControlVisualHelper.RestablecerEstadoCampo(campoUsuario);
            }
        }
    }
}
