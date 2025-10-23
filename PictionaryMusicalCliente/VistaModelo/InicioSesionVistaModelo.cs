using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PictionaryMusicalCliente.ClienteServicios.Abstracciones;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;
using DTOs = global::Servicios.Contratos.DTOs;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class InicioSesionVistaModelo : BaseVistaModelo
    {
        private readonly IInicioSesionServicio _inicioSesionService;
        private readonly ICambioContrasenaServicio _cambioContrasenaService;
        private readonly IRecuperacionCuentaServicio _recuperacionCuentaDialogService;
        private readonly ILocalizacionServicio _localizacionService;

        public const string CampoContrasena = "Contrasena";

        private string _identificador;
        private string _contrasena;
        private bool _estaProcesando;
        private ObservableCollection<IdiomaOpcion> _idiomasDisponibles;
        private IdiomaOpcion _idiomaSeleccionado;

        public InicioSesionVistaModelo(
            IInicioSesionServicio inicioSesionService,
            ICambioContrasenaServicio cambioContrasenaService,
            IRecuperacionCuentaServicio recuperacionCuentaDialogService,
            ILocalizacionServicio localizacionService)
        {
            _inicioSesionService = inicioSesionService ?? throw new ArgumentNullException(nameof(inicioSesionService));
            _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            _recuperacionCuentaDialogService = recuperacionCuentaDialogService ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogService));
            _localizacionService = localizacionService ?? throw new ArgumentNullException(nameof(localizacionService));

            IniciarSesionCommand = new ComandoAsincrono(_ => IniciarSesionAsync(), _ => !EstaProcesando);
            RecuperarCuentaCommand = new ComandoAsincrono(_ => RecuperarCuentaAsync(), _ => !EstaProcesando);
            AbrirCrearCuentaCommand = new ComandoDelegado(_ => AbrirCrearCuenta?.Invoke());
            IniciarSesionInvitadoCommand = new ComandoDelegado(_ => IniciarSesionInvitado?.Invoke());

            CargarIdiomas();
        }

        public string Identificador
        {
            get => _identificador;
            set => EstablecerPropiedad(ref _identificador, value);
        }

        public ObservableCollection<IdiomaOpcion> IdiomasDisponibles
        {
            get => _idiomasDisponibles;
            private set => EstablecerPropiedad(ref _idiomasDisponibles, value);
        }

        public IdiomaOpcion IdiomaSeleccionado
        {
            get => _idiomaSeleccionado;
            set
            {
                if (EstablecerPropiedad(ref _idiomaSeleccionado, value) && value != null)
                {
                    _localizacionService.EstablecerIdioma(value.Codigo);
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
                    ((IComandoNotificable)IniciarSesionCommand).NotificarPuedeEjecutar();
                    ((IComandoNotificable)RecuperarCuentaCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono IniciarSesionCommand { get; }

        public IComandoAsincrono RecuperarCuentaCommand { get; }

        public ICommand AbrirCrearCuentaCommand { get; }

        public ICommand IniciarSesionInvitadoCommand { get; }

        public Action AbrirCrearCuenta { get; set; }

        public Action IniciarSesionInvitado { get; set; }

        public Action<DTOs.ResultadoInicioSesionDTO> InicioSesionCompletado { get; set; }

        public Action CerrarAccion { get; set; }

        public Action<IList<string>> MostrarCamposInvalidos { get; set; }

        public void EstablecerContrasena(string contrasena)
        {
            _contrasena = contrasena;
        }

        private async Task IniciarSesionAsync()
        {
            string identificador = Identificador?.Trim();
            bool identificadorIngresado = !string.IsNullOrWhiteSpace(identificador);
            bool contrasenaIngresada = !string.IsNullOrWhiteSpace(_contrasena);

            MostrarCamposInvalidos?.Invoke(Array.Empty<string>());

            List<string> camposInvalidos = null;

            if (!identificadorIngresado)
            {
                camposInvalidos ??= new List<string>();
                camposInvalidos.Add(nameof(Identificador));
            }

            if (!contrasenaIngresada)
            {
                camposInvalidos ??= new List<string>();
                camposInvalidos.Add(CampoContrasena);
            }

            if (camposInvalidos != null)
            {
                MostrarCamposInvalidos?.Invoke(camposInvalidos);
                AvisoAyudante.Mostrar(Lang.errorTextoCamposInvalidosGenerico);
                return;
            }

            bool credencialesCapturadas = identificadorIngresado && contrasenaIngresada;

            EstaProcesando = true;

            try
            {
                var solicitud = new DTOs.CredencialesInicioSesionDTO
                {
                    Identificador = identificador,
                    Contrasena = _contrasena
                };

                DTOs.ResultadoInicioSesionDTO resultado = await _inicioSesionService
                    .IniciarSesionAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoAyudante.Mostrar(Lang.errorTextoServidorInicioSesion);
                    return;
                }

                if (!resultado.InicioSesionExitoso)
                {
                    string mensaje = resultado.CuentaNoEncontrada
                        ? Lang.errorTextoCuentaNoEncontradaInicioSesion
                        : resultado.Mensaje;

                    if (string.IsNullOrWhiteSpace(mensaje) && credencialesCapturadas)
                    {
                        mensaje = Lang.errorTextoCredencialesIncorrectas;
                    }

                    if (!string.IsNullOrWhiteSpace(mensaje))
                    {
                        AvisoAyudante.Mostrar(mensaje);
                    }

                    return;
                }

                if (resultado.Usuario != null)
                {
                    SesionUsuarioActual.Instancia.EstablecerUsuario(resultado.Usuario);
                }

                InicioSesionCompletado?.Invoke(resultado);
                CerrarAccion?.Invoke();
            }
            catch (ExcepcionServicio ex)
            {
                AvisoAyudante.Mostrar(ex.Message ?? Lang.errorTextoServidorInicioSesion);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private async Task RecuperarCuentaAsync()
        {
            string identificador = Identificador?.Trim();

            if (string.IsNullOrWhiteSpace(identificador))
            {
                AvisoAyudante.Mostrar(Lang.errorTextoIdentificadorRecuperacionRequerido);
                return;
            }

            EstaProcesando = true;

            try
            {
                DTOs.ResultadoOperacionDTO resultado = await _recuperacionCuentaDialogService
                    .RecuperarCuentaAsync(identificador, _cambioContrasenaService).ConfigureAwait(true);

                if (resultado?.OperacionExitosa == false && !string.IsNullOrWhiteSpace(resultado.Mensaje))
                {
                    AvisoAyudante.Mostrar(resultado.Mensaje);
                }
            }
            catch (ExcepcionServicio ex)
            {
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
                _localizacionService,
                nameof(ILocalizacionServicio.IdiomaActualizado),
                LocalizacionServiceOnIdiomaActualizado);

            ActualizarIdiomasDisponibles(_localizacionService.CulturaActual?.Name
                ?? CultureInfo.CurrentUICulture?.Name);
        }

        private void LocalizacionServiceOnIdiomaActualizado(object sender, EventArgs e)
        {
            ActualizarIdiomasDisponibles(_localizacionService.CulturaActual?.Name);
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
