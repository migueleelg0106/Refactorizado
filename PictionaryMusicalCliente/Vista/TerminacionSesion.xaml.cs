using PictionaryMusicalCliente.VistaModelo.Sesion;
using System.Linq;
using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana de dialogo para confirmar y ejecutar el cierre de sesion del usuario.
    /// </summary>
    public partial class TerminacionSesion : Window
    {
        private readonly TerminacionSesionVistaModelo _vistaModelo;

        /// <summary>
        /// Inicializa la ventana y configura la logica de navegacion post-cierre.
        /// </summary>
        public TerminacionSesion()
        {
            InitializeComponent();

            _vistaModelo = new TerminacionSesionVistaModelo();
            _vistaModelo.OcultarDialogo = () => Close();
            _vistaModelo.EjecutarCierreSesionYNavegacion = EjecutarNavegacionInicioSesion;

            DataContext = _vistaModelo;
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