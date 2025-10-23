using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Servicios.Wcf;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para BuscarAmigo.xaml
    /// </summary>
    public partial class BuscarAmigo : Window
    {
        private readonly BuscarAmigoVistaModelo _vistaModelo;

        public BuscarAmigo()
            : this(new BuscarAmigoVistaModelo(new AmigosServicio()))
        {
        }

        public BuscarAmigo(IAmigosServicio amigosService)
            : this(new BuscarAmigoVistaModelo(amigosService))
        {
        }

        public BuscarAmigo(BuscarAmigoVistaModelo vistaModelo)
        {
            _vistaModelo = vistaModelo ?? throw new ArgumentNullException(nameof(vistaModelo));

            InitializeComponent();

            DataContext = _vistaModelo;

            _vistaModelo.SolicitudEnviada += VistaModelo_SolicitudEnviada;
            _vistaModelo.Cancelado += VistaModelo_Cancelado;
            Closed += BuscarAmigo_Closed;
        }

        private void VistaModelo_SolicitudEnviada()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_SolicitudEnviada);
                return;
            }

            Close();
        }

        private void VistaModelo_Cancelado()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(VistaModelo_Cancelado);
                return;
            }

            Close();
        }

        private void BuscarAmigo_Closed(object sender, EventArgs e)
        {
            Closed -= BuscarAmigo_Closed;
            _vistaModelo.SolicitudEnviada -= VistaModelo_SolicitudEnviada;
            _vistaModelo.Cancelado -= VistaModelo_Cancelado;
        }
    }
}
