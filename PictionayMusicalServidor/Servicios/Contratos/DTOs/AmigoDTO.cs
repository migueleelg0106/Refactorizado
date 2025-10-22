using System.Runtime.Serialization;
namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AmigoDTO
    {
        [DataMember]
        public int IdUsuario { get; set; }

        [DataMember]
        public string NombreUsuario { get; set; }
    }
}
