using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana emergente generica para mostrar mensajes informativos o de error al usuario.
    /// </summary>
    public partial class Avisos : Window
    {
        /// <summary>
        /// Constructor por defecto, solo para uso del disenador/XAML. 
        /// La aplicacion debe usar el constructor que recibe dependencias.
        /// </summary>
        public Avisos()
        {
        }

        /// <summary>
        /// Crea una nueva instancia de la ventana de avisos.
        /// </summary>
        /// <param name="mensaje">El texto a mostrar en el cuerpo del aviso.</param>
        public Avisos(string mensaje)
        {
            InitializeComponent();
            bloqueTextoMensaje.Text = mensaje;
        }

        private void AlHacerClicEnBotonAceptar(object remitente, RoutedEventArgs argumentosEvento)
        {
            Close();
        }
    }
}