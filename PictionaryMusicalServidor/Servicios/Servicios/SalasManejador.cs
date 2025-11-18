using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SalasManejador : ISalasManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasManejador));
        private static readonly ConcurrentDictionary<string, SalaInterna> _salas = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<Guid, ISalasCallback> _suscripciones = new();

        public SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion)
        {
            ValidarNombreUsuario(nombreCreador, nameof(nombreCreador));
            ValidarConfiguracion(configuracion);

            try
            {
                string codigo = GenerarCodigoSala();
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();

                var sala = new SalaInterna(codigo, nombreCreador.Trim(), configuracion);
                sala.AgregarJugador(nombreCreador.Trim(), callback, notificar: false);

                if (!_salas.TryAdd(codigo, sala))
                {
                    throw new FaultException(MensajesError.Cliente.ErrorCrearSala);
                }

                NotificarListaSalasATodos();
                return sala.ToDto();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaCrearErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoCrearSala);
            }
        }

        public SalaDTO UnirseSala(string codigoSala, string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var resultado = sala.AgregarJugador(nombreUsuario.Trim(), callback, notificar: true);

                NotificarListaSalasATodos();
                return resultado;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaUnirseTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoUnirse);
            }
        }

        public IList<SalaDTO> ObtenerSalas()
        {
            try
            {
                return _salas.Values.Select(s => s.ToDto()).ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaObtenerListaOperacionInvalida, ex);
                return new List<SalaDTO>();
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaObtenerListaErrorGeneral, ex);
                return new List<SalaDTO>();
            }
        }

        public void AbandonarSala(string codigoSala, string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                sala.RemoverJugador(nombreUsuario.Trim());

                if (sala.DebeEliminarse)
                    _salas.TryRemove(codigoSala.Trim(), out _);

                NotificarListaSalasATodos();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaAbandonarOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoAbandonar);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaAbandonarErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoAbandonar);
            }
        }

        public void SuscribirListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var sesionId = Guid.NewGuid();
                
                _suscripciones.AddOrUpdate(sesionId, callback, (_, __) => callback);

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => _suscripciones.TryRemove(sesionId, out ISalasCallback _);
                    canal.Faulted += (_, __) => _suscripciones.TryRemove(sesionId, out ISalasCallback _);
                }

                NotificarListaSalas(callback);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionComunicacion, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionTimeout, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaSuscripcionErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoSuscripcion);
            }
        }

        public void CancelarSuscripcionListaSalas()
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var keysToRemove = _suscripciones.Where(callbakJugador => ReferenceEquals(callbakJugador.Value, callback))
                    .Select(callbakJugador => callbakJugador.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _suscripciones.TryRemove(key, out _);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionOperacionInvalida, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionComunicacion, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionTimeout, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaCancelarSuscripcionErrorGeneral, ex);
            }
        }

        public void ExpulsarJugador(string codigoSala, string nombreHost, string nombreJugadorAExpulsar)
        {
            ValidarNombreUsuario(nombreHost, nameof(nombreHost));
            ValidarNombreUsuario(nombreJugadorAExpulsar, nameof(nombreJugadorAExpulsar));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                sala.ExpulsarJugador(nombreHost.Trim(), nombreJugadorAExpulsar.Trim());

                if (sala.DebeEliminarse)
                    _salas.TryRemove(codigoSala.Trim(), out _);

                NotificarListaSalasATodos();
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(MensajesError.Log.SalaExpulsarOperacionInvalida, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoExpulsar);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaExpulsarErrorGeneral, ex);
                throw new FaultException(MensajesError.Cliente.ErrorInesperadoExpulsar);
            }
        }

        internal static SalaDTO ObtenerSalaPorCodigo(string codigoSala)
        {
            if (string.IsNullOrWhiteSpace(codigoSala))
            {
                return null;
            }

            if (_salas.TryGetValue(codigoSala.Trim(), out var sala))
            {
                return sala.ToDto();
            }

            return null;
        }


        private static string GenerarCodigoSala()
        {
            var random = new Random();
            const int maxIntentos = 1000;

            for (int i = 0; i < maxIntentos; i++)
            {
                string codigo = random.Next(0, 1_000_000).ToString("D6");
                if (!_salas.ContainsKey(codigo))
                    return codigo;
            }

            throw new FaultException(MensajesError.Cliente.ErrorGenerarCodigo);
        }

        private static void ValidarNombreUsuario(string nombreUsuario, string parametro)
        {
            string normalizado = nombreUsuario?.Trim();

            if (string.IsNullOrWhiteSpace(normalizado))
            {
                string mensaje = string.Format(CultureInfo.CurrentCulture, MensajesError.Cliente.ParametroObligatorio, parametro);
                throw new FaultException(mensaje);
            }

            if (normalizado.Length > EntradaComunValidador.LongitudMaximaTexto)
            {
                throw new FaultException(MensajesError.Cliente.UsuarioRegistroInvalido);
            }
        }

        private static void ValidarConfiguracion(ConfiguracionPartidaDTO configuracion)
        {
            if (configuracion == null)
                throw new FaultException(MensajesError.Cliente.ConfiguracionObligatoria);

            if (configuracion.NumeroRondas <= 0)
                throw new FaultException(MensajesError.Cliente.NumeroRondasInvalido);

            if (configuracion.TiempoPorRondaSegundos <= 0)
                throw new FaultException(MensajesError.Cliente.TiempoRondaInvalido);

            if (string.IsNullOrWhiteSpace(configuracion.IdiomaCanciones))
                throw new FaultException(MensajesError.Cliente.IdiomaObligatorio);

            if (string.IsNullOrWhiteSpace(configuracion.Dificultad))
                throw new FaultException(MensajesError.Cliente.DificultadObligatoria);
        }

        private static void NotificarListaSalasATodos()
        {
            var salas = _salas.Values.Select(s => s.ToDto()).ToArray();

            foreach (var callbakJugador in _suscripciones)
            {
                try
                {
                    callbakJugador.Value.NotificarListaSalasActualizada(salas);
                }
                catch (CommunicationException ex)
                {
                    _logger.Warn(MensajesError.Log.SalaNotificarListaComunicacion, ex);
                    _suscripciones.TryRemove(callbakJugador.Key, out _);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn(MensajesError.Log.SalaNotificarListaTimeout, ex);
                    _suscripciones.TryRemove(callbakJugador.Key, out _);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                }
                catch (Exception ex)
                {
                    _logger.Error(MensajesError.Log.SalaNotificarListaErrorGeneral, ex);
                }
            }
        }

        private static void NotificarListaSalas(ISalasCallback callback)
        {
            try
            {
                var salas = _salas.Values.Select(s => s.ToDto()).ToArray();
                callback.NotificarListaSalasActualizada(salas);
            }
            catch (CommunicationException ex)
            {
                _logger.Warn(MensajesError.Log.SalaNotificarListaComunicacion, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(MensajesError.Log.SalaNotificarListaTimeout, ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(MensajesError.Log.SalaNotificarListaErrorGeneral, ex);
            }
        }


        private sealed class SalaInterna
        {
            private const int MaximoJugadores = 4;
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

            public SalaDTO AgregarJugador(string nombreUsuario, ISalasCallback callback, bool notificar)
            {
                lock (_sync)
                {
                    if (JugadorYaExiste(nombreUsuario))
                    {
                        _callbacks[nombreUsuario] = callback;
                        return ToDto();
                    }

                    ValidarCapacidadSala();

                    Jugadores.Add(nombreUsuario);
                    _callbacks[nombreUsuario] = callback;

                    if (notificar)
                    {
                        var salaActualizada = ToDto();
                        NotificarNuevoJugadorYActualizacion(nombreUsuario, salaActualizada);
                    }

                    return ToDto();
                }
            }

            public void RemoverJugador(string nombreUsuario)
            {
                lock (_sync)
                {
                    if (!RemoverJugadorDeSala(nombreUsuario))
                    {
                        return;
                    }

                    var salaActualizada = ToDto();
                    NotificarSalidaYActualizacion(nombreUsuario, salaActualizada);

                    if (DebeMarcarseParaEliminar(nombreUsuario))
                    {
                        DebeEliminarse = true;
                    }
                }
            }

            public void ExpulsarJugador(string nombreHost, string nombreJugadorAExpulsar)
            {
                lock (_sync)
                {
                    ValidarExpulsion(nombreHost, nombreJugadorAExpulsar);

                    var callbackExpulsado = ObtenerCallback(nombreJugadorAExpulsar);

                    Jugadores.RemoveAll(j => string.Equals(j, nombreJugadorAExpulsar, StringComparison.OrdinalIgnoreCase));
                    _callbacks.Remove(nombreJugadorAExpulsar);

                    var salaActualizada = ToDto();

                    NotificarJugadorExpulsado(callbackExpulsado, nombreJugadorAExpulsar);
                    NotificarSalidaYActualizacion(nombreJugadorAExpulsar, salaActualizada);
                }
            }


            private int ContarJugadoresActivos()
            {
                return Jugadores.Count;
            }
        }
    }
}
