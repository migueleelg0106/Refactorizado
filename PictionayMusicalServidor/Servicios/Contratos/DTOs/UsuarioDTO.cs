using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class UsuarioDTO
    {
        [DataMember]
        public int IdUsuario { get; set; }

        [DataMember]
        public int JugadorId { get; set; }

        [DataMember]
        public string NombreUsuario { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Apellido { get; set; }

        [DataMember]
        public string Correo { get; set; }

        [DataMember]
        public int AvatarId { get; set; }

        [DataMember]
        public string AvatarRutaRelativa { get; set; }

        [DataMember]
        public string Instagram { get; set; }

        [DataMember]
        public string Facebook { get; set; }

        [DataMember]
        public string X { get; set; }

        [DataMember]
        public string Discord { get; set; }
    }
}
