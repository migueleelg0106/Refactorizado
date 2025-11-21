using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using System;
using System.Windows.Input;
using log4net;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    /// <summary>
    /// ViewModel para el cuadro de dialogo de confirmacion de eliminacion de amigo.
    /// </summary>
    public class EliminacionAmigoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Inicializa el ViewModel construyendo el mensaje de confirmacion.
        /// </summary>
        /// <param name="nombreAmigo">Nombre del usuario a eliminar.</param>
        public EliminacionAmigoVistaModelo(string nombreAmigo)
        {
            MensajeConfirmacion = string.IsNullOrWhiteSpace(nombreAmigo)
                ? Lang.eliminarAmigoTextoConfirmacion
                : string.Concat(Lang.eliminarAmigoTextoConfirmacion, nombreAmigo, "?");

            AceptarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                _logger.InfoFormat("Usuario confirmó la eliminación del amigo: {0}",
                    nombreAmigo);
                Cerrar?.Invoke(true);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                SonidoManejador.ReproducirClick();
                _logger.InfoFormat("Usuario canceló la eliminación del amigo: {0}",
                    nombreAmigo);
                Cerrar?.Invoke(false);
            });
        }

        /// <summary>
        /// Mensaje completo a mostrar al usuario.
        /// </summary>
        public string MensajeConfirmacion { get; }

        /// <summary>
        /// Comando para confirmar la eliminacion.
        /// </summary>
        public ICommand AceptarComando { get; }

        /// <summary>
        /// Comando para cancelar la operacion.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Accion para cerrar el dialogo retornando el resultado (true=confirmado).
        /// </summary>
        public Action<bool?> Cerrar { get; set; }
    }
}