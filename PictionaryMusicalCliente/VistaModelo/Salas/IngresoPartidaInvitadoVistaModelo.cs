using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    /// <summary>
    /// Controla el flujo para que un usuario invitado se una a una partida mediante codigo.
    /// </summary>
    public class IngresoPartidaInvitadoVistaModelo : BaseVistaModelo
    {
        private static readonly ILog Log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaximoJugadoresSala = 4;

        private readonly ISalasServicio _salasServicio;
        private readonly ILocalizacionServicio _localizacionServicio;

        private bool _estaProcesando;
        private string _codigoSala;

        /// <summary>
        /// Inicializa el ViewModel.
        /// </summary>
        /// <param name="localizacionServicio">Servicio para obtener la cultura actual.</param>
        /// <param name="salasServicio">Servicio para unirse a la sala.</param>
        public IngresoPartidaInvitadoVistaModelo(
            ILocalizacionServicio localizacionServicio,
            ISalasServicio salasServicio)
        {
            _localizacionServicio = localizacionServicio ??
                throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicio = salasServicio ??
                throw new ArgumentNullException(nameof(salasServicio));

            UnirseSalaComando = new ComandoAsincrono(async _ =>
            {
                SonidoManejador.ReproducirClick();
                await UnirseSalaComoInvitadoAsync().ConfigureAwait(true);
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(() =>
            {
                SonidoManejador.ReproducirClick();
                CerrarVentana?.Invoke();
            }, () => !EstaProcesando);
        }

        /// <summary>
        /// Codigo de sala ingresado por el usuario.
        /// </summary>
        public string CodigoSala
        {
            get => _codigoSala;
            set => EstablecerPropiedad(ref _codigoSala, value);
        }

        /// <summary>
        /// Indica si hay una operacion de union en curso.
        /// </summary>
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

        /// <summary>
        /// Indica si el proceso de union fue exitoso.
        /// </summary>
        public bool SeUnioSala { get; private set; }

        /// <summary>
        /// Comando para intentar unirse a la sala.
        /// </summary>
        public IComandoAsincrono UnirseSalaComando { get; }

        /// <summary>
        /// Comando para cancelar y cerrar el dialogo.
        /// </summary>
        public ICommand CancelarComando { get; }

        /// <summary>
        /// Accion para cerrar la ventana.
        /// </summary>
        public Action CerrarVentana { get; set; }

        /// <summary>
        /// Evento disparado al unirse correctamente, devolviendo los datos de la sala y el 
        /// nombre generado.
        /// </summary>
        public Action<DTOs.SalaDTO, string> SalaUnida { get; set; }

        private async Task UnirseSalaComoInvitadoAsync()
        {
            if (EstaProcesando)
            {
                return;
            }

            string codigo = CodigoSala?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                SonidoManejador.ReproducirError();
                AvisoAyudante.Mostrar(Lang.unirseSalaTextoVacio);
                return;
            }

            await IntentarUnirseSalaAsync(codigo).ConfigureAwait(true);
        }

        private async Task IntentarUnirseSalaAsync(string codigo)
        {
            try
            {
                EstaProcesando = true;

                var nombresReservados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var culturaActual = _localizacionServicio?.CulturaActual;
                const int maxIntentos = 20;

                for (int intento = 0; intento < maxIntentos; intento++)
                {
                    string nombreInvitado = NombreInvitadoGenerador.Generar(
                        culturaActual,
                        nombresReservados);

                    if (string.IsNullOrWhiteSpace(nombreInvitado))
                    {
                        Log.Warn("Generador de nombres retornó vacío/nulo.");
                        break;
                    }

                    ResultadoUnionInvitado resultado = await IntentarUnirseAsync(
                        codigo,
                        nombreInvitado).ConfigureAwait(true);

                    switch (resultado.Estado)
                    {
                        case EstadoUnionInvitado.Exito:
                            Log.InfoFormat("Invitado unido exitosamente: {0}",
                                nombreInvitado);
                            SonidoManejador.ReproducirExito();
                            SeUnioSala = true;
                            SalaUnida?.Invoke(resultado.Sala, nombreInvitado);
                            CerrarVentana?.Invoke();
                            return;

                        case EstadoUnionInvitado.NombreDuplicado:
                            Log.InfoFormat("Nombre duplicado '{0}', reintentando...",
                                nombreInvitado);
                            nombresReservados.Add(nombreInvitado);
                            if (resultado.JugadoresActuales != null)
                            {
                                foreach (string jugador in resultado.JugadoresActuales)
                                {
                                    nombresReservados.Add(jugador);
                                }
                            }
                            continue;

                        case EstadoUnionInvitado.SalaLlena:
                            Log.Warn("Intento de unirse a sala llena.");
                            SonidoManejador.ReproducirError();
                            AvisoAyudante.Mostrar(Lang.errorTextoSalaLlena);
                            return;

                        case EstadoUnionInvitado.SalaNoEncontrada:
                            Log.WarnFormat("Sala no encontrada: {0}",
                                codigo);
                            SonidoManejador.ReproducirError();
                            AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida);
                            return;

                        case EstadoUnionInvitado.Error:
                            Log.ErrorFormat("Error al unirse: {0}",
                                resultado.Mensaje);
                            SonidoManejador.ReproducirError();
                            AvisoAyudante.Mostrar(
                                resultado.Mensaje ?? Lang.errorTextoNoEncuentraPartida);
                            return;
                    }
                }

                Log.Error("Se agotaron los intentos de generar nombre único.");
                SonidoManejador.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoNombresInvitadoAgotados);
            }
            finally
            {
                EstaProcesando = false;
            }
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
                Log.Error("Excepción de servicio al intentar unirse como invitado.", ex);
                string mensaje;
                if (ex?.Tipo == TipoErrorServicio.FallaServicio || string.IsNullOrWhiteSpace
                    (ex?.Message))
                {
                    mensaje = Lang.errorTextoNoEncuentraPartida;
                }
                else
                {
                    mensaje = ex.Message;
                }

                SonidoManejador.ReproducirError();
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
                Log.Warn("Error en cleanup al abandonar sala (ignorado intencionalmente).", ex);
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