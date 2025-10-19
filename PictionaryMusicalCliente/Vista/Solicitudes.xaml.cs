using System;
using System.Windows;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para Solicitudes.xaml
    /// </summary>
    public partial class Solicitudes : Window
    {
        private readonly SolicitudesVistaModelo _vistaModelo;

        public Solicitudes()
            : this(AmigosService.Instancia)
        {
        }

        public Solicitudes(IAmigosService amigosService)
        {
            if (amigosService == null)
            {
                throw new ArgumentNullException(nameof(amigosService));
            }

            InitializeComponent();

            _vistaModelo = new SolicitudesVistaModelo(amigosService)
            {
                CerrarAccion = Close,
                MostrarMensaje = AvisoHelper.Mostrar
            };

            DataContext = _vistaModelo;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _vistaModelo?.Dispose();
        }
    }
}
