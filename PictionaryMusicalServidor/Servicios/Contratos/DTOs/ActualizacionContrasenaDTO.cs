using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion para actualizar una contrasena.
    /// </summary>
    [DataContract]
    public class ActualizacionContrasenaDTO
    {
        /// <summary>
        /// Token asociado al codigo de verificacion de recuperacion. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        /// <summary>
        /// Nueva contrasena para la cuenta. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string NuevaContrasena { get; set; }
    }
}
