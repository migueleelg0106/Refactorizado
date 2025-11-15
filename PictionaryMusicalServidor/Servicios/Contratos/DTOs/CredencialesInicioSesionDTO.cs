using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class CredencialesInicioSesionDTO
    {
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }

        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }
    }
}
