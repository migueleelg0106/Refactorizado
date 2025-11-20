using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la clasificacion de un usuario.
    /// Contiene las estadisticas de juego de un usuario para el ranking.
    /// </summary>
    [DataContract]
    public class ClasificacionUsuarioDTO
    {
        /// <summary>
        /// Nombre de usuario del jugador clasificado.
        /// </summary>
        [DataMember]
        public string Usuario { get; set; }

        /// <summary>
        /// Puntos totales acumulados por el usuario.
        /// </summary>
        [DataMember]
        public int Puntos { get; set; }

        /// <summary>
        /// Numero de rondas ganadas por el usuario.
        /// </summary>
        [DataMember]
        public int RondasGanadas { get; set; }
    }
}