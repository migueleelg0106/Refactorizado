using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class RespuestaSolicitudAmistadNotificacionDTO
    {
        [DataMember]
        public string Remitente { get; set; }

        [DataMember]
        public string Receptor { get; set; }

        [DataMember]
        public bool Aceptada { get; set; }
    }
}
