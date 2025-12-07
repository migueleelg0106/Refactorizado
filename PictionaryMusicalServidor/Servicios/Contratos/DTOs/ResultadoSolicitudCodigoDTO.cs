using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el resultado de una solicitud de codigo de 
    /// verificacion.
    /// Contiene informacion sobre el envio del codigo y posibles conflictos.
    /// </summary>
    [DataContract]
    public class ResultadoSolicitudCodigoDTO
    {
        /// <summary>
        /// Indica si el codigo de verificacion fue enviado exitosamente.
        /// </summary>
        [DataMember]
        public bool CodigoEnviado { get; set; }

        /// <summary>
        /// Indica si el nombre de usuario ya esta registrado en el sistema.
        /// </summary>
        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        /// <summary>
        /// Indica si el correo electronico ya esta registrado en el sistema.
        /// </summary>
        [DataMember]
        public bool CorreoRegistrado { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la solicitud.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }

        /// <summary>
        /// Token de sesion para confirmar el codigo posteriormente.
        /// </summary>
        [DataMember]
        public string TokenCodigo { get; set; }
    }
}