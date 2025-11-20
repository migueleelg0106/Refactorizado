using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Transporta la informacion de clasificacion de un usuario.
    /// </summary>
    [DataContract]
    public class ClasificacionUsuarioDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [DataMember]
        public string Usuario { get; set; }

        /// <summary>
        /// Total de puntos acumulados por el usuario.
        /// </summary>
        [DataMember]
        public int Puntos { get; set; }

        /// <summary>
        /// Total de rondas ganadas por el usuario.
        /// </summary>
        [DataMember]
        public int RondasGanadas { get; set; }
    }
}