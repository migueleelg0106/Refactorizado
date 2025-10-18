using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ReenviarCodigoDTO
    {
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }
    }
}
