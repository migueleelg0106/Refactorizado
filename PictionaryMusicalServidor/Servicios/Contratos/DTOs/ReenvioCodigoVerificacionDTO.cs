using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ReenvioCodigoVerificacionDTO
    {
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }
    }
}