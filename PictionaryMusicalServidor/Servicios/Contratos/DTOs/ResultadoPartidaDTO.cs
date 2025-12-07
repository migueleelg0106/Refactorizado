using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Representa el resultado final de una partida con la clasificacion de los jugadores.
    /// </summary>
    [DataContract]
    public class ResultadoPartidaDTO
    {
        /// <summary>
        /// Establece la lista de clasificacion de los usuarios participantes, ordenados
        /// por puntos y partidas ganadas.
        /// </summary>
        [DataMember]
        public List<ClasificacionUsuarioDTO> Clasificacion { get; set; }

        /// <summary>
        /// Establece el mensaje descriptivo o de estado relacionado con la finalizacion
        /// de la partida.
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }
    }
}
