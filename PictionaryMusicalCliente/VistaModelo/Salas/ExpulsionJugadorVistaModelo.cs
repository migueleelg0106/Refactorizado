using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using log4net;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla la logica de la ventana de confirmacion para expulsar un jugador.
    /// </summary>
    public class ExpulsionJugadorVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Inicializa el ViewModel con el mensaje de confirmacion.
        /// </summary>
        /// <param name="mensajeConfirmacion">Texto que se mostrara al usuario.</param>
        public ExpulsionJugadorVistaModelo(string mensajeConfirmacion)
        {
            MensajeConfirmacion = string.IsNullOrWhiteSpace(mensajeConfirmacion)
                ? Lang.expulsarTextoConfirmacion
                : mensajeConfirmacion;

            ConfirmarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                _logger.Info("Usuario confirmó la expulsión del jugador.");
                Cerrar?.Invoke(true);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                _logger.Info("Usuario canceló la expulsión del jugador.");
                Cerrar?.Invoke(false);
            });
        }

        /// <summary>
        /// Mensaje descriptivo sobre a quien se va a expulsar.
        /// </summary>
        public string MensajeConfirmacion { get; }

        /// <summary>
        /// Comando para proceder con la expulsion.
        /// </summary>
        public ICommand ConfirmarComando { get; }

        /// <summary>
        /// Comando para cancelar la operacion.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Accion para cerrar el dialogo retornando la decision del usuario.
        /// </summary>
        public Action<bool?> Cerrar { get; set; }
    }
}