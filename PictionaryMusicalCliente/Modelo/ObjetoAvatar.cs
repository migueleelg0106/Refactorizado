using System.Windows.Media;

namespace PictionaryMusicalCliente.Modelo
{
    /// <summary>
    /// Encapsula la informacion visual y logica de un avatar disponible en el catalogo.
    /// </summary>
    public class ObjetoAvatar(
        int id,
        string nombre,
        ImageSource imagen)
    {
        /// <summary>
        /// Identificador unico del avatar en la base de datos.
        /// </summary>
        public int Id { get; } = id;

        /// <summary>
        /// Nombre descriptivo del avatar (ej. nombre del artista).
        /// </summary>
        public string Nombre { get; } = nombre;

        /// <summary>
        /// Recurso de imagen cargado en memoria para su visualizacion.
        /// </summary>
        public ImageSource Imagen { get; } = imagen;
    }
}