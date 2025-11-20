using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion para confirmar un codigo de verificacion.
    /// </summary>
    [DataContract]
    public class ConfirmacionCodigoDTO
    {
        /// <summary>
        /// Token asociado al codigo de verificacion. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }

        /// <summary>
        /// Codigo ingresado por el usuario para verificacion. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CodigoIngresado { get; set; }
    }
}