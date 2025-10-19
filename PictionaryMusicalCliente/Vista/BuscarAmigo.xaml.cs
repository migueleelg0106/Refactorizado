using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para BuscarAmigo.xaml
    /// </summary>
    public partial class BuscarAmigo : Window
    {
        public BuscarAmigo()
            : this(AmigosService.Instancia)
        {
        }

        public BuscarAmigo(IAmigosService amigosService)
        {
            if (amigosService == null)
            {
                throw new ArgumentNullException(nameof(amigosService));
            }

            InitializeComponent();

            var vistaModelo = new BuscarAmigoVistaModelo(amigosService)
            {
                CerrarAccion = Close,
                MostrarMensaje = AvisoHelper.Mostrar,
                MarcarCampoUsuarioInvalido = MarcarCampoUsuarioInvalido
            };

            DataContext = vistaModelo;
        }

        private void MarcarCampoUsuarioInvalido(bool invalido)
        {
            if (invalido)
            {
                ControlVisualHelper.MarcarCampoInvalido(BloqueUsuario);
            }
            else
            {
                ControlVisualHelper.RestablecerEstadoCampo(BloqueUsuario);
            }
        }
    }
}
