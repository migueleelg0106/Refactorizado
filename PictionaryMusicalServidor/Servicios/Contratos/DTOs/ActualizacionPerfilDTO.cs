using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para actualizar el perfil de un usuario.
    /// Contiene los datos actualizables del perfil incluyendo redes sociales.
    /// </summary>
    [DataContract]
    public class ActualizacionPerfilDTO
    {
        /// <summary>
        /// Identificador unico del usuario cuyo perfil se actualiza.
        /// </summary>
        [DataMember]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre actualizado del usuario.
        /// </summary>
        [DataMember]
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido actualizado del usuario.
        /// </summary>
        [DataMember]
        public string Apellido { get; set; }

        /// <summary>
        /// Identificador del nuevo avatar del usuario.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

        /// <summary>
        /// Nombre de usuario actualizado de Instagram.
        /// </summary>
        [DataMember]
        public string Instagram { get; set; }

        /// <summary>
        /// Nombre de usuario actualizado de Facebook.
        /// </summary>
        [DataMember]
        public string Facebook { get; set; }

        /// <summary>
        /// Nombre de usuario actualizado de X/Twitter.
        /// </summary>
        [DataMember]
        public string X { get; set; }

        /// <summary>
        /// Nombre de usuario actualizado de Discord.
        /// </summary>
        [DataMember]
        public string Discord { get; set; }
    }
}