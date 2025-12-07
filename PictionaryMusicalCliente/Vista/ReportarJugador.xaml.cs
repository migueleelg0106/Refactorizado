using System;
using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Salas;

namespace PictionaryMusicalCliente.Vista

{
    /// <summary>
    /// Lógica de interacción para ReportarJugador.xaml
    /// </summary>
    public partial class ReportarJugador : Window
    {
        private readonly ReportarJugadorVistaModelo _vistaModelo;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public ReportarJugador()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la ventana de diálogo <see cref="ReportarJugador"/>
        /// inyectando el <see cref="ReportarJugadorVistaModelo"/> con la lógica de negocio.
        /// </summary>
        /// <param name="vistaModelo">El ViewModel que gestiona la lógica de reportes.</param>
        public ReportarJugador(ReportarJugadorVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));
            InitializeComponent();
            DataContext = _vistaModelo;
            _vistaModelo.Cerrar = resultado => DialogResult = resultado;

            Closed += ReportarJugador_Closed;
        }

        private void ReportarJugador_Closed(object sender, EventArgs e)
        {
            _vistaModelo.Cerrar = null;
            Closed -= ReportarJugador_Closed;
        }
    }
}