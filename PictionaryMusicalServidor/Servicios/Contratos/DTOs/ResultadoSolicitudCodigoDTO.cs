using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta el resultado de la solicitud de un codigo de verificacion.
    /// </summary>
    [DataContract]
    public class ResultadoSolicitudCodigoDTO
    {
        /// <summary>
        /// Indica si el codigo fue enviado exitosamente.
        /// </summary>
        [DataMember]
        public bool CodigoEnviado { get; set; }

        /// <summary>
        /// Indica si el nombre de usuario ya esta registrado.
        /// </summary>
        [DataMember]
        public bool UsuarioRegistrado { get; set; }

        /// <summary>
        /// Indica si el correo electronico ya esta registrado.
        /// </summary>
        [DataMember]
        public bool CorreoRegistrado { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la solicitud.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }

        /// <summary>
        /// Token asociado al codigo de verificacion enviado.
        /// </summary>
        [DataMember]
        public string TokenCodigo { get; set; }
    }
}