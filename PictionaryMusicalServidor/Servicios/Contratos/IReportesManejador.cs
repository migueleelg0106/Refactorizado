using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la gestion de reportes de jugadores.
    /// Permite registrar reportes desde el cliente.
    /// </summary>
    [ServiceContract]
    public interface IReportesManejador
    {
        /// <summary>
        /// Crea un nuevo reporte hacia un jugador especificado.
        /// </summary>
        /// <param name="reporte">Datos del reporte a registrar.</param>
        /// <returns>Resultado de la operacion indicando exito o fallo.</returns>
        [OperationContract]
        ResultadoOperacionDTO ReportarJugador(ReporteJugadorDTO reporte);
    }
}