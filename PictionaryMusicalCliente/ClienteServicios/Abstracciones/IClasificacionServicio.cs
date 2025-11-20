using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Provee acceso a las estadisticas globales y tablas de puntuacion del juego.
    /// </summary>
    public interface IClasificacionServicio
    {
        /// <summary>
        /// Recupera el listado de jugadores con mayores puntuaciones acumuladas.
        /// </summary>
        /// <returns>Lista de solo lectura con los datos de clasificacion.</returns>
        Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync();
    }
}