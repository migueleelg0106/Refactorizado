using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define las operaciones disponibles para gestionar reportes de jugadores.
    /// </summary>
    public interface IReportesServicio
    {
        /// <summary>
        /// Envia un reporte de jugador al servidor.
        /// </summary>
        /// <param name="reporte">Datos del reporte a enviar.</param>
        Task<DTOs.ResultadoOperacionDTO> ReportarJugadorAsync(DTOs.ReporteJugadorDTO reporte);
    }
}