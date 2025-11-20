using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion necesaria para registrar una nueva cuenta de usuario.
    /// </summary>
    [DataContract]
    public class NuevaCuentaDTO
    {
        /// <summary>
        /// Nombre de usuario unico para la cuenta. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Usuario { get; set; }

        /// <summary>
        /// Direccion de correo electronico del usuario. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Correo { get; set; }

        /// <summary>
        /// Nombre del jugador. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del jugador. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Apellido { get; set; }

        /// <summary>
        /// Contrasena para la cuenta. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }

        /// <summary>
        /// Identificador del avatar seleccionado por el usuario. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int AvatarId { get; set; }

    }
}
