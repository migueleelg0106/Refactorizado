using System.Windows;


namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Avisos : Window
    {
        public Avisos(string mensaje)
        {
            InitializeComponent();
            bloqueTextoMensaje.Text = mensaje; 

        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
