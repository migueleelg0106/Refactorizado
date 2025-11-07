using System;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios.Wcf;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.VistaModelo.Amigos;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para BuscarAmigo.xaml
    /// </summary>
    public partial class BusquedaAmigo : Window
    {
        private readonly BusquedaAmigoVistaModelo _vistaModelo;

        public BusquedaAmigo()
            : this(new BusquedaAmigoVistaModelo(new AmigosServicio()))
        {
        }

        public BusquedaAmigo(IAmigosServicio amigosServicio)
            : this(new BusquedaAmigoVistaModelo(amigosServicio))
        {
        }

        public BusquedaAmigo(BusquedaAmigoVistaModelo vistaModelo)
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
