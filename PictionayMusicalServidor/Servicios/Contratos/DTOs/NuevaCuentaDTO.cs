using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class NuevaCuentaDTO
    {
        [DataMember(IsRequired = true)]
        public string Usuario { get; set; }

        [DataMember(IsRequired = true)]
        public string Correo { get; set; }

        [DataMember(IsRequired = true)]
        public string Nombre { get; set; }

        [DataMember(IsRequired = true)]
        public string Apellido { get; set; }

        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }

        [DataMember(IsRequired = true)]
        public string AvatarRutaRelativa { get; set; }

    }
}
