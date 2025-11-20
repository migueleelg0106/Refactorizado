using System;
using System.Linq;
using log4net;
using PictionaryMusicalServidor.Datos.DAL.Interfaces;
using PictionaryMusicalServidor.Datos.Modelo;

namespace PictionaryMusicalServidor.Datos.DAL.Implementaciones
{
    public class JugadorRepositorio : IJugadorRepositorio
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JugadorRepositorio));
        private readonly BaseDatosPruebaEntities1 _contexto;

        public JugadorRepositorio(BaseDatosPruebaEntities1 contexto)
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
                _logger.Error($"Error al verificar existencia del correo '{correo}'.", ex);
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

                _logger.Info($"Jugador registrado exitosamente. Correo: {entidad.Correo}, ID: {entidad.idJugador}.");
                return entidad;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error al guardar el jugador con correo '{jugador.Correo}'.", ex);
                throw;
            }
        }
    }
}