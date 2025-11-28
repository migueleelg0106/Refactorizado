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
        private static readonly ISalasManejadorCallback CallbackNulo = new SalasCallbackNulo();
        private readonly object _sync = new();
        private readonly Dictionary<string, ISalasManejadorCallback> _callbacks = new(StringComparer.OrdinalIgnoreCase);

        public SalaInterna(string codigo, string creador, ConfiguracionPartidaDTO configuracion)
        {
            Codigo = codigo;
            Creador = creador;
            Configuracion = configuracion;
            Jugadores = new List<string>();
            PartidaIniciada = false;
        }

        public string Codigo { get; }
        public string Creador { get; }
        public ConfiguracionPartidaDTO Configuracion { get; }
        public List<string> Jugadores { get; }
        public bool DebeEliminarse { get; private set; }
        public bool PartidaIniciada { get; set; }

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
        public SalaDTO AgregarJugador(string nombreUsuario, ISalasManejadorCallback callback, bool notificar)
        {
            lock (_sync)
            {
                if (JugadorYaExiste(nombreUsuario))
                {
                    _callbacks[nombreUsuario] = callback;
                    _logger.InfoFormat("Jugador '{0}' se reconectó a la sala {1}.", nombreUsuario, Codigo);
                    return ToDto();
                }

                ValidarCapacidadSala();

                Jugadores.Add(nombreUsuario);
                _callbacks[nombreUsuario] = callback;
                _logger.InfoFormat("Jugador '{0}' se unió a la sala {1}. Total jugadores: {2}.", nombreUsuario, Codigo, Jugadores.Count);

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

                _logger.InfoFormat("Jugador '{0}' salió de la sala {1}.", nombreUsuario, Codigo);

                var salaActualizada = ToDto();
                NotificarSalidaYActualizacion(nombreUsuario, salaActualizada);

                bool anfitrionAbandono = string.Equals(
                    nombreUsuario,
                    Creador,
                    StringComparison.OrdinalIgnoreCase);

                if (anfitrionAbandono)
                {
                    NotificarCancelacionSala();
                    Jugadores.Clear();
                    _callbacks.Clear();
                    DebeEliminarse = true;
                    return;
                }

                if (Jugadores.Count == 0)
                {
                    _logger.InfoFormat("Marcando sala {0} para eliminación (Host salió o sala vacía).", Codigo);
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

                _logger.InfoFormat("Jugador '{0}' fue expulsado de la sala {1} por '{2}'.", nombreJugadorAExpulsar, Codigo, nombreHost);

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

        private void NotificarCancelacionSala()
        {
            foreach (var callback in _callbacks.Select(callbakJugador => callbakJugador.Value))
            {
                NotificarSalaCancelada(callback);
            }
        }

        private ISalasManejadorCallback ObtenerCallback(string nombreJugador)
        {
            if (_callbacks.TryGetValue(nombreJugador, out var callback))
            {
                return callback;
            }

            return CallbackNulo;
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
                _logger.Warn("Operación inválida en comunicación WCF. El canal no está en el estado correcto para la operación.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(logError, ex);
            }
        }

        private void NotificarJugadorSeUnio(ISalasManejadorCallback callback, string nombreJugador)
        {
            EjecutarNotificacion(
                () => callback.NotificarJugadorSeUnio(Codigo, nombreJugador),
                "Error al notificar la unión del jugador a la sala a través del callback.");
        }

        private void NotificarJugadorSalio(ISalasManejadorCallback callback, string nombreJugador)
        {
            EjecutarNotificacion(
                () => callback.NotificarJugadorSalio(Codigo, nombreJugador),
                "Error al notificar la salida del jugador de la sala a través del callback.");
        }

        private void NotificarJugadorExpulsado(ISalasManejadorCallback callback, string nombreJugador)
        {
            EjecutarNotificacion(
                () => callback.NotificarJugadorExpulsado(Codigo, nombreJugador),
                "Error al notificar la expulsión del jugador de la sala a través del callback.");
        }

        private void NotificarSalaCancelada(ISalasManejadorCallback callback)
        {
            EjecutarNotificacion(
                () => callback.NotificarSalaCancelada(Codigo),
                "Error al notificar la cancelación de la sala a través del callback.");
        }

        private static void NotificarSalaActualizada(ISalasManejadorCallback callback, SalaDTO salaActualizada)
        {
            EjecutarNotificacion(
                () => callback.NotificarSalaActualizada(salaActualizada),
                "Error al notificar la actualización de la sala a través del callback.");
        }

        private sealed class SalasCallbackNulo : ISalasManejadorCallback
        {
            public void NotificarJugadorSeUnio(string codigoSala, string nombreJugador)
            {
            }

            public void NotificarJugadorSalio(string codigoSala, string nombreJugador)
            {
            }

            public void NotificarListaSalasActualizada(SalaDTO[] salas)
            {
            }

            public void NotificarSalaActualizada(SalaDTO sala)
            {
            }

            public void NotificarJugadorExpulsado(string codigoSala, string nombreJugador)
            {
            }

            public void NotificarSalaCancelada(string codigoSala)
            {
            }
        }
    }
}