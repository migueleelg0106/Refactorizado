using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la informacion de un usuario.
    /// Contiene los datos de perfil y redes sociales de un usuario del sistema.
    /// </summary>
    [DataContract]
    public class UsuarioDTO
    {
        /// <summary>
        /// Identificador unico del usuario.
        /// </summary>
        [DataMember]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Identificador del jugador asociado al usuario.
        /// </summary>
        [DataMember]
        public int JugadorId { get; set; }

        /// <summary>
        /// Nombre de usuario para inicio de sesion.
        /// </summary>
        [DataMember]
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [DataMember]
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [DataMember]
        public string Apellido { get; set; }

        /// <summary>
        /// Correo electronico del usuario.
        /// </summary>
        [DataMember]
        public string Correo { get; set; }

        /// <summary>
        /// Identificador del avatar del usuario.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

        /// <summary>
        /// Nombre de usuario de Instagram (red social).
        /// </summary>
        [DataMember]
        public string Instagram { get; set; }

        /// <summary>
        /// Nombre de usuario de Facebook (red social).
        /// </summary>
        [DataMember]
        public string Facebook { get; set; }

        /// <summary>
        /// Nombre de usuario de X/Twitter (red social).
        /// </summary>
        [DataMember]
        public string X { get; set; }

        /// <summary>
        /// Nombre de usuario de Discord (red social).
        /// </summary>
        [DataMember]
        public string Discord { get; set; }
    }
}