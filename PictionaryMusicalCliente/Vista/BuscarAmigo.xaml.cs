using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
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
            : this(AmigosService.Instancia)
        {
        }

        public BuscarAmigo(IAmigosService amigosService)
        {
            InitializeComponent();

            var vistaModelo = new BuscarAmigoVistaModelo(amigosService)
            {
                CerrarAccion = Close,
                MostrarMensaje = AvisoHelper.Mostrar
            };

            DataContext = vistaModelo;
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
