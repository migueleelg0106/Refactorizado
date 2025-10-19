using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class OperacionAmistadDTO
    {
        [DataMember]
        public int JugadorId { get; set; }

        [DataMember]
        public int AmigoId { get; set; }
    }
}
