using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PictionaryMusicalCliente.Comandos;
using PictionaryMusicalCliente.Modelo;
using PictionaryMusicalCliente.Properties.Langs;
using PictionaryMusicalCliente.Servicios;
using PictionaryMusicalCliente.Servicios.Abstracciones;
using PictionaryMusicalCliente.Utilidades;

namespace PictionaryMusicalCliente.VistaModelo.Amigos
{
    public class BuscarAmigoVistaModelo : BaseVistaModelo
    {
        private readonly IAmigosService _amigosService;

        private string _nombreUsuario;
        private bool _estaEnviando;

        public BuscarAmigoVistaModelo(IAmigosService amigosService)
        {
            _amigosService = amigosService ?? throw new ArgumentNullException(nameof(amigosService));

            EnviarSolicitudCommand = new ComandoAsincrono(() => EnviarSolicitudAsync(), () => !EstaEnviando);
            CancelarCommand = new ComandoDelegado(_ => Cancelar());
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => EstablecerPropiedad(ref _nombreUsuario, value);
        }

        public bool EstaEnviando
        {
            get => _estaEnviando;
            private set
            {
                if (EstablecerPropiedad(ref _estaEnviando, value))
                {
                    ((IComandoNotificable)EnviarSolicitudCommand).NotificarPuedeEjecutar();
                }
            }
        }

        public IComandoAsincrono EnviarSolicitudCommand { get; }

        public ICommand CancelarCommand { get; }

        public Action CerrarAccion { get; set; }

        public Action<string> MostrarMensaje { get; set; }

        public Action<bool> MarcarCampoUsuarioInvalido { get; set; }

        private async Task EnviarSolicitudAsync()
        {
            string nombreUsuario = NombreUsuario?.Trim();

            MarcarCampoUsuarioInvalido?.Invoke(false);

            ResultadoOperacion validacion = ValidacionEntradaHelper.ValidarUsuario(nombreUsuario);

            if (validacion == null || !validacion.Exito)
            {
                MarcarCampoUsuarioInvalido?.Invoke(true);
                MostrarMensajeVista(validacion?.Mensaje ?? Lang.errorTextoErrorProcesarSolicitud);
                return;
            }

            EstaEnviando = true;

            try
            {
                ResultadoOperacion resultado = await _amigosService.EnviarSolicitudAsync(nombreUsuario).ConfigureAwait(true);

                if (resultado?.Exito == true)
                {
                    string mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                        ? Lang.amigosTextoSolicitudEnviada
                        : resultado.Mensaje;

                    MostrarMensajeVista(mensaje);
                    CerrarAccion?.Invoke();
                    return;
                }

                string mensajeError = string.IsNullOrWhiteSpace(resultado?.Mensaje)
                    ? Lang.errorTextoErrorProcesarSolicitud
                    : resultado.Mensaje;

                MostrarMensajeVista(mensajeError);
            }
            catch (ServicioException ex)
            {
                string mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                    ? Lang.errorTextoServidorNoDisponible
                    : ex.Message;

                MostrarMensajeVista(mensajeError);
            }
            catch (Exception)
            {
                MostrarMensajeVista(Lang.errorTextoErrorProcesarSolicitud);
            }
            finally
            {
                EstaEnviando = false;
            }
        }

        private void Cancelar()
        {
            CerrarAccion?.Invoke();
        }

        private void MostrarMensajeVista(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = Lang.errorTextoErrorProcesarSolicitud;
            }

            (MostrarMensaje ?? AvisoHelper.Mostrar)?.Invoke(mensaje);
        }
    }
}
