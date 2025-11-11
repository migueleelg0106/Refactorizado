using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using log4net;
using Servicios.Contratos;
using Servicios.Contratos.DTOs;

namespace Servicios.Servicios
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SalasManejador : ISalasManejador
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SalasManejador));
        private static readonly ConcurrentDictionary<string, SalaInterna> _salas = new(StringComparer.OrdinalIgnoreCase);

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
                    throw new FaultException("No se pudo crear la sala.");
                }

                return sala.ToDto();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al crear una sala.", ex);
                throw new FaultException("Ocurrió un error inesperado al crear la sala.");
            }
        }

        public SalaDTO UnirseSala(string codigoSala, string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException("El parámetro codigoSala es obligatorio.");

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException("No se encontró la sala especificada.");

                var callback = OperationContext.Current.GetCallbackChannel<ISalasCallback>();
                return sala.AgregarJugador(nombreUsuario.Trim(), callback, notificar: true);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al unirse a la sala {codigoSala}", ex);
                throw new FaultException("Ocurrió un error inesperado al unirse a la sala.");
            }
        }

        public IList<SalaDTO> ObtenerSalas()
        {
            try
            {
                return _salas.Values.Select(s => s.ToDto()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al obtener la lista de salas.", ex);
                return new List<SalaDTO>();
            }
        }

        public void AbandonarSala(string codigoSala, string nombreUsuario)
        {
            ValidarNombreUsuario(nombreUsuario, nameof(nombreUsuario));

            if (string.IsNullOrWhiteSpace(codigoSala))
                throw new FaultException("El parámetro codigoSala es obligatorio.");

            try
            {
                if (!_salas.TryGetValue(codigoSala.Trim(), out var sala))
                    throw new FaultException("No se encontró la sala especificada.");

                sala.RemoverJugador(nombreUsuario.Trim());

                if (sala.DebeEliminarse)
                    _salas.TryRemove(codigoSala.Trim(), out _);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al abandonar la sala {codigoSala}", ex);
                throw new FaultException("Ocurrió un error inesperado al abandonar la sala.");
            }
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

            throw new FaultException("No se pudo generar un código único para la sala.");
        }

        private static void ValidarNombreUsuario(string nombreUsuario, string parametro)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new FaultException($"El parámetro {parametro} es obligatorio.");
        }

        private static void ValidarConfiguracion(ConfiguracionPartidaDTO configuracion)
        {
            if (configuracion == null)
                throw new FaultException("La configuración de la partida es obligatoria.");

            if (configuracion.NumeroRondas <= 0)
                throw new FaultException("El número de rondas debe ser mayor a cero.");

            if (configuracion.TiempoPorRondaSegundos <= 0)
                throw new FaultException("El tiempo por ronda debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(configuracion.IdiomaCanciones))
                throw new FaultException("El idioma de las canciones es obligatorio.");

            if (string.IsNullOrWhiteSpace(configuracion.Dificultad))
                throw new FaultException("La dificultad es obligatoria.");
        }


        private class SalaInterna
        {
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

                    if (Jugadores.Count >= 4)
                        throw new FaultException("La sala está llena.");

                    Jugadores.Add(nombreUsuario);
                    _callbacks[nombreUsuario] = callback;

                    if (notificar)
                    {
                        foreach (var kvp in _callbacks)
                        {
                            if (!string.Equals(kvp.Key, nombreUsuario, StringComparison.OrdinalIgnoreCase))
                            {
                                try 
                                { 
                                    kvp.Value.JugadorSeUnio(Codigo, nombreUsuario); 
                                }
                                catch 
                                { 

                                }
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
                    if (!Jugadores.Remove(nombreUsuario))
                        return;

                    _callbacks.Remove(nombreUsuario);

                    foreach (var kvp in _callbacks)
                    {
                        try { kvp.Value.JugadorSalio(Codigo, nombreUsuario); }
                        catch 
                        {  

                        }
                    }

                    if (string.Equals(nombreUsuario, Creador, StringComparison.OrdinalIgnoreCase) || Jugadores.Count == 0)
                        DebeEliminarse = true;
                }
            }
        }
    }
}
