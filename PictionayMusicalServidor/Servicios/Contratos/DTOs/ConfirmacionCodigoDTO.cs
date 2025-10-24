using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ConfirmacionCodigoDTO
    {
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        [DataMember(IsRequired = true)]
        public string CodigoIngresado { get; set; }
    }
}