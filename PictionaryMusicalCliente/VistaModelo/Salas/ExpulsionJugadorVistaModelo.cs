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
    /// Controla la logica de la ventana de confirmacion para expulsar un jugador.
    /// </summary>
    public class ExpulsionJugadorVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SonidoManejador _sonidoManejador;

        public ExpulsionJugadorVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            SonidoManejador sonidoManejador,
            string mensajeConfirmacion)
            : base(ventana, localizador)
        {
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));

            MensajeConfirmacion = string.IsNullOrWhiteSpace(mensajeConfirmacion)
                ? Lang.expulsarTextoConfirmacion
                : mensajeConfirmacion;

            ConfirmarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _logger.Info("Usuario confirmo la expulsion del jugador.");
                DialogResult = true;
                _ventana.CerrarVentana(this);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                _sonidoManejador.ReproducirClick();
                _logger.Info("Usuario cancelo la expulsion del jugador.");
                DialogResult = false;
                _ventana.CerrarVentana(this);
            });
        }

        public string MensajeConfirmacion { get; }

        public ICommand ConfirmarComando { get; }

        public ICommand CancelarComando { get; }

        public bool? DialogResult { get; private set; }
    }
}