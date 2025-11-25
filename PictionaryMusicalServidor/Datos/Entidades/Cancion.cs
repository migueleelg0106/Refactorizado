namespace PictionaryMusicalServidor.Datos.Entidades
{
    /// <summary>
    /// Representa la informacion de una cancion disponible para las rondas del juego.
    /// </summary>
    public class Cancion
    {
        /// <summary>
        /// Identificador unico de la cancion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre oficial de la cancion.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Nombre normalizado para comparaciones sin acentos ni mayusculas.
        /// </summary>
        public string NombreNormalizado { get; set; }

        /// <summary>
        /// Artista principal de la cancion.
        /// </summary>
        public string Artista { get; set; }

        /// <summary>
        /// Genero musical principal de la cancion.
        /// </summary>
        public string Genero { get; set; }

        /// <summary>
        /// Idioma principal de la letra de la cancion.
        /// </summary>
        public string Idioma { get; set; }
    }
}
