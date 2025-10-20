using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Solicitudes.xaml
    /// </summary>
    public partial class Solicitudes : Window
    {
        private readonly SolicitudesVistaModelo _vistaModelo;

        public Solicitudes()
            : this(AmigosService.Instancia)
        {
        }

        public Solicitudes(IAmigosService amigosService)
        {
            InitializeComponent();

            _vistaModelo = new SolicitudesVistaModelo(amigosService)
            {
                CerrarAccion = Close,
                MostrarMensaje = AvisoHelper.Mostrar
            };

            DataContext = _vistaModelo;
            Loaded += Solicitudes_Loaded;
            Closed += Solicitudes_Closed;
        }

        private void Solicitudes_Loaded(object sender, RoutedEventArgs e)
        {
            _vistaModelo.CargarSolicitudes();
        }

        private void Solicitudes_Closed(object sender, System.EventArgs e)
        {
            _vistaModelo.Liberar();
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
