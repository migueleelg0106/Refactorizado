using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion para reenviar un codigo de verificacion.
    /// </summary>
    [DataContract]
    public class ReenvioCodigoDTO
    {
        /// <summary>
        /// Token asociado al codigo de verificacion. Dato requerido.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TokenCodigo { get; set; }
    }
}