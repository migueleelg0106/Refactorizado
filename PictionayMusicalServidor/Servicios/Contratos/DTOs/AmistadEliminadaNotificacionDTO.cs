using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AmistadEliminadaNotificacionDTO
    {
        [DataMember]
        public string Jugador { get; set; }

        [DataMember]
        public string Amigo { get; set; }
    }
}
