using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
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
