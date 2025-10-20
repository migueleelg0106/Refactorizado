using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class AmistadEliminadaDTO
    {
        [DataMember]
        public string UsuarioA { get; set; }

        [DataMember]
        public string UsuarioB { get; set; }
    }
}
