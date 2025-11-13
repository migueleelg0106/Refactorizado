using System;
using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Salas;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para ExpulsionJugador.xaml
    /// </summary>
    public partial class ExpulsionJugador : Window
    {
        private readonly ExpulsionJugadorVistaModelo _vistaModelo;

        public ExpulsionJugador(string mensajeConfirmacion)
            : this(new ExpulsionJugadorVistaModelo(mensajeConfirmacion))
        {
        }

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