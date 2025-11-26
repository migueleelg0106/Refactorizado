using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Representa los datos de configuracion y rol de una ronda de juego para un participante.
    /// </summary>
    [DataContract]
    public class RondaDTO
    {
        /// <summary>
        /// Identificador de la cancion asignada a la ronda.
        /// </summary>
        [DataMember]
        public int IdCancion { get; set; }

        /// <summary>
        /// Rol del jugador en la ronda ("Dibujante" o "Adivinador").
        /// </summary>
        [DataMember]
        public string Rol { get; set; }

        /// <summary>
        /// Pista opcional con el nombre del artista de la cancion.
        /// </summary>
        [DataMember]
        public string PistaArtista { get; set; }

        /// <summary>
        /// Pista opcional con el genero de la cancion.
        /// </summary>
        [DataMember]
        public string PistaGenero { get; set; }

        /// <summary>
        /// Tiempo disponible para completar la ronda en segundos.
        /// </summary>
        [DataMember]
        public int TiempoSegundos { get; set; }

        /// <summary>
        /// Nombre del jugador que es el dibujante de la ronda.
        /// </summary>
        [DataMember]
        public string NombreDibujante { get; set; }
    }
}
