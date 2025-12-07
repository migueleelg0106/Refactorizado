using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el registro de una nueva cuenta de usuario.
    /// Contiene toda la informacion necesaria para crear una cuenta nueva en el sistema.
    /// </summary>
    [DataContract]
    public class NuevaCuentaDTO
    {
        /// <summary>
        /// Nombre de usuario para la nueva cuenta (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Usuario { get; set; }

        /// <summary>
        /// Correo electronico del usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Correo { get; set; }

        /// <summary>
        /// Nombre del usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Apellido { get; set; }

        /// <summary>
        /// Contrasena de la cuenta (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }

        /// <summary>
        /// Identificador del avatar seleccionado por el usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public int AvatarId { get; set; }

        /// <summary>
        /// Codigo de idioma seleccionado por el usuario.
        /// </summary>
        [DataMember]
        public string Idioma { get; set; }

    }
}
