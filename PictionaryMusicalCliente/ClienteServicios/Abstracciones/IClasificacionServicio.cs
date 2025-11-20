using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.ClienteServicios.Abstracciones
{
    /// <summary>
    /// Define el contrato para el servicio de consulta de clasificaciones.
    /// </summary>
    public interface IClasificacionServicio
    {
        /// <summary>
        /// Obtiene la lista de los mejores jugadores.
        /// </summary>
        /// <returns>Lista de jugadores ordenados por puntuacion.</returns>
        Task<IReadOnlyList<DTOs.ClasificacionUsuarioDTO>> ObtenerTopJugadoresAsync();
    }
}
