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
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Salas
{
    public class IngresoPartidaInvitadoVistaModelo : BaseVistaModelo
    {
        private const int MaximoJugadoresSala = 4;

        private readonly ISalasServicio _salasServicio;
        private readonly ILocalizacionServicio _localizacionServicio;

        private bool _estaProcesando;
        private string _codigoSala;

        public IngresoPartidaInvitadoVistaModelo(
            ILocalizacionServicio localizacionServicio,
            ISalasServicio salasServicio)
        {
            _localizacionServicio = localizacionServicio ?? throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicio = salasServicio ?? throw new ArgumentNullException(nameof(salasServicio));

            UnirseSalaComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await UnirseSalaComoInvitadoAsync().ConfigureAwait(true);
            }, _ => !EstaProcesando);

            CancelarComando = new ComandoDelegado(() =>
            {
                ManejadorSonido.ReproducirClick();
                CerrarVentana?.Invoke();
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

        public Action CerrarVentana { get; set; }

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
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.globalTextoIngreseCodigoPartida);
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
                    string nombreInvitado = NombreInvitadoGenerador.Generar(culturaActual, nombresReservados);

                    if (string.IsNullOrWhiteSpace(nombreInvitado))
                    {
                        break;
                    }

                    ResultadoUnionInvitado resultado = await IntentarUnirseAsync(codigo, nombreInvitado).ConfigureAwait(true);

                    switch (resultado.Estado)
                    {
                        case EstadoUnionInvitado.Exito:
                            ManejadorSonido.ReproducirExito();
                            SeUnioSala = true;
                            SalaUnida?.Invoke(resultado.Sala, nombreInvitado);
                            CerrarVentana?.Invoke();
                            return;

                        case EstadoUnionInvitado.NombreDuplicado:
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
                            ManejadorSonido.ReproducirError();
                            AvisoAyudante.Mostrar(Lang.errorTextoSalaLlena);
                            return;

                        case EstadoUnionInvitado.SalaNoEncontrada:
                            ManejadorSonido.ReproducirError();
                            AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida);
                            return;

                        case EstadoUnionInvitado.Error:
                            ManejadorSonido.ReproducirError();
                            AvisoAyudante.Mostrar(resultado.Mensaje ?? Lang.errorTextoNoEncuentraPartida);
                            return;
                    }
                }

                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoNombresInvitadoAgotados);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task<ResultadoUnionInvitado> IntentarUnirseAsync(string codigo, string nombreInvitado)
        {
            try
            {
                DTOs.SalaDTO sala = await _salasServicio.UnirseSalaAsync(codigo, nombreInvitado).ConfigureAwait(true);

                if (sala == null)
                {
                    return ResultadoUnionInvitado.SalaNoEncontrada();
                }

                if (SalaLlena(sala))
                {
                    await IntentarAbandonarSalaAsync(sala.Codigo ?? codigo, nombreInvitado).ConfigureAwait(true);
                    return ResultadoUnionInvitado.CrearErrorSalaLlena();
                }

                if (NombreDuplicado(sala, nombreInvitado))
                {
                    await IntentarAbandonarSalaAsync(sala.Codigo ?? codigo, nombreInvitado).ConfigureAwait(true);
                    return ResultadoUnionInvitado.CrearErrorNombreDuplicado(sala.Jugadores);
                }

                return ResultadoUnionInvitado.Exito(sala);
            }
            catch (ServicioExcepcion ex)
            {
                string mensajeServidor = ex?.Message;
                string mensajeLocalizado = MensajeServidorAyudante.Localizar(mensajeServidor, mensajeServidor);

                bool esSalaLlena = MensajeServidorAyudante.CoincideConMensaje(
                    mensajeServidor,
                    "La sala está llena",
                    Lang.errorTextoSalaLlena);

                bool esSalaNoEncontrada = MensajeServidorAyudante.CoincideConMensaje(
                    mensajeServidor,
                    "No se encontró la sala especificada",
                    Lang.errorTextoNoEncuentraPartida);

                if (esSalaLlena)
                {
                    return ResultadoUnionInvitado.CrearErrorSalaLlena();
                }

                if (ex?.Tipo == TipoErrorServicio.FallaServicio || esSalaNoEncontrada)
                {
                    return ResultadoUnionInvitado.SalaNoEncontrada();
                }

                return ResultadoUnionInvitado.Error(mensajeLocalizado ?? Lang.errorTextoNoEncuentraPartida);
            }
        }

        private static bool SalaLlena(DTOs.SalaDTO sala)
        {
            if (sala?.Jugadores == null)
            {
                return false;
            }

            int jugadoresValidos = sala.Jugadores.Count(jugador => !string.IsNullOrWhiteSpace(jugador));
            return jugadoresValidos > MaximoJugadoresSala;
        }

        private static bool NombreDuplicado(DTOs.SalaDTO sala, string nombreInvitado)
        {
            if (sala?.Jugadores == null)
            {
                return false;
            }

            int coincidencias = sala.Jugadores
                .Count(jugador => string.Equals(jugador?.Trim(), nombreInvitado, StringComparison.OrdinalIgnoreCase));

            return coincidencias > 1;
        }

        private async Task IntentarAbandonarSalaAsync(string codigoSala, string nombreInvitado)
        {
            try
            {
                await _salasServicio.AbandonarSalaAsync(codigoSala, nombreInvitado).ConfigureAwait(true);
            }
            catch
            {
                // Ignorar cualquier error al abandonar la sala en este flujo.
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

            public static ResultadoUnionInvitado CrearErrorNombreDuplicado(IEnumerable<string> jugadores)
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
