using System.Runtime.Serialization;

namespace PictionaryMusical.Servicios.Contratos.DTOs
{
    [DataContract]
    public class AmigoDTO
    {
        [DataMember]
        public int UsuarioId { get; set; }

        [DataMember]
        public string NombreUsuario { get; set; }
    }
}