using System.Windows;
using PictionaryMusicalCliente.Utilidades; 
using PictionaryMusicalCliente.VistaModelo.Ajustes; 

namespace PictionaryMusicalCliente
{
    public partial class AjustesPartida : Window
    {
        private readonly AjustesPartidaVistaModelo _viewModel;

        public AjustesPartida(CancionManejador servicioCancion)
        {
            InitializeComponent();

            _viewModel = new AjustesPartidaVistaModelo(servicioCancion);

            _viewModel.OcultarVentana = () => this.Close();

            _viewModel.MostrarDialogoSalirPartida = () =>
            {
                ConfirmacionSalirPartida confirmacionSalirPartida = new ConfirmacionSalirPartida
                {
                    Owner = this
                };
                confirmacionSalirPartida.ShowDialog();
            };

            this.DataContext = _viewModel;
        }
    }
}