using System.Windows.Media;

namespace PictionaryMusicalCliente.Modelo
{
    public class ObjetoAvatar
    {
        public ObjetoAvatar(int id, string nombre, ImageSource imagen)
        {
            Id = id;
            Nombre = nombre;
            Imagen = imagen;
        }

        public int Id { get; }

        public string Nombre { get; }

        public ImageSource Imagen { get; }
    }
}
