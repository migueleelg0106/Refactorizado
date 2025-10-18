namespace Datos.DAL.Interfaces
{
    using Datos.Modelo;

    public interface IUsuarioRepositorio
    {
        bool ExisteNombreUsuario(string nombreUsuario);

        Usuario CrearUsuario(Usuario usuario);
    }
}
