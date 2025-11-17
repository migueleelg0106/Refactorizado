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
            catch (FaultException ex)
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
                var sessionId = Guid.NewGuid();
                
                _suscripciones.AddOrUpdate(sessionId, callback, (_, __) => callback);

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => _suscripciones.TryRemove(sessionId, out ISalasCallback _);
                    canal.Faulted += (_, __) => _suscripciones.TryRemove(sessionId, out ISalasCallback _);
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
                var keysToRemove = _suscripciones.Where(kvp => ReferenceEquals(kvp.Value, callback))
                    .Select(kvp => kvp.Key)
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
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                string mensaje = string.Format(CultureInfo.CurrentCulture, MensajesError.Cliente.ParametroObligatorio, parametro);
                throw new FaultException(mensaje);
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

            foreach (var kvp in _suscripciones)
            {
                try
                {
                    kvp.Value.NotificarListaSalasActualizada(salas);
                }
                catch (CommunicationException ex)
                {
                    _logger.Warn(MensajesError.Log.SalaNotificarListaComunicacion, ex);
                    _suscripciones.TryRemove(kvp.Key, out _);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warn(MensajesError.Log.SalaNotificarListaTimeout, ex);
                    _suscripciones.TryRemove(kvp.Key, out _);
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


        private class SalaInterna
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

            public SalaDTO AgregarJugador(string nombreUsuario, ISalasCallback callback, bool notificar)
            {
                lock (_sync)
                {
                    if (Jugadores.Contains(nombreUsuario, StringComparer.OrdinalIgnoreCase))
                    {
                        _callbacks[nombreUsuario] = callback;
                        return ToDto();
                    }

                    if (ContarJugadoresActivos() >= MaximoJugadores)
                        throw new FaultException(MensajesError.Cliente.SalaLlena);

                    Jugadores.Add(nombreUsuario);
                    _callbacks[nombreUsuario] = callback;

                    if (notificar)
                    {
                        var salaActualizada = ToDto();

                        foreach (var kvp in _callbacks)
                        {
                            if (!string.Equals(kvp.Key, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    kvp.Value.NotificarJugadorSeUnio(Codigo, nombreUsuario);
                                }
                                catch (CommunicationException ex)
                                {
                                    _logger.Warn(MensajesError.Log.SalaNotificarJugadorUnionError, ex);
                                }
                                catch (TimeoutException ex)
                                {
                                    _logger.Warn(MensajesError.Log.SalaNotificarJugadorUnionError, ex);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(MensajesError.Log.SalaNotificarJugadorUnionError, ex);
                                }
                            }

                            try
                            {
                                kvp.Value.NotificarSalaActualizada(salaActualizada);
                            }
                            catch (CommunicationException ex)
                            {
                                _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                            }
                            catch (TimeoutException ex)
                            {
                                _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                            }
                            catch (InvalidOperationException ex)
                            {
                                _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                            }
                        }
                    }

                    return ToDto();
                }
            }

            public void RemoverJugador(string nombreUsuario)
            {
                lock (_sync)
                {
                    if (Jugadores.RemoveAll(j => string.Equals(j, nombreUsuario, StringComparison.OrdinalIgnoreCase)) == 0)
                        return;

                    _callbacks.Remove(nombreUsuario);

                    var salaActualizada = ToDto();

                    foreach (var kvp in _callbacks)
                    {
                        try
                        {
                            kvp.Value.NotificarJugadorSalio(Codigo, nombreUsuario);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }

                        try
                        {
                            kvp.Value.NotificarSalaActualizada(salaActualizada);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                    }

                    if (string.Equals(nombreUsuario, Creador, StringComparison.OrdinalIgnoreCase) || Jugadores.Count == 0)
                        DebeEliminarse = true;
                }
            }

            public void ExpulsarJugador(string nombreHost, string nombreJugadorAExpulsar)
            {
                lock (_sync)
                {
                    if (!string.Equals(nombreHost, Creador, StringComparison.OrdinalIgnoreCase))
                        throw new FaultException(MensajesError.Cliente.SalaExpulsionRestringida);

                    if (string.Equals(nombreJugadorAExpulsar, Creador, StringComparison.OrdinalIgnoreCase))
                        throw new FaultException(MensajesError.Cliente.SalaCreadorNoExpulsable);

                    if (!Jugadores.Contains(nombreJugadorAExpulsar, StringComparer.OrdinalIgnoreCase))
                        throw new FaultException(MensajesError.Cliente.SalaJugadorNoExiste);

                    // Obtener el callback del jugador expulsado antes de removerlo
                    ISalasCallback callbackExpulsado = null;
                    if (_callbacks.ContainsKey(nombreJugadorAExpulsar))
                    {
                        callbackExpulsado = _callbacks[nombreJugadorAExpulsar];
                    }

                    Jugadores.RemoveAll(j => string.Equals(j, nombreJugadorAExpulsar, StringComparison.OrdinalIgnoreCase));
                    _callbacks.Remove(nombreJugadorAExpulsar);

                    var salaActualizada = ToDto();

                    // Notificar al jugador expulsado
                    if (callbackExpulsado != null)
                    {
                        try
                        {
                            callbackExpulsado.NotificarJugadorExpulsado(Codigo, nombreJugadorAExpulsar);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorExpulsionError, ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorExpulsionError, ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(MensajesError.Log.SalaNotificarJugadorExpulsionError, ex);
                        }
                    }

                    // Notificar a los demás jugadores
                    foreach (var kvp in _callbacks)
                    {
                        try
                        {
                            kvp.Value.NotificarJugadorSalio(Codigo, nombreJugadorAExpulsar);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(MensajesError.Log.SalaNotificarJugadorSalidaError, ex);
                        }

                        try
                        {
                            kvp.Value.NotificarSalaActualizada(salaActualizada);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                        catch (TimeoutException ex)
                        {
                            _logger.Warn(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Warn(MensajesError.Log.ComunicacionOperacionInvalida, ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(MensajesError.Log.SalaNotificarJugadorActualizacionError, ex);
                        }
                    }
                }
            }

            private int ContarJugadoresActivos()
            {
                return Jugadores.Count;
            }
        }
    }
}
