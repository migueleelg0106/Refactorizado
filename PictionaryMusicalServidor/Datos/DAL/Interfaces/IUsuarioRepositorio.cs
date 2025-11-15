namespace PictionaryMusical.Datos.DAL.Interfaces
{
    using PictionaryMusical.Datos.Modelo;

    public interface IUsuarioRepositorio
    {
        bool ExisteNombreUsuario(string nombreUsuario);

        Usuario CrearUsuario(Usuario usuario);

        Usuario ObtenerPorNombreUsuario(string nombreUsuario);
    }
}
