using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoOperacionDTO
    {
        [DataMember]
        public bool OperacionExitosa { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}