using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using PictionaryMusicalServidor.Servicios.Contratos;
using PictionaryMusicalServidor.Servicios.Contratos.DTOs;
using PictionaryMusicalServidor.Servicios.Servicios.Constantes;
using PictionaryMusicalServidor.Servicios.Servicios.Utilidades;
using PictionaryMusicalServidor.Servicios.Servicios.Modelos;
using PictionaryMusicalServidor.Servicios.Servicios.Notificadores;

namespace PictionaryMusicalServidor.Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SalasManejador : ISalasManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasManejador));
        private static readonly ConcurrentDictionary<string, SalaInterna> _salas = new(StringComparer.OrdinalIgnoreCase);
        private static readonly NotificadorSalas _notificador = new(() => _salas.Values);

        public SalaDTO CrearSala(string nombreCreador, ConfiguracionPartidaDTO configuracion)
        {
            ValidadorNombreUsuario.Validar(nombreCreador, nameof(nombreCreador));
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

                _notificador.NotificarListaSalasATodos();
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
            ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                var resultado = sala.AgregarJugador(nombreUsuario.Trim(), callback, notificar: true);

                _notificador.NotificarListaSalasATodos();
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
            ValidadorNombreUsuario.Validar(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                sala.RemoverJugador(nombreUsuario.Trim());

                if (sala.DebeEliminarse)
                    _salas.TryRemove(codigoSala.Trim(), out _);

                _notificador.NotificarListaSalasATodos();
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
                var sesionId = _notificador.Suscribir(callback);

                var canal = OperationContext.Current?.Channel;
                if (canal != null)
                {
                    canal.Closed += (_, __) => _notificador.Desuscribir(sesionId);
                    canal.Faulted += (_, __) => _notificador.Desuscribir(sesionId);
                }

                _notificador.NotificarListaSalas(callback);
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
                _notificador.DesuscribirPorCallback(callback);
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
            ValidadorNombreUsuario.Validar(nombreHost, nameof(nombreHost));
            ValidadorNombreUsuario.Validar(nombreJugadorAExpulsar, nameof(nombreJugadorAExpulsar));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException(MensajesError.Cliente.CodigoSalaObligatorio);

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException(MensajesError.Cliente.SalaNoEncontrada);

                sala.ExpulsarJugador(nombreHost.Trim(), nombreJugadorAExpulsar.Trim());

                if (sala.DebeEliminarse)
                    _salas.TryRemove(codigoSala.Trim(), out _);

                _notificador.NotificarListaSalasATodos();
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

    }
}
