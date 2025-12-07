using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la informacion de una sala de juego.
    /// Contiene el codigo, creador, configuracion y lista de jugadores de una sala.
    /// </summary>
    [DataContract]
    public class SalaDTO
    {
        /// <summary>
        /// Codigo unico identificador de la sala.
        /// </summary>
        [DataMember]
        public string Codigo { get; set; }

        /// <summary>
        /// Nombre del usuario creador de la sala.
        /// </summary>
        [DataMember]
        public string Creador { get; set; }

        /// <summary>
        /// Configuracion de la partida para esta sala.
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
