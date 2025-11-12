using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Utilidades;
using DTOs = Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente
{
    /// <summary>
    /// Lógica de interacción para UnirsePartidaInvitado.xaml
    /// </summary>
    public partial class IngresoPartidaInvitado : Window
    {
        private readonly ISalasServicio _salasServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private bool _estaProcesando;

        public IngresoPartidaInvitado(ILocalizacionServicio localizacionServicio, ISalasServicio salasServicio)
        {
            _localizacionServicio = localizacionServicio ?? throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicio = salasServicio ?? throw new ArgumentNullException(nameof(salasServicio));

            InitializeComponent();
        }

        public Action<DTOs.SalaDTO, string> SalaUnida { get; set; }

        private async void BotonUnirsePartida(object sender, RoutedEventArgs e)
        {
            if (_estaProcesando)
            {
                return;
            }

            string codigo = campoTextoCodigoPartida.Text?.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.globalTextoIngreseCodigoPartida);
                return;
            }

            await UnirseSalaComoInvitadoAsync(codigo).ConfigureAwait(true);
        }

        private async Task UnirseSalaComoInvitadoAsync(string codigo)
        {
            try
            {
                EstablecerProcesando(true);

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
                            SalaUnida?.Invoke(resultado.Sala, nombreInvitado);
                            DialogResult = true;
                            Close();
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
                EstablecerProcesando(false);
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
                    return ResultadoUnionInvitado.SalaLlena();
                }

                if (NombreDuplicado(sala, nombreInvitado))
                {
                    await IntentarAbandonarSalaAsync(sala.Codigo ?? codigo, nombreInvitado).ConfigureAwait(true);
                    return ResultadoUnionInvitado.NombreDuplicado(sala.Jugadores);
                }

                return ResultadoUnionInvitado.Exito(sala);
            }
            catch (ExcepcionServicio ex)
            {
                return ResultadoUnionInvitado.Error(ex.Message ?? Lang.errorTextoNoEncuentraPartida);
            }
            catch (Exception)
            {
                return ResultadoUnionInvitado.Error(Lang.errorTextoNoEncuentraPartida);
            }
        }

        private static bool SalaLlena(DTOs.SalaDTO sala)
        {
            if (sala?.Jugadores == null)
            {
                return false;
            }

            int jugadoresValidos = sala.Jugadores.Count(jugador => !string.IsNullOrWhiteSpace(jugador));
            return jugadoresValidos > 4;
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

        private void EstablecerProcesando(bool procesando)
        {
            _estaProcesando = procesando;
            botonUnirsePartida.IsEnabled = !procesando;
            botonCancelarPartida.IsEnabled = !procesando;
        }

        private void BotonCancelarUnirse(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            public static ResultadoUnionInvitado SalaLlena()
            {
                return new ResultadoUnionInvitado(EstadoUnionInvitado.SalaLlena);
            }

            public static ResultadoUnionInvitado NombreDuplicado(IEnumerable<string> jugadores)
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
