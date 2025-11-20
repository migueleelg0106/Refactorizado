using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana para gestionar las solicitudes de amistad pendientes (aceptar/rechazar).
    /// </summary>
    public partial class Solicitudes : Window
    {
        private readonly SolicitudesVistaModelo _vistaModelo;

        /// <summary>
        /// Inicializa la ventana con el servicio de amigos por defecto.
        /// </summary>
        public Solicitudes()
            : this(new SolicitudesVistaModelo(new AmigosServicio()))
        {
        }

        /// <summary>
        /// Inicializa la ventana inyectando una implementacion del servicio.
        /// </summary>
        /// <param name="amigosServicio">Servicio de gestion de amigos.</param>
        public Solicitudes(IAmigosServicio amigosServicio)
            : this(new SolicitudesVistaModelo(amigosServicio))
        {
        }

        /// <summary>
        /// Constructor principal que configura la vista modelo y eventos.
        /// </summary>
        public Solicitudes(SolicitudesVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));

            InitializeComponent();

            DataContext = _vistaModelo;

            _vistaModelo.Cerrar += VistaModelo_Cerrar;
            Closed += Solicitudes_Closed;
        }

        private void VistaModelo_Cerrar()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_Cerrar);
                return;
            }

            Close();
        }

        private void Solicitudes_Closed(object sender, EventArgs e)
        {
            Closed -= Solicitudes_Closed;
            _vistaModelo.Cerrar -= VistaModelo_Cerrar;
            _vistaModelo.Dispose();
        }
    }
}