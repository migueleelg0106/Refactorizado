using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Cuentas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Solicitudes.xaml
    /// </summary>
    public partial class Solicitudes : Window
    {
        private readonly SolicitudesVistaModelo _vistaModelo;

        public Solicitudes()
        {
            InitializeComponent();

            IAmigosService amigosService = new AmigosService();

            _vistaModelo = new SolicitudesVistaModelo(amigosService)
            {
                CerrarAccion = Close
            };

            _vistaModelo.MostrarMensaje = AvisoHelper.Mostrar;

            DataContext = _vistaModelo;
        }
    }
}
