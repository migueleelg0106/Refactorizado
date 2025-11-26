using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Representa el resultado final de una partida con la clasificación de los jugadores.
    /// </summary>
    [DataContract]
    public class ResultadoPartidaDTO
    {
        [DataMember]
        public List<ClasificacionUsuarioDTO> Clasificacion { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}
