using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para confirmar un codigo de verificacion.
    /// Contiene el token de sesion y el codigo ingresado por el usuario.
    /// </summary>
    [DataContract]
    public class ConfirmacionCodigoDTO
    {
        /// <summary>
        /// Token de identificacion de la sesion de verificacion (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        /// <summary>
        /// Codigo de verificacion ingresado por el usuario (dato requerido).
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CodigoIngresado { get; set; }
    }
}