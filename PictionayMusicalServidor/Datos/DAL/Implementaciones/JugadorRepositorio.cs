using System;
using System.Linq;
using Datos.DAL.Interfaces;
using Datos.Modelo;

namespace Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
        private readonly BaseDatosPruebaEntities1 contexto;

        public JugadorRepositorio(BaseDatosPruebaEntities1 contexto)
        {
            this.contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteCorreo(string correo)
        {
            return contexto.Jugador.Any(j => j.Correo == correo);
        }

        public Jugador CrearJugador(Jugador jugador)
        {
            if (jugador == null)
            {
                throw new ArgumentNullException(nameof(jugador));
            }

            var entidad = contexto.Jugador.Add(jugador);
            contexto.SaveChanges();
            return entidad;
        }
    }
}
