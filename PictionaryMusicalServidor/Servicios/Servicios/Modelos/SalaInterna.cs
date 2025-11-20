using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios.Modelos
{
    /// <summary>
    /// Representa una sala de juego interna con su estado y jugadores.
    /// Gestiona la lógica de ingreso, salida y expulsión de jugadores.
    /// </summary>
    internal sealed class SalaInterna
    {
        private const int MaximoJugadores = 4;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalaInterna));
        private readonly object _sync = new();
        private readonly Dictionary<string, ISalasCallback> _callbacks = new(StringComparer.OrdinalIgnoreCase);

        public SalaInterna(string codigo, string creador, ConfiguracionPartidaDTO configuracion)
        {
            Codigo = codigo;
            Creador = creador;
            Configuracion = configuracion;
            Jugadores = new List<string>();
        }

        public string Codigo { get; }
        public string Creador { get; }
        public ConfiguracionPartidaDTO Configuracion { get; }
        public List<string> Jugadores { get; }
        public bool DebeEliminarse { get; private set; }

        /// <summary>
        /// Convierte la sala interna a su representación DTO.
        /// </summary>
        public SalaDTO ToDto()
        {
            lock (_sync)
            {
                return new SalaDTO
                {
                    Codigo = Codigo,
                    Creador = Creador,
                    Configuracion = Configuracion,
                    Jugadores = new List<string>(Jugadores)
                };
            }
        }

        /// <summary>
        /// Agrega un jugador a la sala.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del jugador a agregar.</param>
        /// <param name="callback">Callback del jugador.</param>
        /// <param name="notificar">Indica si se debe notificar a otros jugadores.</param>
        /// <returns>Estado actualizado de la sala.</returns>
        public SalaDTO AgregarJugador(string nombreUsuario, ISalasCallback callback, bool notificar)
        {
            lock (_sync)
            {
                if (JugadorYaExiste(nombreUsuario))
                {
                    _callbacks[nombreUsuario] = callback;
                    _logger.Info($"Jugador '{nombreUsuario}' se reconectó a la sala {Codigo}.");
                    return ToDto();
                }

                ValidarCapacidadSala();

                Jugadores.Add(nombreUsuario);
                _callbacks[nombreUsuario] = callback;
                _logger.Info($"Jugador '{nombreUsuario}' se unió a la sala {Codigo}. Total jugadores: {Jugadores.Count}.");

                if (notificar)
                {
                    var salaActualizada = ToDto();
                    NotificarNuevoJugadorYActualizacion(nombreUsuario, salaActualizada);
                }

                return ToDto();
            }
        }

        /// <summary>
        /// Remueve un jugador de la sala.
        /// </summary>
        /// <param name="nombreUsuario">Nombre del jugador a remover.</param>
        public void RemoverJugador(string nombreUsuario)
        {
            lock (_sync)
            {
                if (!RemoverJugadorDeSala(nombreUsuario))
                {
                    return;
                }

                _logger.Info($"Jugador '{nombreUsuario}' salió de la sala {Codigo}.");

                var salaActualizada = ToDto();
                NotificarSalidaYActualizacion(nombreUsuario, salaActualizada);

                if (DebeMarcarseParaEliminar(nombreUsuario))
                {
                    _logger.Info($"Marcando sala {Codigo} para eliminación (Host salió o sala vacía).");
                    DebeEliminarse = true;
                }
            }
        }

        /// <summary>
        /// Expulsa un jugador de la sala.
        /// </summary>
        /// <param name="nombreHost">Nombre del host que expulsa.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        public void ExpulsarJugador(string nombreHost, string nombreJugadorAExpulsar)
        {
            lock (_sync)
            {
                ValidarExpulsion(nombreHost, nombreJugadorAExpulsar);

                var callbackExpulsado = ObtenerCallback(nombreJugadorAExpulsar);

                Jugadores.RemoveAll(j => string.Equals(j, nombreJugadorAExpulsar, StringComparison.OrdinalIgnoreCase));
                _callbacks.Remove(nombreJugadorAExpulsar);

                _logger.Info($"Jugador '{nombreJugadorAExpulsar}' fue expulsado de la sala {Codigo} por '{nombreHost}'.");

                var salaActualizada = ToDto();

                NotificarJugadorExpulsado(callbackExpulsado, nombreJugadorAExpulsar);
                NotificarSalidaYActualizacion(nombreJugadorAExpulsar, salaActualizada);
            }
        }

        private bool JugadorYaExiste(string nombreUsuario)
        {
            return Jugadores.Contains(nombreUsuario, StringComparer.OrdinalIgnoreCase);
        }

        private void ValidarCapacidadSala()
        {
            if (ContarJugadoresActivos() >= MaximoJugadores)
            {
                throw new FaultException(MensajesError.Cliente.SalaLlena);
            }
        }

        private int ContarJugadoresActivos()
        {
            return Jugadores.Count;
        }

        private bool RemoverJugadorDeSala(string nombreUsuario)
        {
            if (Jugadores.RemoveAll(j => string.Equals(j, nombreUsuario, StringComparison.OrdinalIgnoreCase)) == 0)
            {
                return false;
            }

            _callbacks.Remove(nombreUsuario);
            return true;
        }

        private bool DebeMarcarseParaEliminar(string nombreUsuario)
        {
            return string.Equals(nombreUsuario, Creador, StringComparison.OrdinalIgnoreCase)
                || Jugadores.Count == 0;
        }

        private ISalasCallback ObtenerCallback(string nombreJugador)
        {
            if (_callbacks.TryGetValue(nombreJugador, out var callback))
            {
                return callback;
            }

            return null;
        }

        private void ValidarExpulsion(string nombreHost, string nombreJugadorAExpulsar)
        {
            if (!string.Equals(nombreHost, Creador, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException(MensajesError.Cliente.SalaExpulsionRestringida);
            }

            if (string.Equals(nombreJugadorAExpulsar, Creador, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException(MensajesError.Cliente.SalaCreadorNoExpulsable);
            }

            if (!Jugadores.Contains(nombreJugadorAExpulsar, StringComparer.OrdinalIgnoreCase))
            {
                throw new FaultException(MensajesError.Cliente.SalaJugadorNoExiste);
            }
        }

        private void NotificarNuevoJugadorYActualizacion(string nombreUsuario, SalaDTO salaActualizada)
        {
            foreach (var callback in _callbacks
                .Where(callbakJugador => !string.Equals(callbakJugador.Key, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                .Select(callbakJugador => callbakJugador.Value))
            {
                NotificarJugadorSeUnio(callback, nombreUsuario);
            }

            foreach (var callback in _callbacks.Select(callbakJugador => callbakJugador.Value))
            {
                NotificarSalaActualizada(callback, salaActualizada);
            }
        }

        private void NotificarSalidaYActualizacion(string nombreJugador, SalaDTO salaActualizada)
        {
            foreach (var callback in _callbacks.Select(callbakJugador => callbakJugador.Value))
            {
                NotificarJugadorSalio(callback, nombreJugador);
                NotificarSalaActualizada(callback, salaActualizada);
            }
        }

        private static void EjecutarNotificacion(Action accionNotificacion, string logError)
        {
            try
            {
                accionNotificacion();
            }
            catch (CommunicationException ex)
            {
                _logger.Warn(logError, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(logError, ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(logError, ex);
            }
        }

        private void NotificarJugadorSeUnio(ISalasCallback callback, string nombreJugador)
        {
            EjecutarNotificacion(
                () => callback.NotificarJugadorSeUnio(Codigo, nombreJugador),
                MensajesError.Log.SalaNotificarJugadorUnionError);
        }

        private void NotificarJugadorSalio(ISalasCallback callback, string nombreJugador)
        {
            EjecutarNotificacion(
                () => callback.NotificarJugadorSalio(Codigo, nombreJugador),
                MensajesError.Log.SalaNotificarJugadorSalidaError);
        }

        private void NotificarJugadorExpulsado(ISalasCallback callback, string nombreJugador)
        {
            if (callback == null)
            {
                return;
            }

            EjecutarNotificacion(
                () => callback.NotificarJugadorExpulsado(Codigo, nombreJugador),
                MensajesError.Log.SalaNotificarJugadorExpulsionError);
        }

        private static void NotificarSalaActualizada(ISalasCallback callback, SalaDTO salaActualizada)
        {
            EjecutarNotificacion(
                () => callback.NotificarSalaActualizada(salaActualizada),
                MensajesError.Log.SalaNotificarJugadorActualizacionError);
        }
    }
}