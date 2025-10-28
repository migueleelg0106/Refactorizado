using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para ComoJugar.xaml
    /// </summary>
    public partial class ComoJugar : Window
    {
        public ComoJugar()
        {
            InitializeComponent();
        }

        private void BotonRegresar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
