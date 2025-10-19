using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudAmistadNotificacionDTO
    {
        [DataMember]
        public string Remitente { get; set; }

        [DataMember]
        public string Receptor { get; set; }
    }
}
