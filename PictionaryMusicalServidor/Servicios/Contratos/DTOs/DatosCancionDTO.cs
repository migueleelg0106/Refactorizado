using System.Runtime.Serialization;

namespace PictionaryMusicalServidor.Servicios.Contratos.DTOs
{
    /// <summary>
    /// Objeto de transferencia de datos para la informacion de una cancion.
    /// Contiene los datos basicos de una cancion para el juego.
    /// </summary>
    [DataContract]
    public class DatosCancionDTO
    {
        /// <summary>
        /// Identificador unico de la cancion.
        /// </summary>
        [DataMember]
        public int IdCancion { get; set; }

        /// <summary>
        /// Nombre o titulo de la cancion.
        /// </summary>
        [DataMember]
        public string Nombre { get; set; }

        /// <summary>
        /// Nombre del artista de la cancion.
        /// </summary>
        [DataMember]
        public string Artista { get; set; }

        /// <summary>
        /// Genero musical de la cancion.
        /// </summary>
        [DataMember]
        public string Genero { get; set; }

        /// <summary>
        /// Ruta del archivo de audio de la cancion.
        /// </summary>
        [DataMember]
        public string RutaArchivo { get; set; }
    }
}
