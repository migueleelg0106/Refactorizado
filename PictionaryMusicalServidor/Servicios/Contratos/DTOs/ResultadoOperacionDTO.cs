using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para el resultado de operaciones genericas.
    /// Proporciona informacion sobre el exito o fallo de una operacion con su mensaje asociado.
    /// </summary>
    [DataContract]
    public class ResultadoOperacionDTO
    {
        /// <summary>
        /// Indica si la operacion se completo exitosamente.
        /// </summary>
        [DataMember]
        public bool OperacionExitosa { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado de la operacion.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }
    }
}