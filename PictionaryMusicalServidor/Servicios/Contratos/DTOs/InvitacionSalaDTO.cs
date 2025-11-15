using System.Runtime.Serialization;

namespace PictionaryMusical.Servicios.Contratos.DTOs
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
