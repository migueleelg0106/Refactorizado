using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Sesiones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Cuentas
{
    public class BuscarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;
        private string _usuarioBuscado;
        private bool _estaProcesando;

        public BuscarAmigoVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            EnviarSolicitudCommand = new ComandoAsincrono(_ => EnviarSolicitudAsync(), _ => !EstaProcesando);
            CancelarCommand = new ComandoDelegado(_ => CerrarAccion?.Invoke());
        }

        public string UsuarioBuscado
        {
            get => _usuarioBuscado;
            set => EstablecerPropiedad(ref _usuarioBuscado, value);
        }

        public bool EstaProcesando
        {
            get => _estaProcesando;
            private set
            {
                if (EstablecerPropiedad(ref _estaProcesando, value))
                {
                    ((IComandoNotificable)EnviarSolicitudCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EnviarSolicitudCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public Action<bool> MarcarUsuarioInvalido { get; set; }

        private async Task EnviarSolicitudAsync()
        {
            string usuarioObjetivo = UsuarioBuscado?.Trim();
            string usuarioActual = SesionUsuarioActual.Instancia.Usuario?.NombreUsuario;

            MarcarUsuarioInvalido?.Invoke(false);

            ResultadoOperacion validacion = ValidacionEntradaHelper.ValidarUsuario(usuarioObjetivo);
            if (!validacion.Exito)
            {
                MarcarUsuarioInvalido?.Invoke(true);
                MostrarMensaje?.Invoke(validacion.Mensaje ?? Lang.errorTextoCampoObligatorio);
                return;
            }

            if (string.IsNullOrWhiteSpace(usuarioActual))
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            if (string.Equals(usuarioActual, usuarioObjetivo, StringComparison.OrdinalIgnoreCase))
            {
                MarcarUsuarioInvalido?.Invoke(true);
                MostrarMensaje?.Invoke(Lang.buscarAmigoErrorSolicitudMismoUsuario);
                return;
            }

            EstaProcesando = true;

            try
            {
                ResultadoOperacion resultado = await _amigosService
                    .EnviarSolicitudAmistadAsync(usuarioActual, usuarioObjetivo)
                    .ConfigureAwait(true);

                if (resultado == null)
                {
                    MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
                    return;
                }

                string mensaje = resultado.Mensaje;

                if (!resultado.Exito)
                {
                    MarcarUsuarioInvalido?.Invoke(true);
                }

                MostrarMensaje?.Invoke(string.IsNullOrWhiteSpace(mensaje)
                    ? (resultado.Exito
                        ? Lang.buscarAmigoTextoSolicitudEnviada
                        : Lang.errorTextoErrorProcesarSolicitud)
                    : mensaje);

                if (resultado.Exito)
                {
                    CerrarAccion?.Invoke();
                }
            }
            catch (ServicioException ex)
            {
                MostrarMensaje?.Invoke(ex.Message ?? Lang.errorTextoErrorProcesarSolicitud);
            }
            catch (Exception)
            {
                MostrarMensaje?.Invoke(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaProcesando = false;
            }
        }
    }
}
