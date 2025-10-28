using Datos.Modelo;

namespace Datos.DAL.Interfaces
{
    public interface IJugadorRepositorio
    {
        bool ExisteCorreo(string correo);

        Jugador CrearJugador(Jugador jugador);
    }
}
