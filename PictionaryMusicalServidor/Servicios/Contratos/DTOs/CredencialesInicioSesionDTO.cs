using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para las credenciales de inicio de sesion.
    /// Contiene la informacion necesaria para autenticar un usuario en el sistema.
    /// </summary>
    [DataContract]
    public class CredencialesInicioSesionDTO
    {
        /// <summary>
        /// Identificador del usuario (nombre de usuario o correo electronico) (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }

        /// <summary>
        /// Contrasena del usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Contrasena { get; set; }
    }
}
