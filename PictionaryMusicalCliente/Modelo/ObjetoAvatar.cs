using System.Windows.Media;

namespace PictionaryMusicalCliente.Modelo
{
    public class ObjetoAvatar(
        int id,
        string nombre,
        ImageSource imagen)
    {

        public int Id { get; } = id;

        public string Nombre { get; } = nombre;

        public ImageSource Imagen { get; } = imagen;
    }
}
