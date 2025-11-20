using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la configuracion de una partida.
    /// Contiene los parametros de configuracion que definen como se jugara una partida.
    /// </summary>
    [DataContract]
    public class ConfiguracionPartidaDTO
    {
        /// <summary>
        /// Numero de rondas que tendra la partida.
        /// </summary>
        [DataMember]
        public int NumeroRondas { get; set; }

        /// <summary>
        /// Duracion en segundos de cada ronda de la partida.
        /// </summary>
        [DataMember]
        public int TiempoPorRondaSegundos { get; set; }

        /// <summary>
        /// Idioma de las canciones que se usaran en la partida.
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
