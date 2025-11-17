using PictionaryMusicalCliente.ClienteServicios;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.ClienteServicios.Wcf.Ayudante;
using PictionaryMusicalCliente.VistaModelo.Salas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DTOs = PictionaryMusicalServidor.Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.InicioSesion
{
    public class InicioSesionVistaModelo : BaseVistaModelo
    {
        private readonly IInicioSesionServicio _inicioSesionServicio;
        private readonly ICambioContrasenaServicio _cambioContrasenaServicio;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaDialogoServicio;
        private readonly ILocalizacionServicio _localizacionServicio;
        private readonly Func<ISalasServicio> _salasServicioFactory;

        public const string CampoContrasena = "Contrasena";

        private string _identificador;
        private string _contrasena;
        private bool _estaProcesando;
        private ObservableCollection<IdiomaOpcion> _idiomasDisponibles;
        private IdiomaOpcion _idiomaSeleccionado;

        public InicioSesionVistaModelo(
            IInicioSesionServicio inicioSesionServicio,
            ICambioContrasenaServicio cambioContrasenaServicio,
            IRecuperacionCuentaServicio recuperacionCuentaDialogoServicio,
            ILocalizacionServicio localizacionServicio,
            Func<ISalasServicio> salasServicioFactory)
        {
            _inicioSesionServicio = inicioSesionServicio ?? throw new ArgumentNullException(nameof(inicioSesionServicio));
            _cambioContrasenaServicio = cambioContrasenaServicio ?? throw new ArgumentNullException(nameof(cambioContrasenaServicio));
            _recuperacionCuentaDialogoServicio = recuperacionCuentaDialogoServicio ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogoServicio));
            _localizacionServicio = localizacionServicio ?? throw new ArgumentNullException(nameof(localizacionServicio));
            _salasServicioFactory = salasServicioFactory ?? throw new ArgumentNullException(nameof(salasServicioFactory));

            IniciarSesionComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await IniciarSesionAsync();
            }, _ => !EstaProcesando);

            RecuperarCuentaComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await RecuperarCuentaAsync();
            }, _ => !EstaProcesando);

            AbrirCrearCuentaComando = new ComandoDelegado(_ =>
            {
                ManejadorSonido.ReproducirClick();
                AbrirCrearCuenta?.Invoke();
            });

            IniciarSesionInvitadoComando = new ComandoAsincrono(async _ =>
            {
                ManejadorSonido.ReproducirClick();
                await IniciarSesionInvitadoAsync().ConfigureAwait(true);
            }, _ => !EstaProcesando);

            CargarIdiomas();
        }

        public string Identificador { get => _identificador; set => EstablecerPropiedad(ref _identificador, value); }
        public ObservableCollection<IdiomaOpcion> IdiomasDisponibles { get => _idiomasDisponibles; private set => EstablecerPropiedad(ref _idiomasDisponibles, value); }
        public IdiomaOpcion IdiomaSeleccionado
        {
            get => _idiomaSeleccionado;
            set
            {
                if (EstablecerPropiedad(ref _idiomaSeleccionado, value) && value != null)
                {
                    _localizacionServicio.EstablecerIdioma(value.Codigo);
                }
            }
        }
        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)IniciarSesionComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)RecuperarCuentaComando).NotificarPuedeEjecutar();
                    ((IComandoNotificable)IniciarSesionInvitadoComando).NotificarPuedeEjecutar();
                }
            }
        }
        public IComandoAsincrono IniciarSesionComando { get; }
        public IComandoAsincrono RecuperarCuentaComando { get; }
        public ICommand AbrirCrearCuentaComando { get; }
        public IComandoAsincrono IniciarSesionInvitadoComando { get; }
        public Action AbrirCrearCuenta { get; set; }
        public Action<DTOs.ResultadoInicioSesionDTO> InicioSesionCompletado { get; set; }
        public Action CerrarAccion { get; set; }
        public Action<IList<string>> MostrarCamposInvalidos { get; set; }
        public Action<IngresoPartidaInvitadoVistaModelo> MostrarIngresoInvitado { get; set; }
        public Action<DTOs.SalaDTO, ISalasServicio, string> AbrirVentanaJuegoInvitado { get; set; }


        public void EstablecerContrasena(string contrasena)
        {
            _contrasena = contrasena;
        }

        private Task IniciarSesionInvitadoAsync()
        {
            ISalasServicio salasServicio = null;

            try
            {
                salasServicio = _salasServicioFactory?.Invoke();

                if (salasServicio == null)
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida);
                    return Task.CompletedTask;
                }

                var vistaModelo = new IngresoPartidaInvitadoVistaModelo(_localizacionServicio, salasServicio);

                vistaModelo.SalaUnida = (sala, nombreInvitado) =>
                {
                    AbrirVentanaJuegoInvitado?.Invoke(sala, salasServicio, nombreInvitado);
                };

                if (MostrarIngresoInvitado == null)
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida);
                    salasServicio.Dispose();
                    return Task.CompletedTask;
                }

                MostrarIngresoInvitado(vistaModelo);

                if (!vistaModelo.SeUnioSala)
                {
                    salasServicio.Dispose();
                }
            }
            catch (Exception)
            {
                salasServicio?.Dispose();
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoNoEncuentraPartida);
            }

            return Task.CompletedTask;
        }

        private async Task IniciarSesionAsync()
        {
            var (esValido, identificadorTrimmed) = ValidarEntradasYMostrarErrores();
            if (!esValido)
            {
                ManejadorSonido.ReproducirError();
                return;
            }

            EstaProcesando = true;

            try
            {
                var solicitud = new DTOs.CredencialesInicioSesionDTO
                {
                    Identificador = identificadorTrimmed,
                    Contrasena = _contrasena
                };

                DTOs.ResultadoInicioSesionDTO resultado = await _inicioSesionServicio
                    .IniciarSesionAsync(solicitud).ConfigureAwait(true);

                ProcesarResultadoInicioSesion(resultado);
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorInicioSesion);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private (bool EsValido, string IdentificadorTrimmed) ValidarEntradasYMostrarErrores()
        {
            string identificador = Identificador?.Trim();
            bool identificadorIngresado = !string.IsNullOrWhiteSpace(identificador);
            bool contrasenaIngresada = !string.IsNullOrWhiteSpace(_contrasena);

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            var camposInvalidos = new List<string>();

            if (!identificadorIngresado)
            {
                camposInvalidos.Add(nameof(Identificador));
            }

            if (!contrasenaIngresada)
            {
                camposInvalidos.Add(CampoContrasena);
            }

            if (camposInvalidos.Any())
            {
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                AvisoAyudante.Mostrar(Lang.errorTextoCamposInvalidosGenerico);
                return (false, null);
            }

            return (true, identificador);
        }

        private void ProcesarResultadoInicioSesion(DTOs.ResultadoInicioSesionDTO resultado)
        {
            if (resultado == null)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoServidorInicioSesion);
                return;
            }

            if (!resultado.InicioSesionExitoso)
            {
                ManejadorSonido.ReproducirError();
                MostrarErrorInicioSesion(resultado);
                return;
            }

            if (resultado.Usuario != null)
            {
                SesionUsuarioActual.EstablecerUsuario(resultado.Usuario);
            }

            ManejadorSonido.ReproducirExito();
            InicioSesionCompletado?.Invoke(resultado);
            CerrarAccion?.Invoke();
        }

        private static void MostrarErrorInicioSesion(DTOs.ResultadoInicioSesionDTO resultado)
        {
            string mensaje = resultado?.Mensaje;

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = Lang.errorTextoCredencialesIncorrectas;
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                AvisoAyudante.Mostrar(mensaje);
            }
        }

        private async Task RecuperarCuentaAsync()
        {
            string identificador = Identificador?.Trim();

            if (string.IsNullOrWhiteSpace(identificador))
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(Lang.errorTextoIdentificadorRecuperacionRequerido);
                return;
            }

            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _recuperacionCuentaDialogoServicio
                    .RecuperarCuentaAsync(identificador, _cambioContrasenaServicio).ConfigureAwait(true);

                if (resultado?.OperacionExitosa == false && !string.IsNullOrWhiteSpace(resultado.Mensaje))
                {
                    ManejadorSonido.ReproducirError();
                    AvisoAyudante.Mostrar(resultado.Mensaje);
                }
            }
            catch (ServicioExcepcion ex)
            {
                ManejadorSonido.ReproducirError();
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorSolicitudCambioContrasena);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void CargarIdiomas()
        {
            WeakEventManager<ILocalizacionServicio, EventArgs>.AddHandler(
                _localizacionServicio,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServicioEnIdiomaActualizado);

            ActualizarIdiomasDisponibles(_localizacionServicio.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name);
        }

        private void LocalizacionServicioEnIdiomaActualizado(object sender, EventArgs e)
        {
            ActualizarIdiomasDisponibles(_localizacionServicio.CulturaActual?.Name);
        }

        private void ActualizarIdiomasDisponibles(string culturaActual)
        {
            var opciones = new[]
            {
                new IdiomaOpcion("es-MX", Lang.idiomaTextoEspañol),
                new IdiomaOpcion("en-US", Lang.idiomaTextoIngles)
            };

            if (IdiomasDisponibles == null)
            {
                IdiomasDisponibles = new ObservableCollection<IdiomaOpcion>(opciones);
            }
            else
            {
                IdiomasDisponibles.Clear();

                foreach (var opcion in opciones)
                {
                    IdiomasDisponibles.Add(opcion);
                }
            }

            if (string.IsNullOrWhiteSpace(culturaActual))
            {
                IdiomaSeleccionado = IdiomasDisponibles.FirstOrDefault();
                return;
            }

            IdiomaSeleccionado = IdiomasDisponibles
                .FirstOrDefault(i => string.Equals(i.Codigo, culturaActual, StringComparison.OrdinalIgnoreCase))
                ?? IdiomasDisponibles.FirstOrDefault();
        }
    }
}