using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class SolicitudRecuperarCuentaDTO
    {
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }
    }
}