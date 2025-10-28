using System;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
        private readonly BaseDatosPruebaEntities1 _contexto;

        public JugadorRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteCorreo(string correo)
        {
            return _contexto.Jugador.Any(j => j.Correo == correo);
        }

        public Jugador CrearJugador(Jugador jugador)
        {
            if (jugador == null)
            {
                throw new ArgumentNullException(nameof(jugador));
            }

            var entidad = _contexto.Jugador.Add(jugador);
            _contexto.SaveChanges();
            return entidad;
        }
    }
}

