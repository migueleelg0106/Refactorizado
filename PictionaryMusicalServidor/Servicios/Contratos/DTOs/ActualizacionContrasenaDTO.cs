using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizacionContrasenaDTO
    {
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        [DataMember(IsRequired = true)]
        public string NuevaContrasena { get; set; }
    }
}
