using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Ajustes.xaml
    /// </summary>
    public partial class Ajustes : Window
    {
        public Ajustes()
        {
            InitializeComponent();
        }

        private void BotonConfirmar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BotonCerrarSesion(object sender, RoutedEventArgs e)
        {
            TerminacionSesion cerrarSesion = new TerminacionSesion();
            cerrarSesion.Owner = this;
            cerrarSesion.ShowDialog();
        }
    }
}
