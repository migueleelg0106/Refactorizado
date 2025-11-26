using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Representa el resultado de una ronda de juego finalizada.
    /// Contiene la cancion correcta y la lista de puntajes actualizados.
    /// </summary>
    [DataContract]
    public class ResultadoRondaDTO
    {
        /// <summary>
        /// Nombre de la cancion correcta de la ronda.
        /// </summary>
        [DataMember]
        public string CancionCorrecta { get; set; }

        /// <summary>
        /// Nombre del artista de la cancion correcta.
        /// </summary>
        [DataMember]
        public string ArtistaCancion { get; set; }

        /// <summary>
        /// Lista de puntajes actualizados de todos los jugadores.
        /// </summary>
        [DataMember]
        public List<ClasificacionUsuarioDTO> PuntajesActualizados { get; set; }
    }
}
