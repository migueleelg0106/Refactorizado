using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudAmistadEstadoDTO
    {
        [DataMember]
        public int RemitenteId { get; set; }

        [DataMember]
        public int DestinatarioId { get; set; }

        [DataMember]
        public bool Aceptada { get; set; }
    }
}
