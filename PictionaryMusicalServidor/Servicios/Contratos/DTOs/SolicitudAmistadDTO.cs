using System.Runtime.Serialization;
namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para una solicitud de amistad.
    /// Contiene informacion sobre el emisor, receptor y estado de una solicitud de amistad.
    /// </summary>
    [DataContract]
    public class SolicitudAmistadDTO
    {
        /// <summary>
        /// Nombre del usuario que envia la solicitud de amistad.
        /// </summary>
        [DataMember]
        public string UsuarioEmisor { get; set; }

        /// <summary>
        /// Nombre del usuario que recibe la solicitud de amistad.
        /// </summary>
        [DataMember]
        public string UsuarioReceptor { get; set; }

        /// <summary>
        /// Indica si la solicitud de amistad fue aceptada.
        /// </summary>
        [DataMember]
        public bool SolicitudAceptada { get; set; }
    }
}