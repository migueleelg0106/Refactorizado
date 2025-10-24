using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ResultadoInicioSesionDTO
    {
        [DataMember]
        public bool InicioSesionExitoso { get; set; }

        [DataMember]
        public bool CuentaEncontrada { get; set; }

        [DataMember]
        public bool ContrasenaIncorrecta { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public UsuarioDTO Usuario { get; set; }
    }
}