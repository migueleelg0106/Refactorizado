using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using log4net;
using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;

namespace PictionaryMusicalCliente.VistaModelo.Ajustes
{
    /// <summary>
    /// Gestiona la logica para la confirmacion de salir de la partida.
    /// </summary>
    public class ConfirmacionSalirPartidaVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Action EjecutarSalidaPartidaYNavegacion { get; set; }

        public ConfirmacionSalirPartidaVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador)
            : base(ventana, localizador)
        {
            AceptarComando = new ComandoDelegado(_ => EjecutarAceptar());
            CancelarComando = new ComandoDelegado(_ => EjecutarCancelar());
        }

        /// <summary>
        /// Comando para confirmar la salida de la partida.
        /// </summary>
        public ICommand AceptarComando { get; }

        /// <summary>
        /// Comando para cancelar la operacion y mantener en la partida.
        /// </summary>
        public ICommand CancelarComando { get; }

        private void EjecutarAceptar()
        {
            _logger.Info("Usuario confirmo la salida de la partida.");
            EjecutarSalidaPartidaYNavegacion?.Invoke();
            _ventana.CerrarVentana(this);
        }

        private void EjecutarCancelar()
        {
            _logger.Info("Usuario cancelo la salida de la partida.");
            _ventana.CerrarVentana(this);
        }
    }
}
