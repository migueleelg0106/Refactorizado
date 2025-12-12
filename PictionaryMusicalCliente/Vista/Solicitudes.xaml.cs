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
            Closed += Solicitudes_Closed;
        }

        private void Solicitudes_Closed(object sender, System.EventArgs e)
        {
            Closed -= Solicitudes_Closed;
            if (DataContext is SolicitudesVistaModelo vistaModelo)
            {
                vistaModelo.Dispose();
            }
        }
    }
}