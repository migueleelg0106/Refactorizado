using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Interfaces
{
    public interface IJugadorRepositorio
    {
        bool ExisteCorreo(string correo);

        Jugador CrearJugador(Jugador jugador);
    }
}
