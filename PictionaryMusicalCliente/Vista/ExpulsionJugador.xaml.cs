using System;
using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Salas;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Dialogo de confirmacion para expulsar a un jugador de la sala.
    /// </summary>
    public partial class ExpulsionJugador : Window
    {
        private readonly ExpulsionJugadorVistaModelo _vistaModelo;

        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public ExpulsionJugador()
        {
        }

        /// <summary>
        /// Inicializa el dialogo inyectando la logica de vista.
        /// </summary>
        /// <param name="vistaModelo">Modelo de vista de expulsion.</param>
        public ExpulsionJugador(ExpulsionJugadorVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));

            InitializeComponent();

            DataContext = _vistaModelo;

            _vistaModelo.Cerrar += VistaModelo_Cerrar;
            Closed += ExpulsionJugador_Closed;
        }

        private void VistaModelo_Cerrar(bool? resultado)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => VistaModelo_Cerrar(resultado));
                return;
            }

            DialogResult = resultado;
            Close();
        }

        private void ExpulsionJugador_Closed(object sender, EventArgs e)
        {
            Closed -= ExpulsionJugador_Closed;
            _vistaModelo.Cerrar -= VistaModelo_Cerrar;
        }
    }
}