namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    using PictionaryMusicalServidor.Datos.Modelo;

    public interface IUsuarioRepositorio
    {
        bool ExisteNombreUsuario(string nombreUsuario);

        Usuario CrearUsuario(Usuario usuario);

        Usuario ObtenerPorNombreUsuario(string nombreUsuario);
    }
}
