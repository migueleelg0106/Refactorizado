using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Modelo.Cuentas;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class InicioSesionVistaModelo : BaseVistaModelo
    {
        private readonly IInicioSesionService _inicioSesionService;
        private readonly ICambioContrasenaService _cambioContrasenaService;
        private readonly IRecuperacionCuentaDialogService _recuperacionCuentaDialogService;

        private string _identificador;
        private string _contrasena;
        private bool _estaProcesando;
        private ObservableCollection<IdiomaOpcion> _idiomasDisponibles;
        private IdiomaOpcion _idiomaSeleccionado;

        public InicioSesionVistaModelo(
            IInicioSesionService inicioSesionService,
            ICambioContrasenaService cambioContrasenaService,
            IRecuperacionCuentaDialogService recuperacionCuentaDialogService)
        {
            _inicioSesionService = inicioSesionService ?? throw new ArgumentNullException(nameof(inicioSesionService));
            _cambioContrasenaService = cambioContrasenaService ?? throw new ArgumentNullException(nameof(cambioContrasenaService));
            _recuperacionCuentaDialogService = recuperacionCuentaDialogService ?? throw new ArgumentNullException(nameof(recuperacionCuentaDialogService));

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
                    CultureInfo cultura = new CultureInfo(value.Codigo);
                    Lang.Culture = cultura;
                    CultureInfo.DefaultThreadCurrentCulture = cultura;
                    CultureInfo.DefaultThreadCurrentUICulture = cultura;
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

        public Action<ResultadoInicioSesion> InicioSesionCompletado { get; set; }

        public Action CerrarAccion { get; set; }

        public void EstablecerContrasena(string contrasena)
        {
            _contrasena = contrasena;
        }

        private async Task IniciarSesionAsync()
        {
            string identificador = Identificador?.Trim();

            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(_contrasena))
            {
                AvisoHelper.Mostrar(Lang.errorTextoCredencialesIncorrectas);
                return;
            }

            EstaProcesando = true;

            try
            {
                var solicitud = new SolicitudInicioSesion
                {
                    Identificador = identificador,
                    Contrasena = _contrasena
                };

                ResultadoInicioSesion resultado = await _inicioSesionService
                    .IniciarSesionAsync(solicitud).ConfigureAwait(true);

                if (resultado == null)
                {
                    AvisoHelper.Mostrar(Lang.errorTextoServidorInicioSesion);
                    return;
                }

                if (!resultado.InicioSesionExitoso)
                {
                    string mensaje = resultado.Mensaje ?? Lang.errorTextoCredencialesIncorrectas;
                    AvisoHelper.Mostrar(mensaje);
                    return;
                }

                if (resultado.Usuario != null)
                {
                    SesionUsuarioActual.Instancia.EstablecerUsuario(resultado.Usuario);
                }

                InicioSesionCompletado?.Invoke(resultado);
                CerrarAccion?.Invoke();
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoServidorInicioSesion);
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
                AvisoHelper.Mostrar(Lang.errorTextoIdentificadorRecuperacionRequerido);
                return;
            }

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _recuperacionCuentaDialogService
                    .RecuperarCuentaAsync(identificador, _cambioContrasenaService).ConfigureAwait(true);

                if (resultado?.Exito == false && !string.IsNullOrWhiteSpace(resultado.Mensaje))
                {
                    AvisoHelper.Mostrar(resultado.Mensaje);
                }
            }
            catch (ServicioException ex)
            {
                AvisoHelper.Mostrar(ex.Message ?? Lang.errorTextoServidorSolicitudCambioContrasena);
            }
            finally
            {
                EstaProcesando = false;
            }
        }

        private void CargarIdiomas()
        {
            IdiomasDisponibles = new ObservableCollection<IdiomaOpcion>
            {
                new IdiomaOpcion("es-MX", "Español"),
                new IdiomaOpcion("en-US", "English")
            };

            IdiomaSeleccionado = IdiomasDisponibles[0];
        }
    }
}
