using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudAmistadNotificacionDTO
    {
        [DataMember]
        public int RemitenteId { get; set; }

        [DataMember]
        public int DestinatarioId { get; set; }

        [DataMember]
        public AmigoDTO Remitente { get; set; }
    }
}
