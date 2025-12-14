using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    /// <summary>
    /// Representa una sala de juego interna con su estado y jugadores.
    /// Gestiona el estado de los jugadores y delega las notificaciones al gestor inyectado.
    /// </summary>
    internal sealed class SalaInternaManejador
    {
        private const int MaximoJugadores = 4;

        private static readonly ILog _logger =
            LogManager.GetLogger(typeof(SalaInternaManejador));

        private readonly object _sincrono = new object();
        private readonly IGestorNotificacionesSalaInterna _gestorNotificaciones;
        private readonly List<string> _jugadores;

        /// <summary>
        /// Inicializa una nueva instancia de la sala interna.
        /// </summary>
        /// <param name="codigo">Codigo unico de la sala.</param>
        /// <param name="creador">Nombre de usuario del creador.</param>
        /// <param name="configuracion">Configuracion de la partida.</param>
        /// <param name="gestorNotificaciones">Dependencia para el manejo de notificaciones.
        /// </param>
        public SalaInternaManejador(
            string codigo,
            string creador,
            ConfiguracionPartidaDTO configuracion,
            IGestorNotificacionesSalaInterna gestorNotificaciones)
        {
            Codigo = codigo;
            Creador = creador;
            Configuracion = configuracion;

            _gestorNotificaciones = gestorNotificaciones ??
                throw new ArgumentNullException(nameof(gestorNotificaciones));

            _jugadores = new List<string>();
            PartidaIniciada = false;
            PartidaFinalizada = false;
            DebeEliminarse = false;
        }

        public string Codigo { get; }
        public string Creador { get; }
        public ConfiguracionPartidaDTO Configuracion { get; }

        public bool DebeEliminarse { get; private set; }
        public bool PartidaIniciada { get; set; }
        public bool PartidaFinalizada { get; set; }

        /// <summary>
        /// Genera un objeto de transferencia de datos (DTO) con el estado actual de la sala.
        /// <returns>Instancia de SalaDTO.</returns>
        /// </summary>
        public SalaDTO ConvertirADto()
        {
            lock (_sincrono)
            {
                return new SalaDTO
                {
                    Codigo = Codigo,
                    Creador = Creador,
                    Configuracion = Configuracion,
                    Jugadores = new List<string>(_jugadores)
                };
            }
        }

        /// <summary>
        /// Intenta agregar un jugador a la sala y gestiona las notificaciones.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a agregar.</param>
        /// <param name="callback">Canal de comunicacion del usuario.</param>
        /// <param name="notificar">Indica si se debe notificar el ingreso a los demas.</param>
        /// <returns>DTO actualizado de la sala.</returns>
        public SalaDTO AgregarJugador(
            string nombreUsuario,
            ISalasManejadorCallback callback,
            bool notificar)
        {
            lock (_sincrono)
            {
                if (_jugadores.Contains(nombreUsuario))
                {
                    _gestorNotificaciones.Registrar(nombreUsuario, callback);
                    return ConvertirADto();
                }

                ValidarCapacidad();

                _jugadores.Add(nombreUsuario);
                _gestorNotificaciones.Registrar(nombreUsuario, callback);

                if (notificar)
                {
                    _gestorNotificaciones.NotificarIngreso(Codigo, nombreUsuario, ConvertirADto());
                }

                return ConvertirADto();
            }
        }

        /// <summary>
        /// Remueve a un jugador de la sala y gestiona la logica de abandono.
        /// </summary>
        /// <param name="nombreUsuario">Usuario a remover.</param>
        public void RemoverJugador(string nombreUsuario)
        {
            lock (_sincrono)
            {
                if (!_jugadores.Contains(nombreUsuario))
                {
                    return;
                }

                _jugadores.Remove(nombreUsuario);
                _gestorNotificaciones.Remover(nombreUsuario);

                ManejarLogicaSalida(nombreUsuario);
            }
        }

        /// <summary>
        /// Expulsa forzosamente a un jugador de la sala si el solicitante es el creador.
        /// </summary>
        /// <param name="nombreAnfitrion">Nombre de quien solicita la expulsion.</param>
        /// <param name="nombreJugadorAExpulsar">Nombre del jugador a expulsar.</param>
        public void ExpulsarJugador(string nombreAnfitrion, string nombreJugadorAExpulsar)
        {
            lock (_sincrono)
            {
                ValidarPermisosExpulsion(nombreAnfitrion, nombreJugadorAExpulsar);

                _logger.InfoFormat(
                    "Sala '{0}': Removiendo jugador '{1}' de la lista de jugadores.",
                    Codigo, nombreJugadorAExpulsar);

                var callbackExpulsado = _gestorNotificaciones.ObtenerCallback(
                    nombreJugadorAExpulsar);

                _jugadores.Remove(nombreJugadorAExpulsar);
                _gestorNotificaciones.Remover(nombreJugadorAExpulsar);

                _logger.InfoFormat(
                    "Sala '{0}': Notificando expulsion de '{1}' a todos los clientes.",
                    Codigo, nombreJugadorAExpulsar);

                _gestorNotificaciones.NotificarExpulsion(
                    Codigo,
                    nombreJugadorAExpulsar,
                    callbackExpulsado,
                    ConvertirADto());

                _logger.InfoFormat(
                    "Sala '{0}': Expulsion de '{1}' completada.",
                    Codigo, nombreJugadorAExpulsar);
            }
        }

        private void ValidarCapacidad()
        {
            if (_jugadores.Count >= MaximoJugadores)
            {
                throw new FaultException(MensajesError.Cliente.SalaLlena);
            }
        }

        private void ValidarPermisosExpulsion(string nombreAnfitrion, string objetivo)
        {
            if (!string.Equals(nombreAnfitrion, Creador, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException(MensajesError.Cliente.SalaExpulsionRestringida);
            }

            if (string.Equals(objetivo, Creador, StringComparison.OrdinalIgnoreCase))
            {
                throw new FaultException(MensajesError.Cliente.SalaCreadorNoExpulsable);
            }

            if (!_jugadores.Contains(objetivo))
            {
                throw new FaultException(MensajesError.Cliente.SalaJugadorNoExiste);
            }
        }

        private void ManejarLogicaSalida(string nombreUsuario)
        {
            bool esAnfitrion = string.Equals(
                nombreUsuario,
                Creador,
                StringComparison.OrdinalIgnoreCase);

            if (PartidaFinalizada && esAnfitrion)
            {
                _gestorNotificaciones.Limpiar();
                DebeEliminarse = true;
                return;
            }

            var salaActualizada = ConvertirADto();

            _gestorNotificaciones.NotificarSalida(Codigo, nombreUsuario, salaActualizada);

            if (esAnfitrion)
            {
                CancelarSala();
            }
            else if (_jugadores.Count == 0)
            {
                DebeEliminarse = true;
            }
        }

        private void CancelarSala()
        {
            _gestorNotificaciones.NotificarCancelacion(Codigo);
            _jugadores.Clear();
            _gestorNotificaciones.Limpiar();
            DebeEliminarse = true;
            _logger.Info("Sala cancelada por salida del anfitrion.");
        }
    }
}