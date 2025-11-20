using System.ServiceModel;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de amistades entre usuarios.
    /// Proporciona operaciones para enviar, responder y gestionar solicitudes de amistad con soporte de callbacks.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IAmigosManejadorCallback))]
    public interface IAmigosManejador
    {
        /// <summary>
        /// Suscribe un usuario para recibir notificaciones de solicitudes de amistad.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario a suscribir.</param>
        [OperationContract]
        void Suscribir(string nombreUsuario);

        /// <summary>
        /// Cancela la suscripcion de un usuario de notificaciones de amistad.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del usuario que cancela la suscripcion.</param>
        [OperationContract]
        void CancelarSuscripcion(string nombreUsuario);

        /// <summary>
        /// Envia una solicitud de amistad de un usuario a otro.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envia la solicitud.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que recibe la solicitud.</param>
        [OperationContract]
        void EnviarSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        /// <summary>
        /// Responde una solicitud de amistad aceptandola.
        /// </summary>
        /// <param name="nombreUsuarioEmisor">Nombre del usuario que envio la solicitud original.</param>
        /// <param name="nombreUsuarioReceptor">Nombre del usuario que responde la solicitud.</param>
        [OperationContract]
        void ResponderSolicitudAmistad(string nombreUsuarioEmisor, string nombreUsuarioReceptor);

        /// <summary>
        /// Elimina la relacion de amistad entre dos usuarios.
        /// </summary>
        /// <param name="nombreUsuarioA">Nombre del primer usuario.</param>
        /// <param name="nombreUsuarioB">Nombre del segundo usuario.</param>
        [OperationContract]
        void EliminarAmigo(string nombreUsuarioA, string nombreUsuarioB);
    }
}
