using System;
using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Dialogo modal para confirmar la eliminacion de un contacto.
    /// </summary>
    public partial class EliminacionAmigo : Window
    {
        private readonly EliminacionAmigoVistaModelo _vistaModelo;

        /// <summary>
        /// Inicializa el dialogo para un amigo especifico.
        /// </summary>
        /// <param name="nombreAmigo">Nombre del usuario a eliminar.</param>
        public EliminacionAmigo(string nombreAmigo)
            : this(new EliminacionAmigoVistaModelo(nombreAmigo))
        {
        }

        /// <summary>
        /// Constructor principal que permite inyeccion de vista modelo.
        /// </summary>
        public EliminacionAmigo(EliminacionAmigoVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));

            InitializeComponent();

            DataContext = _vistaModelo;

            _vistaModelo.Cerrar += VistaModelo_Cerrar;
            Closed += EliminarAmigo_Closed;
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

        private void EliminarAmigo_Closed(object sender, EventArgs e)
        {
            Closed -= EliminarAmigo_Closed;
            _vistaModelo.Cerrar -= VistaModelo_Cerrar;
        }
    }
}