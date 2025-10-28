using System.Linq;
using System.Windows;
using PictionaryMusicalCliente.Sesiones;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para CerrarSesion.xaml
    /// </summary>
    public partial class TerminacionSesion : Window
    {
        public TerminacionSesion()
        {
            InitializeComponent();
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            var ventanasActivas = Application.Current.Windows.Cast<Window>().ToList();

            SesionUsuarioActual.Instancia.CerrarSesion();

            var inicioSesion = new InicioSesion();
            Application.Current.MainWindow = inicioSesion;
            inicioSesion.Show();

            foreach (Window ventana in ventanasActivas)
            {
                if (ventana != inicioSesion)
                {
                    ventana.Close();
                }
            }
        }

        private void BotonCancelar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
