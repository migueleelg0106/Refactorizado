using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para actualizar la contrasena de un usuario.
    /// Contiene el token de verificacion y la nueva contrasena.
    /// </summary>
    [DataContract]
    public class ActualizacionContrasenaDTO
    {
        /// <summary>
        /// Token de identificacion de la sesion de recuperacion (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        /// <summary>
        /// Nueva contrasena del usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string NuevaContrasena { get; set; }
    }
}
