using System;
using System.Windows;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para EliminarAmigo.xaml
    /// </summary>
    public partial class EliminarAmigo : Window
    {
        private readonly EliminarAmigoVistaModelo _vistaModelo;

        public EliminarAmigo(string nombreAmigo)
            : this(new EliminarAmigoVistaModelo(nombreAmigo))
        {
        }

        public EliminarAmigo(EliminarAmigoVistaModelo vistaModelo)
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
