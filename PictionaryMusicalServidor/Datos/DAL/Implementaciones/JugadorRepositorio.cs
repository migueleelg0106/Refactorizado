using System;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JugadorRepositorio));
        private readonly BaseDatosPruebaEntities _contexto;

        public JugadorRepositorio(BaseDatosPruebaEntities contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public bool ExisteCorreo(string correo)
        {
            try
            {
                return _contexto.Jugador.Any(j => j.Correo == correo);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al verificar existencia del correo '{0}'.", correo), ex);
                throw;
            }
        }

        public Jugador CrearJugador(Jugador jugador)
        {
            if (jugador == null)
            {
                var ex = new ArgumentNullException(nameof(jugador));
                _logger.Error("Intento de crear un jugador nulo.", ex);
                throw ex;
            }

            try
            {
                var entidad = _contexto.Jugador.Add(jugador);
                _contexto.SaveChanges();

                _logger.InfoFormat("Jugador registrado exitosamente. Correo: {0}, ID: {1}.", entidad.Correo, entidad.idJugador);
                return entidad;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error al guardar el jugador con correo '{0}'.", jugador.Correo), ex);
                throw;
            }
        }
    }
}