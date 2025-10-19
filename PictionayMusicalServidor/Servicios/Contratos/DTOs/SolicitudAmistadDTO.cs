using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudAmistadDTO
    {
        [DataMember]
        public int RemitenteId { get; set; }

        [DataMember]
        public int DestinatarioId { get; set; }
    }
}
