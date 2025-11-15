using PictionaryMusical.Datos.Modelo;

namespace PictionaryMusical.Datos.DAL.Interfaces
{
    public interface IJugadorRepositorio
    {
        bool ExisteCorreo(string correo);

        Jugador CrearJugador(Jugador jugador);
    }
}
