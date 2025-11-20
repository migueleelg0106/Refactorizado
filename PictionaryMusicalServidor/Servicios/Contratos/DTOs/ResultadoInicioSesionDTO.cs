using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta el resultado del intento de inicio de sesion.
    /// </summary>
    [DataContract]
    public class ResultadoInicioSesionDTO
    {
        /// <summary>
        /// Indica si el inicio de sesion fue exitoso.
        /// </summary>
        [DataMember]
        public bool InicioSesionExitoso { get; set; }

        /// <summary>
        /// Indica si la cuenta fue encontrada en el sistema.
        /// </summary>
        [DataMember]
        public bool CuentaEncontrada { get; set; }

        /// <summary>
        /// Indica si la contrasena proporcionada es incorrecta.
        /// </summary>
        [DataMember]
        public bool ContrasenaIncorrecta { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado del inicio de sesion.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }

        /// <summary>
        /// Informacion del usuario cuando el inicio de sesion es exitoso.
        /// </summary>
        [DataMember]
        public UsuarioDTO Usuario { get; set; }
    }
}