using System.Windows.Media;

namespace PictionaryMusicalCliente.Modelo
{
    public class ObjetoAvatar
    {
        public ObjetoAvatar(int id, string nombre, ImageSource imagen)
            : this(id, nombre, imagen, null, null)
        {
        }

        public ObjetoAvatar(
            int id,
            string nombre,
            ImageSource imagen,
            string rutaRelativa,
            string imagenUriAbsoluta)
        {
            Id = id;
            Nombre = nombre;
            Imagen = imagen;
            RutaRelativa = rutaRelativa;
            ImagenUriAbsoluta = imagenUriAbsoluta;
        }

        public int Id { get; }

        public string Nombre { get; }

        public ImageSource Imagen { get; }

        public string RutaRelativa { get; }

        public string ImagenUriAbsoluta { get; }
    }
}
