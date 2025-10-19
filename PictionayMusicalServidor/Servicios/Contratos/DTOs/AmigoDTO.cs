using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AmigoDTO
    {
        [DataMember]
        public int IdJugador { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Apellido { get; set; }

        [DataMember]
        public string Correo { get; set; }

        [DataMember]
        public int AvatarId { get; set; }

        [DataMember]
        public string AvatarRutaRelativa { get; set; }
    }
}
