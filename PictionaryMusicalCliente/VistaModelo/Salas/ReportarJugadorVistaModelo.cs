using log4net;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Windows.Input;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla la logica de la ventana para reportar a un jugador.
    /// </summary>
    public class ReportarJugadorVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SonidoManejador _sonidoManejador;
        private readonly string _nombreJugador;
        private string _motivo;
        private string _mensajeError;

        public ReportarJugadorVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            SonidoManejador sonidoManejador,
            string nombreJugador)
            : base(ventana, localizador)
        {
            _nombreJugador = nombreJugador;
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            ReportarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                if (string.IsNullOrWhiteSpace(Motivo))
                {
                    MensajeError = Lang.reportarJugadorTextoMotivoRequerido;
                    return;
                }

                Logger.InfoFormat("Usuario confirmo reporte contra: {0}", 
                    _nombreJugador);
                MensajeError = string.Empty;
                DialogResult = true;
                _ventana.CerrarVentana(this);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                Logger.Info("Usuario cancelo el reporte de jugador.");
                DialogResult = false;
                _ventana.CerrarVentana(this);
            });
        }

        public string NombreJugador => _nombreJugador;

        public string DescripcionReporte => string.Format(
            Lang.reportarJugadorTextoDescripcion,
            _nombreJugador);

        public string Motivo
        {
            get => _motivo;
            set => EstablecerPropiedad(ref _motivo, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            private set => EstablecerPropiedad(ref _mensajeError, value);
        }

        public ICommand ReportarComando { get; }

        public ICommand CancelarComando { get; }

        public bool? DialogResult { get; private set; }

        public string MotivoReporte => Motivo;
    }
}