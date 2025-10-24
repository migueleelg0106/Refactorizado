using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AvatarDTO
    {
        [DataMember]
        public int AvatarId { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string RutaRelativa { get; set; }
    }
}
