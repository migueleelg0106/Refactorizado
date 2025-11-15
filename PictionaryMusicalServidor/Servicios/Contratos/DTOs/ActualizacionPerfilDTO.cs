using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    [DataContract]
    public class ActualizacionPerfilDTO
    {
        [DataMember]
        public int UsuarioId { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Apellido { get; set; }

        [DataMember]
        public int AvatarId { get; set; }

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