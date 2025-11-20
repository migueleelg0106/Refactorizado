using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta el resultado de la solicitud de recuperacion de cuenta.
    /// </summary>
    [DataContract]
    public class ResultadoSolicitudRecuperacionDTO
    {
        /// <summary>
        /// Indica si la cuenta fue encontrada en el sistema.
        /// </summary>
        [DataMember]
        public bool CuentaEncontrada { get; set; }

        /// <summary>
        /// Indica si el codigo fue enviado exitosamente.
        /// </summary>
        [DataMember]
        public bool CodigoEnviado { get; set; }

        /// <summary>
        /// Correo electronico al que se envio el codigo de recuperacion.
        /// </summary>
        [DataMember]
        public string CorreoDestino { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la solicitud.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }

        /// <summary>
        /// Token asociado al codigo de recuperacion enviado.
        /// </summary>
        [DataMember]
        public string TokenCodigo { get; set; }
    }
}