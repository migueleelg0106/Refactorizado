using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
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