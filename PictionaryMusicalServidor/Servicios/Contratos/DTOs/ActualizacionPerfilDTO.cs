using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion para actualizar el perfil de un usuario.
    /// </summary>
    [DataContract]
    public class ActualizacionPerfilDTO
    {
        /// <summary>
        /// Identificador unico del usuario.
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