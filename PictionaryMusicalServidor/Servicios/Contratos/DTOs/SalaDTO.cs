using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion de una sala de juego.
    /// </summary>
    [DataContract]
    public class SalaDTO
    {
        /// <summary>
        /// Codigo unico de la sala.
        /// </summary>
        [DataMember]
        public string Codigo { get; set; }

        /// <summary>
        /// Nombre del usuario creador de la sala.
        /// </summary>
        [DataMember]
        public string Creador { get; set; }

        /// <summary>
        /// Configuracion de la partida asociada a la sala.
        /// </summary>
        [DataMember]
        public ConfiguracionPartidaDTO Configuracion { get; set; }

        /// <summary>
        /// Lista de nombres de usuarios que estan en la sala.
        /// </summary>
        [DataMember]
        public IList<string> Jugadores { get; set; }
    }
}
