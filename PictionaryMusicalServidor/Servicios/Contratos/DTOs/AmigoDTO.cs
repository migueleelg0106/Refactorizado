using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion basica de un amigo.
    /// </summary>
    [DataContract]
    public class AmigoDTO
    {
        /// <summary>
        /// Identificador unico del usuario amigo.
        /// </summary>
        [DataMember]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre de usuario del amigo.
        /// </summary>
        [DataMember]
        public string NombreUsuario { get; set; }
    }
}