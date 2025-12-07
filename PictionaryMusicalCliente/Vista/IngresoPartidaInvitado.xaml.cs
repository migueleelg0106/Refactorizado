using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana de dialogo para que un invitado ingrese el codigo de sala.
    /// </summary>
    public partial class IngresoPartidaInvitado : Window
    {

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public IngresoPartidaInvitado()
        {
        }

        /// <summary>
        /// Inicializa la ventana de ingreso.
        /// </summary>
        /// <param name="vistaModelo">Logica para validar y unirse a la sala.</param>
        public IngresoPartidaInvitado(IngresoPartidaInvitadoVistaModelo vistaModelo)
        {
            InitializeComponent();

            DataContext = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));
            vistaModelo.CerrarVentana = Close;
            Closed += (_, __) => vistaModelo.CerrarVentana = null;
        }
    }
}