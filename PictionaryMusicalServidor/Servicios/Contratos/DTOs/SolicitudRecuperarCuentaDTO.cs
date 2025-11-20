using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion necesaria para solicitar la recuperacion de una cuenta.
    /// </summary>
    [DataContract]
    public class SolicitudRecuperarCuentaDTO
    {
        /// <summary>
        /// Identificador del usuario (nombre de usuario o correo electronico). Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }
    }
}