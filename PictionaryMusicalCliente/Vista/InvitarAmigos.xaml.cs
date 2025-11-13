using PictionaryMusicalCliente.VistaModelo.Amigos;
using System;
using System.Windows;

namespace PictionaryMusicalCliente
{
    public partial class InvitarAmigos : Window
    {
        public InvitarAmigos(InvitarAmigosVistaModelo vistaModelo)
        {
            InitializeComponent();
            DataContext = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));
        }
    }
}
