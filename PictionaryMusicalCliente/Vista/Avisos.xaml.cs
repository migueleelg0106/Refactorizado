using System.Windows;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Ventana emergente genérica para mostrar mensajes informativos o de error al usuario.
    /// </summary>
    public partial class Avisos : Window
    {
        /// <summary>
        /// Crea una nueva instancia de la ventana de avisos.
        /// </summary>
        /// <param name="mensaje">El texto a mostrar en el cuerpo del aviso.</param>
        public Avisos(string mensaje)
        {
            InitializeComponent();
            bloqueTextoMensaje.Text = mensaje;
        }

        private void BotonAceptar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}