using System.Runtime.Serialization;

namespace Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizarPerfilDTO
    {
        [DataMember]
        public int UsuarioId { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Apellido { get; set; }

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
