using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el resultado de una solicitud de recuperacion de 
    /// cuenta.
    /// Contiene informacion sobre si la cuenta fue encontrada y si el codigo fue enviado.
    /// </summary>
    [DataContract]
    public class ResultadoSolicitudRecuperacionDTO
    {
        /// <summary>
        /// Indica si la cuenta del usuario fue encontrada en el sistema.
        /// </summary>
        [DataMember]
        public bool CuentaEncontrada { get; set; }

        /// <summary>
        /// Indica si el codigo de recuperacion fue enviado exitosamente.
        /// </summary>
        [DataMember]
        public bool CodigoEnviado { get; set; }

        /// <summary>
        /// Correo electronico de destino donde se envio el codigo.
        /// </summary>
        [DataMember]
        public string CorreoDestino { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la solicitud.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }

        /// <summary>
        /// Token de sesion para confirmar el codigo posteriormente.
        /// </summary>
        [DataMember]
        public string TokenCodigo { get; set; }
    }
}