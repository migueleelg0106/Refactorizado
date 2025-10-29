using System.Windows.Media;

namespace PictionaryMusicalCliente.Modelo
{
    public class ObjetoAvatar(
        int id,
        string nombre,
        ImageSource imagen,
        string rutaRelativa,
        string imagenUriAbsoluta)
    {
        public ObjetoAvatar(int id, string nombre, ImageSource imagen)
            : this(id, nombre, imagen, null, null)
        {
        }

        public int Id { get; } = id;

        public string Nombre { get; } = nombre;

        public ImageSource Imagen { get; } = imagen;

        public string RutaRelativa { get; } = rutaRelativa;

        public string ImagenUriAbsoluta { get; } = imagenUriAbsoluta;
    }
}
