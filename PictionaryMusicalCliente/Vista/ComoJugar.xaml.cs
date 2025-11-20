using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana informativa que muestra las instrucciones del juego.
    /// </summary>
    public partial class ComoJugar : Window
    {
        /// <summary>
        /// Inicializa la ventana de instrucciones.
        /// </summary>
        public ComoJugar()
        {
            InitializeComponent();
        }

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}