using System;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla la logica de la ventana de confirmacion para expulsar un jugador.
    /// </summary>
    public class ExpulsionJugadorVistaModelo : BaseVistaModelo
    {
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
                ManejadorSonido.ReproducirClick();
                Cerrar?.Invoke(true);
            });

            CancelarComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
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