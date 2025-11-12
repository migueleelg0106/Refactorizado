using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class InvitacionSalaDTO
    {
        [DataMember]
        public string CodigoSala { get; set; }

        [DataMember]
        public string Correo { get; set; }
    }
}
