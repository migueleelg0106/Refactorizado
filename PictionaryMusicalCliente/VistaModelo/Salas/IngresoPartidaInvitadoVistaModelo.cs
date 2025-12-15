using log4net;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using PictionaryMusicalCliente.Utilidades.Abstracciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla el flujo para que un usuario invitado se una a una partida 
    /// mediante codigo.
    /// </summary>
    public class IngresoPartidaInvitadoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog _logger = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaximoJugadoresSala = 4;

        private readonly ISalasServicio _salasServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly SonidoManejador _sonidoManejador;
        private readonly IAvisoServicio _avisoServicio;
        private readonly INombreInvitadoGenerador _nombreInvitadoGenerador;

        private bool _estaProcesando;
        private string _codigoSala;

        public IngresoPartidaInvitadoVistaModelo(
            IVentanaServicio ventana,
            ILocalizadorServicio localizador,
            ILocalizacionServicio localizacionServicio,
            ISalasServicio salasServicio,
            IAvisoServicio avisoServicio,
            SonidoManejador sonidoManejador,
            INombreInvitadoGenerador nombreInvitadoGenerador)
            : base(ventana, localizador)
        {
            _localizacionServicio = localizacionServicio ??
                throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicio = salasServicio ??
                throw new ArgumentNullException(nameof(salasServicio));
            _avisoServicio = avisoServicio ??
                throw new ArgumentNullException(nameof(avisoServicio));
            _sonidoManejador = sonidoManejador ??
                throw new ArgumentNullException(nameof(sonidoManejador));
            _nombreInvitadoGenerador = nombreInvitadoGenerador ??
                throw new ArgumentNullException(nameof(nombreInvitadoGenerador));

            UnirseSalaComando = new ComandoAsincrono(async _ =>
            {
                _sonidoManejador.ReproducirClick();
                await UnirseSalaComoInvitadoAsync().ConfigureAwait(true);
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(() =>
            {
                _sonidoManejador.ReproducirClick();
                _ventana.CerrarVentana(this);
            }, () => !EstaProcesando);
        }

        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)UnirseSalaComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)CancelarComando).NotificarPuedeEjecutar();
                }
            }
        }

        public bool SeUnioSala { get; private set; }

        public IComandoAsincrono UnirseSalaComando { get; }

        public ICommand CancelarComando { get; }

        public DTOs.SalaDTO SalaUnida { get; private set; }

        public string NombreInvitadoGenerado { get; private set; }

        private async Task UnirseSalaComoInvitadoAsync()
        {
            if (EstaProcesando || !ValidarCodigoSala())
            {
                return;
            }

            await IntentarUnirseSalaAsync(CodigoSala.Trim()).ConfigureAwait(true);
        }

        private bool ValidarCodigoSala()
        {
            string codigo = CodigoSala?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                _sonidoManejador.ReproducirError();
                _avisoServicio.Mostrar(Lang.unirseSalaTextoVacio);
                return false;
            }

            return true;
        }

        private async Task IntentarUnirseSalaAsync(string codigo)
        {
            EstaProcesando = true;

            await EjecutarOperacionAsync(async () =>
            {
                await BuscarNombreUnicoYUnirseAsync(codigo).ConfigureAwait(true);
            },
            ex =>
            {
                _logger.Error("Error al intentar unirse a la sala.", ex);
                EstaProcesando = false;
            });

            EstaProcesando = false;
        }

        private async Task BuscarNombreUnicoYUnirseAsync(string codigo)
        {
            var nombresReservados = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            var culturaActual = _localizacionServicio?.CulturaActual;
            const int maxIntentos = 20;

            for (int intento = 0; intento < maxIntentos; intento++)
            {
                string nombreInvitado = GenerarNombreInvitado(
                    culturaActual, 
                    nombresReservados);

                if (string.IsNullOrWhiteSpace(nombreInvitado))
                {
                    _logger.Warn("Generador de nombres retorno vacio/nulo.");
                    break;
                }

                ResultadoUnionInvitado resultado = await IntentarUnirseAsync(
                    codigo,
                    nombreInvitado).ConfigureAwait(true);

                if (await ProcesarResultadoUnionAsync(
                    resultado, 
                    nombreInvitado, 
                    nombresReservados))
                {
                    return;
                }
            }

            MostrarErrorIntentosAgotados();
        }

        private string GenerarNombreInvitado(
            System.Globalization.CultureInfo cultura,
            HashSet<string> nombresReservados)
        {
            return _nombreInvitadoGenerador.Generar(cultura, nombresReservados);
        }

        private Task<bool> ProcesarResultadoUnionAsync(
            ResultadoUnionInvitado resultado,
            string nombreInvitado,
            HashSet<string> nombresReservados)
        {
            switch (resultado.Estado)
            {
                case EstadoUnionInvitado.Exito:
                    MarcarUnionExitosa(resultado.Sala, nombreInvitado);
                    return Task.FromResult(true);

                case EstadoUnionInvitado.NombreDuplicado:
                    AgregarNombresReservados(nombreInvitado, 
                        resultado.JugadoresActuales, 
                        nombresReservados);
                    return Task.FromResult(true);

                case EstadoUnionInvitado.SalaLlena:
                    MostrarErrorSalaLlena();
                    return Task.FromResult(true);

                case EstadoUnionInvitado.SalaNoEncontrada:
                    MostrarErrorSalaNoEncontrada();
                    return Task.FromResult(true);

                case EstadoUnionInvitado.Error:
                    MostrarError(resultado.Mensaje);
                    return Task.FromResult(true);

                default:
                    return Task.FromResult(true);
            }
        }

        private void MarcarUnionExitosa(DTOs.SalaDTO sala, string nombreInvitado)
        {
            _logger.InfoFormat("Invitado unido exitosamente: {0}", nombreInvitado);
            _sonidoManejador.ReproducirNotificacion();
            SeUnioSala = true;
            SalaUnida = sala;
            NombreInvitadoGenerado = nombreInvitado;
            _ventana.CerrarVentana(this);
        }

        private void AgregarNombresReservados(
            string nombreInvitado,
            IReadOnlyCollection<string> jugadoresActuales,
            HashSet<string> nombresReservados)
        {
            _logger.InfoFormat("Nombre duplicado '{0}', reintentando...", 
                nombreInvitado);
            nombresReservados.Add(nombreInvitado);
            
            if (jugadoresActuales != null)
            {
                foreach (string jugador in jugadoresActuales)
                {
                    nombresReservados.Add(jugador);
                }
            }
        }

        private void MostrarErrorSalaLlena()
        {
            _logger.Warn("Intento de unirse a sala llena.");
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(Lang.errorTextoSalaLlena);
        }

        private void MostrarErrorSalaNoEncontrada()
        {
            _logger.WarnFormat("Sala no encontrada: {0}", CodigoSala);
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(Lang.errorTextoNoEncuentraPartida);
        }

        private void MostrarError(string mensaje)
        {
            _logger.ErrorFormat("Error al unirse: {0}", mensaje);
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(mensaje ?? Lang.errorTextoNoEncuentraPartida);
        }

        private void MostrarErrorIntentosAgotados()
        {
            _logger.Error("Se agotaron los intentos de generar nombre unico.");
            _sonidoManejador.ReproducirError();
            _avisoServicio.Mostrar(Lang.errorTextoNombresInvitadoAgotados);
        }

        private async Task<ResultadoUnionInvitado> IntentarUnirseAsync(
            string codigo,
            string nombreInvitado)
        {
            try
            {
                DTOs.SalaDTO sala = await _salasServicio.UnirseSalaAsync(
                    codigo,
                    nombreInvitado).ConfigureAwait(true);

                if (sala == null)
                {
                    return ResultadoUnionInvitado.SalaNoEncontrada();
                }

                if (SalaLlena(sala))
                {
                    await IntentarAbandonarSalaAsync(
                        sala.Codigo ?? codigo,
                        nombreInvitado).ConfigureAwait(true);
                    return ResultadoUnionInvitado.CrearErrorSalaLlena();
                }

                if (NombreDuplicado(sala, nombreInvitado))
                {
                    await IntentarAbandonarSalaAsync(
                        sala.Codigo ?? codigo,
                        nombreInvitado).ConfigureAwait(true);
                    return ResultadoUnionInvitado.CrearErrorNombreDuplicado(sala.Jugadores);
                }

                return ResultadoUnionInvitado.Exito(sala);
            }
            catch (ServicioExcepcion ex)
            {
                _logger.Error("Excepcion de servicio al intentar unirse como invitado.", ex);
                string mensaje;
                if (string.IsNullOrWhiteSpace(ex?.Message))
                {
                    mensaje = Lang.errorTextoNoEncuentraPartida;
                }
                else
                {
                    mensaje = ex.Message;
                }

                _sonidoManejador.ReproducirError();
                return ResultadoUnionInvitado.Error(mensaje);
            }
        }

        private static bool SalaLlena(DTOs.SalaDTO sala)
        {
            if (sala?.Jugadores == null)
            {
                return false;
            }

            int jugadoresValidos = sala.Jugadores.Count(
                jugador => !string.IsNullOrWhiteSpace(jugador));
            return jugadoresValidos > MaximoJugadoresSala;
        }

        private static bool NombreDuplicado(DTOs.SalaDTO sala, string nombreInvitado)
        {
            if (sala?.Jugadores == null)
            {
                return false;
            }

            int coincidencias = sala.Jugadores
                .Count(jugador => string.Equals(
                    jugador?.Trim(),
                    nombreInvitado,
                    StringComparison.OrdinalIgnoreCase));

            return coincidencias > 1;
        }

        private async Task IntentarAbandonarSalaAsync(string codigoSala, string nombreInvitado)
        {
            try
            {
                await _salasServicio.AbandonarSalaAsync(
                    codigoSala,
                    nombreInvitado).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                // Se captura Exception general porque es un cleanup "best effort"
                _logger.Warn("Error en cleanup al abandonar sala (ignorado intencionalmente).",
                    ex);
            }
        }

        private enum EstadoUnionInvitado
        {
            Exito,
            SalaNoEncontrada,
            SalaLlena,
            NombreDuplicado,
            Error
        }

        private sealed class ResultadoUnionInvitado
        {
            private ResultadoUnionInvitado(EstadoUnionInvitado estado)
            {
                Estado = estado;
            }

            public EstadoUnionInvitado Estado { get; }

            public DTOs.SalaDTO Sala { get; private set; }

            public IReadOnlyCollection<string> JugadoresActuales { get; private set; }

            public string Mensaje { get; private set; }

            public static ResultadoUnionInvitado Exito(DTOs.SalaDTO sala)
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.Exito)
                {
                    Sala = sala
                };
            }

            public static ResultadoUnionInvitado SalaNoEncontrada()
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.SalaNoEncontrada);
            }

            public static ResultadoUnionInvitado CrearErrorSalaLlena()
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.SalaLlena);
            }

            public static ResultadoUnionInvitado CrearErrorNombreDuplicado(
                IEnumerable<string> jugadores)
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.NombreDuplicado)
                {
                    JugadoresActuales = jugadores?.Where(j => !string.IsNullOrWhiteSpace(j))
                        .Select(j => j.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                };
            }

            public static ResultadoUnionInvitado Error(string mensaje)
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.Error)
                {
                    Mensaje = mensaje
                };
            }
        }
    }
}