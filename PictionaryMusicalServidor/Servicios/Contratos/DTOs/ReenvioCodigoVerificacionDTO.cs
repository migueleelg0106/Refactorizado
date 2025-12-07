using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para solicitar el reenvio de un codigo de verificacion de
    /// registro.
    /// Contiene el token de sesion de verificacion.
    /// </summary>
    [DataContract]
    public class ReenvioCodigoVerificacionDTO
    {
        /// <summary>
        /// Token de identificacion de la sesion de verificacion (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }
    }
}