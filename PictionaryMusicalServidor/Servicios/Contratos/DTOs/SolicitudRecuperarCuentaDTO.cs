using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para solicitar la recuperacion de una cuenta.
    /// Contiene el identificador del usuario que solicita recuperar su cuenta.
    /// </summary>
    [DataContract]
    public class SolicitudRecuperarCuentaDTO
    {
        /// <summary>
        /// Identificador del usuario (nombre de usuario o correo electronico).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Identificador { get; set; }

        /// <summary>
        /// Codigo de idioma solicitado para los mensajes de recuperacion.
        /// </summary>
        [DataMember]
        public string Idioma { get; set; }
    }
}