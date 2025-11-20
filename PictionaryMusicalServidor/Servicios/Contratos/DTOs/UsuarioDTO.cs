using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion completa de un usuario.
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
        /// Identificador unico del jugador asociado al usuario.
        /// </summary>
        [DataMember]
        public int JugadorId { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        [DataMember]
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Nombre del jugador.
        /// </summary>
        [DataMember]
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del jugador.
        /// </summary>
        [DataMember]
        public string Apellido { get; set; }

        /// <summary>
        /// Direccion de correo electronico del usuario.
        /// </summary>
        [DataMember]
        public string Correo { get; set; }

        /// <summary>
        /// Identificador del avatar seleccionado.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

        /// <summary>
        /// Cuenta de Instagram del usuario.
        /// </summary>
        [DataMember]
        public string Instagram { get; set; }

        /// <summary>
        /// Cuenta de Facebook del usuario.
        /// </summary>
        [DataMember]
        public string Facebook { get; set; }

        /// <summary>
        /// Cuenta de X (anteriormente Twitter) del usuario.
        /// </summary>
        [DataMember]
        public string X { get; set; }

        /// <summary>
        /// Cuenta de Discord del usuario.
        /// </summary>
        [DataMember]
        public string Discord { get; set; }
    }
}