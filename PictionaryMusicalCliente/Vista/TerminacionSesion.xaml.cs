using PictionaryMusicalCliente.VistaModelo.Sesion; 
using System.Linq;
using System.Windows;

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
            _viewModel.EjecutarCierreSesionYNavegacion = EjecutarNavegacionInicioSesion;

            this.DataContext = _viewModel;
        }

        private static void EjecutarNavegacionInicioSesion()
        {
            var inicioSesion = new InicioSesion();

            var ventanasACerrar = Application.Current.Windows
                .Cast<Window>()
                .Where(v => v != inicioSesion)
                .ToList();

            inicioSesion.Show();
            Application.Current.MainWindow = inicioSesion;

            foreach (var ventana in ventanasACerrar)
            {
                ventana.Close();
            }
        }
    }
}