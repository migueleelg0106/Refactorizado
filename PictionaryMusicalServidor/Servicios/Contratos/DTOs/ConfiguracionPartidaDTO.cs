using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la configuracion de una partida.
    /// </summary>
    [DataContract]
    public class ConfiguracionPartidaDTO
    {
        /// <summary>
        /// Numero total de rondas de la partida.
        /// </summary>
        [DataMember]
        public int NumeroRondas { get; set; }

        /// <summary>
        /// Tiempo en segundos asignado por ronda.
        /// </summary>
        [DataMember]
        public int TiempoPorRondaSegundos { get; set; }

        /// <summary>
        /// Idioma de las canciones para la partida.
        /// </summary>
        [DataMember]
        public string IdiomaCanciones { get; set; }

        /// <summary>
        /// Nivel de dificultad de la partida.
        /// </summary>
        [DataMember]
        public string Dificultad { get; set; }
    }
}
