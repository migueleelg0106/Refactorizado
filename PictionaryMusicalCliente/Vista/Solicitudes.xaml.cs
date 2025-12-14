using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para gestionar las solicitudes de amistad pendientes.
    /// </summary>
    public partial class Solicitudes : Window
    {
        public Solicitudes()
        {
            InitializeComponent();
            Closed += AlCerrarSolicitudes;
        }

        private void AlCerrarSolicitudes(object remitente, System.EventArgs argumentosEvento)
        {
            Closed -= AlCerrarSolicitudes;
            if (DataContext is SolicitudesVistaModelo vistaModelo)
            {
                vistaModelo.Dispose();
            }
        }
    }
}