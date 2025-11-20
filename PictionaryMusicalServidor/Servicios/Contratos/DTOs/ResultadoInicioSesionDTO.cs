using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el resultado de un intento de inicio de sesion.
    /// Contiene informacion sobre el exito o fallo del inicio de sesion y datos del usuario autenticado.
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
        /// Indica si la cuenta del usuario fue encontrada en el sistema.
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
        /// Datos del usuario autenticado si el inicio de sesion fue exitoso.
        /// </summary>
        [DataMember]
        public UsuarioDTO Usuario { get; set; }
    }
}