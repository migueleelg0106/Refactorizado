using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta las credenciales necesarias para iniciar sesion.
    /// </summary>
    [DataContract]
    public class CredencialesInicioSesionDTO
    {
        /// <summary>
        /// Identificador del usuario (nombre de usuario o correo electronico). Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }

        /// <summary>
        /// Contrasena del usuario. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }
    }
}
