using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoRegistroCuentaDTO
    {
        [DataMember]
        public bool RegistroExitoso { get; set; }

        [DataMember]
        public bool UsuarioYaRegistrado { get; set; }

        [DataMember]
        public bool CorreoYaRegistrado { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}
