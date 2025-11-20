using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta el resultado de una operacion generica.
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