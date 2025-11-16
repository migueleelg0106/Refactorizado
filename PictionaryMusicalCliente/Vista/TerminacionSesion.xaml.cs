using System.Windows;
using PictionaryMusicalCliente.VistaModelo; 

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para TerminacionSesion.xaml
    /// </summary>
    public partial class TerminacionSesion : Window
    {
        private readonly TerminacionSesionVistaModelo _viewModel;

        public TerminacionSesion()
        {
            InitializeComponent();

            _viewModel = new TerminacionSesionVistaModelo();

            _viewModel.OcultarDialogo = () => this.Close();

            this.DataContext = _viewModel;
        }
    }
}