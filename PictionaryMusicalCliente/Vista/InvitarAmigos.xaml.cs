using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Windows;

namespace PictionaryMusicalCliente.Vista
{
    /// <summary>
    /// Ventana para enviar invitaciones a una partida a los amigos conectados.
    /// </summary>
    public partial class InvitarAmigos : Window
    {
        /// <summary>
        /// Constructor por defecto, solo para uso del diseñador/XAML. 
        /// La aplicación debe usar el constructor que recibe dependencias.
        /// </summary>
        public InvitarAmigos()
        {
        }

        /// <summary>
        /// Inicializa la ventana con la logica de invitacion.
        /// </summary>
        /// <param name="vistaModelo">El modelo de vista con la lista de amigos.</param>
        public InvitarAmigos(InvitarAmigosVistaModelo vistaModelo)
        {
            InitializeComponent();
            DataContext = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));
        }
    }
}