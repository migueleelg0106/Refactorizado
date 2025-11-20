using System.Collections.Generic;
using System.ServiceModel;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalServidor.Servicios.Contratos
{
    /// <summary>
    /// Contrato de servicio para la consulta de clasificaciones de jugadores.
    /// </summary>
    [ServiceContract]
    public interface IClasificacionManejador
    {
        /// <summary>
        /// Obtiene la lista de los mejores jugadores ordenados por puntuacion.
        /// </summary>
        /// <returns>Lista de jugadores con sus clasificaciones.</returns>
        [OperationContract]
        IList<ClasificacionUsuarioDTO> ObtenerTopJugadores();
    }
}
