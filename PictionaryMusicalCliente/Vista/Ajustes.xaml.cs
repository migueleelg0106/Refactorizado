using System.Windows;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.VistaModelo.Ajustes;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : Window
    {
        private readonly AjustesVistaModelo _viewModel;

        public Ajustes(MusicaManejador servicioMusica)
        {
            InitializeComponent();

            _viewModel = new AjustesVistaModelo(servicioMusica);

            _viewModel.OcultarVentana = () => this.Close();
            _viewModel.MostrarDialogoCerrarSesion = () =>
            {
                TerminacionSesion cerrarSesion = new TerminacionSesion
                {
                    Owner = this
                };
                cerrarSesion.ShowDialog();
            };

            this.DataContext = _viewModel;
        }
    }
}