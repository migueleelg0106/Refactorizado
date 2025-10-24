using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.Servicios.Abstracciones;
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
            : this(new SolicitudesVistaModelo(new AmigosServicio()))
        {
        }

        public Solicitudes(IAmigosServicio amigosServicio)
            : this(new SolicitudesVistaModelo(amigosServicio))
        {
        }

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
