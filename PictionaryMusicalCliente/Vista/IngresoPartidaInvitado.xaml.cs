using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Windows;

namespace PictionaryMusicalCliente
{
    public partial class IngresoPartidaInvitado : Window
    {
        public IngresoPartidaInvitado(IngresoPartidaInvitadoVistaModelo vistaModelo)
        {
            InitializeComponent();

            DataContext = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));
            vistaModelo.CerrarVentana = Close;
            Closed += (_, __) => vistaModelo.CerrarVentana = null;
        }
    }
}
