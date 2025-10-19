using System.Windows;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        public EliminarAmigo()
        {
            InitializeComponent();
        }

        public void ConfigurarVistaModelo(EliminarAmigoVistaModelo vistaModelo)
        {
            if (vistaModelo == null)
            {
                return;
            }

            vistaModelo.MostrarMensaje ??= AvisoHelper.Mostrar;
            vistaModelo.CerrarAccion = Close;

            DataContext = vistaModelo;
        }
    }
}
