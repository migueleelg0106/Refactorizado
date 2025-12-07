using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Datos requeridos para registrar un reporte de jugador.
    /// Incluye informacion del reportante, el jugador reportado y el motivo.
    /// </summary>
    [DataContract]
    public class ReporteJugadorDTO
    {
        /// <summary>
        /// Nombre del usuario que realiza el reporte.
        /// </summary>
        [DataMember]
        public string NombreUsuarioReportante { get; set; }

        /// <summary>
        /// Nombre del usuario que es reportado.
        /// </summary>
        [DataMember]
        public string NombreUsuarioReportado { get; set; }

        /// <summary>
        /// Motivo proporcionado por el usuario para realizar el reporte.
        /// </summary>
        [DataMember]
        public string Motivo { get; set; }
    }
}