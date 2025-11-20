using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la informacion de un amigo.
    /// Contiene los datos basicos de identificacion de un usuario amigo.
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